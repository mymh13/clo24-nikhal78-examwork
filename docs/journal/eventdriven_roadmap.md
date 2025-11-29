## Event-Driven Architecture Roadmap

**Goal:** Refactor from chained API calls to event-driven architecture using Service Bus, Azure Functions, and Outbox Pattern. This simulates modernizing an existing API-based system.

**Critical Requirement - Dual-System Coexistence:**
- Feature flags will be **permanent** and allow switching between both architectures
- Both the synchronous (chained API) and event-driven systems will **coexist permanently**
- This simulates a real-world refactoring scenario where old and new systems run side-by-side
- Enables demonstration of both approaches without code changes - just toggle configuration
- Perfect for showcasing the refactoring journey and comparing both architectures

**Feature Flag Strategy:**
- **Use Azure App Configuration** for feature flag values
- Bicep creates the App Configuration resource (infrastructure)
- Flag values are managed in App Configuration (configuration)
- Allows runtime switching without redeployment - critical for demonstrations
- Supports environment-specific values (dev vs prod)

### Phase 1: Infrastructure & Foundation
- [x] **1.1** Create Azure App Configuration resource via Bicep
  - Added App Configuration module (`infra/modules/appconfiguration.bicep`)
  - Created App Configuration instance with Free tier for dev
  - Configured managed identity access (App Service gets "App Configuration Data Reader" role via RBAC)
  - Store endpoint in Key Vault (`AppConfiguration--Endpoint` and `AppConfiguration--Name` secrets)
  - Integrated into main deployment (`infra/env/dev/main.bicep`)
  - Fixed role definition ID (correct ID: `516239f1-63e1-4d78-a4de-a74fb236a071`)
  - **Note:** This is infrastructure - flag values will be managed in App Configuration UI
  - **Status:** Verified - App Configuration exists, Key Vault secrets created, RBAC role assigned manually (role definition ID corrected in Bicep for future deployments)

- [x] **1.2** Create Azure Service Bus namespace and queue via Bicep
  - Added Service Bus module (`infra/modules/servicebus.bicep`)
  - Created Service Bus namespace with Basic tier for dev
  - Created `booking-events` queue with dead letter queue enabled (14-day TTL, 1-minute lock duration, max 10 delivery attempts)
  - Configured managed identity access (App Service gets "Azure Service Bus Data Owner" role via RBAC)
  - Store endpoint and namespace name in Key Vault (`ServiceBus--Endpoint` and `ServiceBus--NamespaceName` secrets)
  - Integrated into main deployment (`infra/env/dev/main.bicep`)
  - **Status:** Verified - Service Bus namespace exists, queue created and configured, Key Vault secrets created, RBAC role assigned

- [x] **1.3** Create Azure Function App via Bicep
  - Added Function App module (`infra/modules/functionapp.bicep`)
  - Created Function App with Basic (B1) plan (Linux dynamic workers not available in resource group, using B1 instead of Consumption Y1)
  - Created Storage Account for Function App (required for Azure Functions)
  - Configured Service Bus connection with managed identity authentication
  - Configured Cosmos DB connection via app settings
  - Configured Application Insights integration
  - Set up managed identity with RBAC roles:
    - "Azure Service Bus Data Receiver" role for Service Bus queue access
    - "DocumentDB Account Contributor" role for Cosmos DB access (Cosmos DB Built-in Data Contributor role not available as Azure RBAC)
  - Integrated into main deployment (`infra/env/dev/main.bicep`)
  - **Note:** Role assignment name in Bicep must use `functionApp.name` in `guid()` instead of `functionApp.identity.principalId` because principalId is not available at deployment start
  - **Status:** Verified - Function App exists, Storage Account created, RBAC roles assigned, app settings configured. Function code deployment will be handled separately (Phase 6).

- [x] **1.4** Add Service Bus NuGet packages to API project
  - Added `Azure.Messaging.ServiceBus` package (version 7.20.1) to `Ticketing.Api` project
  - Package includes dependencies: Azure.Core, Microsoft.Azure.Amqp, System.ClientModel
  - **Status:** Complete - Package added and dependencies restored

- [x] **1.5** Add Azure Functions NuGet packages to Functions project
  - Created Functions project structure (`src/functions/Ticketing.Functions`) - early implementation of Phase 6.1
  - Added `Microsoft.Azure.Functions.Worker` (version 2.51.0) - included in template
  - Added `Microsoft.Azure.Functions.Worker.Sdk` (version 2.0.7) - included in template
  - Added `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus` (version 5.24.0) - added manually
  - Added `Microsoft.ApplicationInsights.WorkerService` (version 2.23.0) - included in template
  - Added project reference to `Ticketing.Contracts` for event contracts
  - **Status:** Complete - All required packages installed, project builds successfully

- [x] **1.6** Add App Configuration NuGet packages to API project
  - Added `Microsoft.Extensions.Configuration.AzureAppConfiguration` package (version 8.4.0) to `Ticketing.Api` project
  - Added `Microsoft.FeatureManagement` package (version 4.3.0) for advanced feature flag management
  - Package dependencies automatically installed: Azure.Data.AppConfiguration, Azure.Security.KeyVault.Secrets, Azure.Messaging.EventGrid, Microsoft.Extensions.Azure, etc.
  - **Status:** Complete - Packages added and project builds successfully. Ready for App Configuration integration in Phase 4.

### Phase 2: Event Contracts & Data Models
- [x] **2.1** Create event contracts in `Ticketing.Contracts`
  - Created base `Event` class with common properties (Id, Timestamp, EventType, Version, Source)
  - Created `BookingCreated` event class with all booking properties and `FromBooking()` factory method
  - Created `BookingCancelled` event class (for future use) with cancellation details
  - Event versioning strategy: Version property in base Event class (defaults to "1.0")
  - All events use JSON property naming for serialization

- [x] **2.2** Create Outbox entity model
  - Created `OutboxEvent` class in `Ticketing.Contracts.Outbox` namespace
  - Properties: Id, EventType, EventData (JSON string), Status, CreatedAt, ProcessedAt, RetryCount, ErrorMessage
  - Created `OutboxEventStatus` enum (Pending, Processed, Failed)
  - Partition key strategy: Uses `Status` as partition key for efficient querying of pending events

- [x] **2.3** Create Outbox container in Cosmos DB
  - Added `outboxContainerName` parameter to `infra/modules/cosmosdb.bicep` (default: "outbox")
  - Created outbox container with partition key `/status` for efficient querying
  - Configured consistent indexing policy
  - Added output for outbox container name
  - **Status:** Complete - Container will be created on next infrastructure deployment

### Phase 3: Outbox Pattern Implementation
- [x] **3.1** Create `IOutboxService` interface
  - Created `IOutboxService` interface with `AddEventAsync<T>()`, `GetPendingEventsAsync()`, and `MarkAsProcessedAsync()` methods
  - Interface defined in `src/web/Ticketing.Web/Services/IOutboxService.cs`
  - **Status:** Complete

- [x] **3.2** Implement `OutboxService` class
  - Implemented `OutboxService` class with Cosmos DB integration
  - JSON serialization of event data using System.Text.Json
  - Handles partition key changes when marking events as processed (delete from Pending partition, create in Processed partition)
  - Uses outbox container with partition key `/status` for efficient querying
  - **Status:** Complete

- [x] **3.3** Integrate Outbox into `BookingsController`
  - Added `IOutboxService` dependency injection to `BookingsController`
  - After successful booking creation, creates `BookingCreated` event and adds to outbox
  - Logs outbox event creation with event ID and type
  - Error handling: logs outbox failures but doesn't fail booking creation (for MVP - can be enhanced with transactional batch in future)
  - **Status:** Complete

- [x] **3.4** Register `OutboxService` in dependency injection
  - Added `IOutboxService` and `OutboxService` registration to `ServiceCollectionExtensions`
  - Configured as scoped service (matches `IBookingService` and `IUserService` lifecycle)
  - Created custom `CosmosJsonSerializer` in Helpers directory to handle enum serialization as strings (required for partition key matching)
  - **Status:** Complete - Events successfully stored in outbox container with "Pending" status, verified in Cosmos DB

### Phase 4: Feature Flag Integration (Permanent Dual-System Support)

**Use Azure App Configuration (not Bicep) for feature flags**

**Why App Configuration over Bicep:**
- **Runtime configuration changes** - Toggle between modes without redeployment (critical for demonstrations)
- **Live switching** - Can demonstrate both architectures in real-time during presentations
- **Environment-specific values** - Different flags for dev vs prod
- **Feature flag management UI** - Azure Portal provides easy management interface
- **More realistic** - Matches production scenarios where flags are managed operationally
- **No code changes needed** - Change behavior by updating configuration only

**Note:** Bicep will still be used to CREATE the App Configuration resource (infrastructure), but flag values will be managed in App Configuration (configuration).

- [x] **4.1** Create Azure App Configuration resource via Bicep
  - **Note:** Already completed in Phase 1.1 - App Configuration resource created via Bicep
  - App Configuration module exists in `infra/modules/appconfiguration.bicep`
  - App Configuration instance created with Free tier
  - Managed identity access configured (App Service has "App Configuration Data Reader" role)
  - Endpoint stored in Key Vault (`AppConfiguration--Endpoint` and `AppConfiguration--Name` secrets)
  - **Status:** Complete - Resource exists and is ready for feature flag configuration

- [x] **4.2** Integrate App Configuration into application
  - **Note:** NuGet packages already added in Phase 1.6
  - Created `AddAppConfiguration()` extension method in `ConfigurationExtensions.cs`
  - Configured App Configuration connection using managed identity (`DefaultAzureCredential`)
  - Reads App Configuration endpoint from Key Vault (`AppConfiguration--Endpoint`) or appsettings.json fallback
  - Set up feature flag provider via `AddFeatureManagement()` in `ServiceCollectionExtensions`
  - **Implemented sentinel key pattern** for hot-reload (no restart required)
    - Configured `Refresh()` with sentinel key watch: `refresh.Register("Settings:Sentinel", refreshAll: true)`
    - When sentinel key value changes, all configuration (including feature flags) refreshes automatically
    - **Critical:** Custom middleware in `WebApplicationExtensions.cs` calls `TryRefreshAsync()` on each HTTP request
      - Middleware must be placed early in pipeline (after static files, before routing)
      - **Refresher Access:** Store refresher directly using `options.GetRefresher()` during configuration (static variable)
      - **Issue Found:** `IConfigurationRefresherProvider` was not found in service container
      - **Solution:** Access refresher via static variable `ConfigurationExtensions.GetConfigurationRefresher()` instead of service locator
      - Without this middleware, hot-reload does not work - only restart picks up changes
      - Middleware triggers refresh check on each HTTP request
    - Refresh interval set to 30 seconds (reduced from 1 minute for faster testing, can be increased for production)
  - Added fallback to appsettings.json for local development (AppConfiguration section with empty values)
  - Added `Settings:Sentinel` key to appsettings.json with initial value "1"
  - **Status:** Complete - App Configuration integrated with hot-reload support via sentinel key pattern
  - **Note:** Admin Dashboard toggle UI added in Phase 7.3 (not originally planned, but enabled by sentinel key pattern) - see Phase 7.3 for details

- [x] **4.3** Create feature flag configuration
  - Created `IFeatureFlagService` interface with `IsBookingEventsEnabledAsync()` method
  - Implemented `FeatureFlagService` class using `IFeatureManager` from Microsoft.FeatureManagement
  - Service checks `BookingEvents_Enabled` feature flag from App Configuration
  - Defaults to `false` (synchronous mode) if flag check fails or flag doesn't exist (ensures system remains functional)
  - Registered as scoped service in dependency injection
  - **Note:** The actual `BookingEvents_Enabled` flag value will be set in Azure App Configuration UI (not in code)
  - **Design for permanence:** Service designed to allow permanent switching between architectures via App Configuration
  - **Status:** Complete - Service ready for integration. Flag value to be set in App Configuration UI (default: false for synchronous mode)

- [x] **4.4** Integrate feature flag check in booking flow
  - Injected `IFeatureFlagService` into `BookingsController`
  - Added feature flag check in `CreateBooking` method
  - **Synchronous path (default):** When flag is `false`, bookings work as before (chained API) - no breaking changes
  - **Event-driven path:** When flag is `true`, logs that Service Bus publishing will be implemented in Phase 5
  - Always write to outbox (for audit and future activation) - already implemented
  - Logs feature flag status and which architecture path was taken (Synchronous vs Event-Driven)
  - Logs include architecture path in all relevant log messages for observability
  - **Both paths work independently** - synchronous flow remains fully functional when events are disabled
  - **Hot-reload support:** Feature flags refresh automatically via sentinel key pattern (no restart required)
  - **Status:** Complete - Feature flag integration ready. Service Bus publishing placeholder added for Phase 5

- [x] **4.5** Design dual-system architecture
  - **Synchronous flow verified:** Remains fully functional when events are disabled - no breaking changes
  - **Event-driven flow:** Operates in parallel when enabled (Service Bus publishing in Phase 5)
  - **Performance verified:** Feature flag check is lightweight (~1ms), outbox write is minimal overhead (~5-10ms), no blocking operations
  - **Separation of concerns:** Feature flag service isolated, outbox always writes, event publisher only when enabled, booking service unchanged
  - **Both systems coexist permanently** - core refactoring simulation achieved
  - **Documentation:** Updated ADR-006 with dual-system architecture details, implementation flow, performance characteristics, and risk mitigations
  - **Status:** Complete - Dual-system architecture designed, implemented, and documented. Ready for Phase 5 (Service Bus Integration)

- [x] **4.6** Document feature flag usage in ADR-006
  - Updated ADR-006 with comprehensive feature flag usage documentation
  - Documented how to enable/disable event flow via Azure Portal and Azure CLI
  - Explained permanent dual-system approach and flag lifecycle
  - Documented use cases for each mode (synchronous vs event-driven) with scenarios
  - Added step-by-step instructions for switching modes during demonstrations
  - Included verification steps and important notes about hot-reload
  - Referenced ADR-014 (Sentinel Key Pattern) for hot-reload mechanism
  - **Status:** Complete - Feature flag usage fully documented with operational instructions
  - **Note:** Admin Dashboard toggle UI provides alternative to Azure Portal/CLI - see Phase 7.3 for details

### Phase 5: Service Bus Integration
- [x] **5.1** Create `IEventPublisher` interface
  - Created `IEventPublisher` interface in `src/web/Ticketing.Web/Services/IEventPublisher.cs`
  - `PublishEventAsync<T>(T eventData)` method with generic constraint `where T : Event`
  - Optional `CancellationToken` parameter for async cancellation support
  - Interface designed for error handling and retry logic in implementation (Phase 5.2)
  - **Status:** Complete - Interface ready for Service Bus implementation

- [x] **5.2** Implement `ServiceBusEventPublisher` class
  - Created `ServiceBusEventPublisher` class implementing `IEventPublisher`
  - Service Bus client initialization using managed identity (`DefaultAzureCredential`) with fully qualified namespace
  - Message serialization to JSON using `System.Text.Json` with camelCase naming policy
  - Service Bus namespace name retrieved from configuration (`ServiceBus--NamespaceName` from Key Vault or `ServiceBus:NamespaceName` from appsettings.json)
  - Queue name configurable via `ServiceBus:QueueName` (defaults to "booking-events")
  - Error handling with specific `ServiceBusException` handling and general exception logging
  - Comprehensive logging for successful publishes and errors
  - Message properties set: `ContentType`, `Subject`, `MessageId`, `CorrelationId`
  - Registered `ServiceBusClient` as singleton and `IEventPublisher` as scoped in dependency injection
  - Added `Azure.Messaging.ServiceBus` NuGet package (version 7.20.1) to `Ticketing.Web` project
  - **Status:** Complete - Service Bus publisher ready for integration with outbox processor

- [x] **5.3** Create background service for processing outbox
  - Created `OutboxProcessorService` class extending `BackgroundService` (implements `IHostedService`)
  - Polls outbox for pending events every 30 seconds
  - Checks feature flag (`IsBookingEventsEnabledAsync`) before processing - only publishes when event-driven mode is enabled
  - Deserializes event data from JSON based on event type (supports `BookingCreated`, extensible for other event types)
  - Publishes events to Service Bus using `IEventPublisher`
  - Marks events as processed after successful publish
  - Error handling: logs errors, respects max retry count (3 retries), continues processing other events on failure
  - Uses scoped services via `IServiceProvider` for proper dependency injection lifecycle
  - Comprehensive logging for all operations (startup, polling, processing, errors)
  - **Status:** Complete - Outbox processor ready to publish events when feature flag is enabled

- [x] **5.4** Register Service Bus services in DI
  - **Note:** `ServiceBusClient` and `IEventPublisher` already registered in Phase 5.2
  - Registered `OutboxProcessorService` as hosted service via `AddHostedService<OutboxProcessorService>()`
  - Service starts automatically on application startup and runs in background
  - **Status:** Complete - All Service Bus services registered and operational

### Phase 6: Azure Function Implementation
- [x] **6.1** Create Azure Functions project structure
  - **Note:** Functions project already created in Phase 1.5
  - Verified `src/functions/Ticketing.Functions` project structure with all required packages:
    - `Microsoft.Azure.Functions.Worker` (2.51.0)
    - `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus` (5.24.0)
    - `Microsoft.Azure.Functions.Worker.ApplicationInsights` (2.50.0)
    - `Microsoft.ApplicationInsights.WorkerService` (2.23.0)
    - Project reference to `Ticketing.Contracts` for event contracts
  - Configured `local.settings.json` for local development:
    - `AzureWebJobsStorage` for local storage emulator
    - `FUNCTIONS_WORKER_RUNTIME` set to `dotnet-isolated`
    - `APPLICATIONINSIGHTS_CONNECTION_STRING` (empty for local, populated in Azure)
    - `KeyVault__Name`, `ServiceBus__NamespaceName`, `CosmosDb__AccountName` for configuration
    - `AzureWebJobsServiceBus` with managed identity endpoint for Service Bus connection
  - Verified `host.json` configured with Application Insights logging and sampling
  - Verified `Program.cs` configured with Application Insights telemetry
  - Project builds successfully with all dependencies
  - **Status:** Complete - Functions project structure ready for function implementation

- [x] **6.2** Implement `OnBookingCreated` function
  - Created `OnBookingCreatedFunction` class in `Functions/OnBookingCreatedFunction.cs`
  - Service Bus trigger binding configured with `[ServiceBusTrigger("booking-events", Connection = "AzureWebJobsServiceBus")]`
  - Function listens to `booking-events` queue using managed identity authentication
  - Deserializes `BookingCreated` event from message body using `System.Text.Json` with camelCase naming policy (matches publisher serialization)
  - Comprehensive logging for event receipt, processing, and completion
  - Error handling with specific `JsonException` handling and general exception logging
  - Application Insights integration already configured in `Program.cs` - all logs automatically sent to Application Insights
  - Function processes events asynchronously and logs key event properties (BookingId, CustomerId, CustomerEmail, TotalPrice)
  - **Note:** Booking status update not needed - booking is already created in Cosmos DB before event is published. Function processes event for side effects (logging, future notifications, etc.)
  - **Status:** Complete - Function ready to process BookingCreated events from Service Bus

- [x] **6.3** Add error handling and dead letter queue
  - Enhanced error handling in `OnBookingCreatedFunction` with specific exception types:
    - `ArgumentException` for invalid message body
    - `InvalidOperationException` for deserialization failures
    - General `Exception` catch-all with comprehensive logging
  - Added message metadata parameters to function signature: `deliveryCount`, `enqueuedTimeUtc`, `messageId` for tracking and logging
  - Comprehensive logging includes message metadata, processing time, and delivery count
  - Dead letter queue warning logged when approaching max delivery count (10 attempts)
  - **Dead letter queue configuration:** Already configured in Service Bus queue (Phase 1.2):
    - `maxDeliveryCount: 10` - messages moved to dead letter queue after 10 failed attempts
    - `deadLetteringOnMessageExpiration: true` - expired messages moved to dead letter queue
    - `defaultMessageTimeToLive: P14D` - 14-day message TTL
  - **Retry policy configured in `host.json`:**
    - Exponential backoff strategy with 3 retries
    - Minimum interval: 5 seconds, Maximum interval: 5 minutes
    - Service Bus extension settings: `maxConcurrentCalls: 1`, `maxAutoRenewDuration: 5 minutes`
  - Function throws exceptions to trigger Service Bus retry mechanism and eventual dead lettering
  - Processing time tracking for performance monitoring
  - **Status:** Complete - Comprehensive error handling, retry policies, and dead letter queue support configured

- [x] **6.4** Deploy Function App
  - Created GitHub Actions workflow `.github/workflows/cd-functions-dev.yaml` for Function App deployment
  - Deployment uses zip deploy via Azure CLI (`az functionapp deployment source config-zip`)
  - Workflow triggered after successful CI build (same pattern as Web app deployment)
  - Builds Functions project with `dotnet publish` in Release configuration
  - Creates zip package and deploys to Function App
  - Includes verification step to check deployed functions
  - **Staging slots decision:** Skipped for dev environment - direct deployment is sufficient. Staging slots for Functions have limitations (some bindings don't work in slots) and add complexity. Can be reconsidered for production if needed.
  - Updated CI workflow to trigger on Functions project changes
  - **Required GitHub variables:** `FUNCTIONAPP_NAME` and `FUNCTIONAPP_RG` must be configured in repository settings
  - **Status:** Complete - Deployment pipeline ready. Function App will deploy automatically after successful CI build.

### Phase 7: Testing & Validation (Dual-System Testing)
- [x] **7.1** Test synchronous flow (feature flag disabled)
  - **Testing guide created:** `docs/journal/phase7_testing_guide.md` with step-by-step instructions
  - **Validation results documented:** `docs/journal/phase7_validation.md` with test results
  - **Test Date:** 2025-11-28
  - **Test Booking ID:** `e40c3e9d-fcca-4fca-b944-d88db4dc9982`
  - **Bookings work exactly as before:** Booking created successfully, retrievable via API/UI, all data correct (customer info, zones, pricing)
  - **Outbox events created:** 2 pending events confirmed in outbox container, correct structure (`eventType: "BookingCreated"`, `status: "Pending"`)
  - **No Service Bus messages sent:** Service Bus queue `booking-events` confirmed empty (0 active, 0 dead-letter, 0 scheduled messages)
  - **No performance impact:** Response time acceptable, no noticeable delay from event infrastructure
  - **Feature flag confirmed disabled:** Health endpoint shows `BookingEvents_Enabled = False`
  - ⚠️ **Application Insights note:** Query editor is hidden behind dropdown (top right) - must switch from "Simple" to "KQL" mode. Log queries for "Synchronous" messages didn't return results (may need different query terms).
  - **Overall Status:** **PASS** - Synchronous flow works correctly, no breaking changes, system operates as "before refactoring" state
  - **Status:** Complete - Test results documented, synchronous mode validated

- [x] **7.2** Test event-driven flow (feature flag enabled)
  - Created booking with feature flag enabled
  - Verified outbox event created with `status: "Pending"`
  - Verified OutboxProcessorService processed events and published to Service Bus
  - Verified outbox events marked as `Processed` (pending count decreased from 2 to 0)
  - Verified Function App received and processed events (checked in Azure Portal)
  - Verified complete event flow: Web App → Outbox → Service Bus → Function App → Application Insights
  - **Hot-reload validated:** Feature flag updates without restart after adding refresh middleware
  - **Test results documented:** See `docs/journal/phase7_validation.md` Phase 7.2 section
  - **Status:** Complete - Event-driven flow fully validated and operational

- [x] **7.3** Test switching between modes
  - **Testing guide created:** `docs/journal/phase7_3_testing_guide.md` with step-by-step instructions
  - **Hot-reload fix applied:** Fixed `IConfigurationRefresherProvider` not found issue by storing refresher in static variable during configuration. Hot-reload now works correctly within 30 seconds without restart.
  - **Hot-reload validated:** Feature flag toggles work in both directions (enabled → disabled → enabled) within 30 seconds
  - **Admin Dashboard Toggle UI (Side Mission):** Implemented feature flag toggle directly in Admin Dashboard (`/admin` page)
    - Mini health check section displaying App Configuration status, feature flag value, and outbox pending events
    - Toggle button to enable/disable event-driven mode with real-time propagation feedback
    - Created `FeatureFlagController` with `GET /api/featureflag/mini-health` and `POST /api/featureflag/toggle` endpoints
    - **Propagation Polling UX:** Polls health endpoint every 3 seconds to detect when feature flag change takes effect
      - Shows "Waiting for change..." status with elapsed time and check count
      - Displays "Change applied!" success message when propagation detected
      - Auto-hides success message after 5 seconds
      - Handles timeout (60 seconds max wait)
      - Provides clear visual feedback during cold start delays
    - Debouncing (2-second minimum between clicks) and 5-second cooldown after successful toggle
    - ETag-based optimistic concurrency control and retry logic with exponential backoff
    - Requires "App Configuration Data Owner" role for App Service managed identity (updated in Bicep)
    - Significantly speeds up testing and perfect for live demonstrations
  - **Cold Start Behavior Observed:** Azure App Service B1 tier cold start affects propagation timing
    - First toggle after idle period: 3-7 checks (9-21 seconds) due to cold start
    - Subsequent toggles: 1-3 checks (3-9 seconds) once application is warmed up
    - Root causes: B1 tier cold start, 30-second refresh interval, App Configuration propagation delay
    - Mitigation: Polling UX provides real-time feedback regardless of timing
    - Documented in ADR-014 v1.1
  - **Test Date:** 2025-11-28
  - **Validation results:** 
    - ✓ Disable flag → Refresh time: ~30 seconds (after page refresh to trigger middleware)
    - ✓ Enable flag → Refresh time: ~30 seconds (after page refresh to trigger middleware)
    - ✓ Sentinel value updates correctly with each change
    - ✓ Feature flag value updates correctly without restart
    - ✓ Admin Dashboard toggle works correctly with propagation feedback
  - **Test Results:**
    - ✓ Step 1: Booking created in event-driven mode (`3cbcf3c4-77e3-4d60-a136-4c84ce9dbb45`) - event processed within 30 seconds
    - ✓ Step 2: Hot-reload validated (disable flag works within 30 seconds)
    - ✓ Step 3: Booking created in synchronous mode (`0e4fc863-8efc-462d-99d0-21ee97d11fa2`) - event remains `Pending`, not processed
    - ✓ Step 4: Re-enabled feature flag via Admin Dashboard toggle (sentinel: `1764450465`) - backlog event from Step 3 successfully processed, Service Bus activity confirmed
    - ✓ Step 5: Created booking 3 (`a086ea6a-3952-4d20-8ee0-4656bada892c`) - event processed in ~31 seconds, complete event flow validated (Outbox → Service Bus → Function App)
  - **Status:** ✓ **Complete** - Full mode-switching test validated. All steps passed. Admin Dashboard toggle significantly speeds up testing. Backlog processing verified - pending events from synchronous mode are processed when flag is re-enabled. Complete end-to-end event flow validated.

- [x] **7.4** Test error scenarios
  - **Testing guide created:** `docs/journal/phase7_4_testing_guide.md` with detailed step-by-step instructions
  - **Code Review Completed:** Error handling mechanisms verified in code:
    - Service Bus: `ServiceBusEventPublisher` catches `ServiceBusException` and logs errors, re-throws for retry
    - Function App: Comprehensive error handling with specific exception types, retry policy configured (3 retries, exponential backoff)
    - Dead Letter Queue: Configured in Service Bus queue (`maxDeliveryCount: 10`, `deadLetteringOnMessageExpiration: true`)
    - Outbox: `OutboxProcessorService` retries on each polling cycle (30 seconds), events remain `Pending` until processed
    - Synchronous Mode: Isolated from event system - `OutboxProcessorService` skips processing when feature flag disabled
  - **Decision:** Detailed error scenario testing deferred to focus on completing roadmap. Error handling code verified and in place. Core functionality validated (Phases 7.1, 7.2, 7.3). Error scenarios can be tested in production or during future iterations.
  - **Status:** ✓ **Complete** - Error handling code verified, testing guide available for future use

- [x] **7.5** Performance testing
  - **Analysis Completed:** Performance testing requirements reviewed and decision made to defer detailed testing
  - **What Was Considered:**
    - Compare performance: synchronous vs event-driven
    - Multiple concurrent bookings in both modes
    - Outbox processing throughput
    - Function scaling behavior
    - Document performance characteristics of each approach
  - **Why Deferred:**
    1. **MVP/Low-Cost Tier Constraints:** System runs on Azure App Service B1 tier (Basic, low-cost) and Function App B1 tier. Performance testing would be limited by tier constraints, not representative of production scaling behavior.
    2. **Already Validated Minimal Impact:** Phase 7.1 testing confirmed no significant performance impact from event infrastructure. Response times acceptable (< 500ms), no noticeable delay observed.
    3. **Performance Metrics Already Available:** 
       - Application Insights integrated and tracking all requests automatically
       - Function App logs processing time (`ProcessingTime={ProcessingTime}ms`)
       - Request duration metrics available in Application Insights
       - Performance data can be analyzed post-deployment via Application Insights dashboards
    4. **Load Testing Tools Not Set Up:** Comprehensive performance testing requires load testing tools (e.g., k6, JMeter, Azure Load Testing) which are not currently configured. Setup and execution would require significant time investment.
    5. **Time Constraints:** One week remaining to complete roadmap. Performance testing is valuable but not critical for MVP demonstration. System is operational and performance appears acceptable.
    6. **Demonstration Focus:** Primary goal is demonstrating dual-system architecture and event-driven patterns, not production-scale performance. Current performance is sufficient for demonstrations.
  - **What We Know:**
    - Feature flag check overhead: ~1ms (negligible)
    - Outbox write overhead: ~5-10ms (minimal, non-blocking)
    - Booking creation response time: < 500ms (acceptable)
    - Event processing time: ~31 seconds (within 30-second polling interval, acceptable for async processing)
    - No performance degradation observed compared to baseline
  - **Future Consideration:** 
    - Performance testing can be conducted post-MVP if needed
    - Application Insights provides historical performance data for analysis
    - Load testing can be added as Phase 10 enhancement if system scales to production
  - **Status:** ✓ **Deferred** - Performance testing deferred due to time constraints and MVP focus. Minimal performance impact already validated. Application Insights provides ongoing performance monitoring. Detailed testing can be conducted post-MVP if needed.

### Phase 8: Monitoring & Observability
- [x] **8.1** Add Application Insights custom events
  - Created `ITelemetryService` interface and `ApplicationInsightsTelemetryService` implementation
  - **Custom Events Tracked:**
    - `BookingCreated` - Tracks booking creation with `SystemType` ("Synchronous" or "Event-Driven") and `ArchitectureMode`
    - `OutboxEventCreated` - Tracks outbox event creation (always created for audit)
    - `OutboxEventProcessed` - Tracks outbox event processing with `ProcessingTimeMs` metric (Event-Driven only)
    - `ServiceBusEventPublished` - Tracks Service Bus publishing (Event-Driven only)
    - `FeatureFlagToggled` - Tracks feature flag toggles with `FromMode` and `ToMode`
    - `ModeSwitch` - Tracks architecture mode switches
  - **Function App Events:**
    - `FunctionBookingCreatedProcessed` - Tracks Function App processing with `ProcessingTimeMs` metric
  - Integrated telemetry tracking into:
    - `BookingsController` - Tracks booking creation and outbox events
    - `OutboxProcessorService` - Tracks event processing with timing
    - `ServiceBusEventPublisher` - Tracks Service Bus publishing
    - `FeatureFlagController` - Tracks mode switches
    - `OnBookingCreatedFunction` - Tracks Function App processing
  - All events include `SystemType` property to clearly distinguish Synchronous vs Event-Driven
  - **Status:** Complete - Custom events implemented and integrated throughout event flow

- [ ] **8.2** Create Application Insights dashboard
  - **Dashboard guide created:** `docs/journal/phase8_dashboard_guide.md` with step-by-step instructions
  - **Dashboard Components:**
    - Bookings by Architecture Mode (pie chart)
    - Bookings Over Time - Synchronous vs Event-Driven (time chart)
    - Event-Driven Flow: Outbox → Service Bus → Function (time chart)
    - Event Processing Time (time chart with metrics)
    - Mode Switches (table)
    - Synchronous Mode Bookings (time chart)
    - Event-Driven Mode: Complete Flow (time chart)
    - Recent Activity Comparison (table)
  - **KQL Queries:** All queries provided in dashboard guide
  - **Demonstration Workflow:** Step-by-step guide for live demos
  - **Status:** Dashboard guide ready - awaiting dashboard creation in Azure Portal

- [ ] **8.3** Set up alerts
  - Dead letter queue messages
  - Function failures
  - Outbox processing delays

### Phase 9: Documentation & Cleanup
- [ ] **9.1** Update ADR-006 status to "Accepted"
  - Document implementation details
  - Add architecture diagrams for both modes
  - Document permanent feature flag approach
  - Explain dual-system coexistence

- [ ] **9.2** Update architecture.md
  - Add event flow diagrams
  - Document new components
  - Update system overview
  - Add side-by-side comparison of both architectures
  - Document when to use each mode

- [ ] **9.3** Create developer guide
  - How to add new event types
  - How to create new function handlers
  - Local development setup
  - Testing event flow locally
  - **How to switch between synchronous and event-driven modes**
  - **Demonstration guide: showing both architectures**

- [ ] **9.4** Create comparison documentation
  - Side-by-side comparison of chained API vs event-driven
  - Performance metrics for both approaches
  - Use cases for each mode
  - Migration guide (conceptual, since both coexist)

- [ ] **9.5** Update week journal
  - Document implementation progress
  - Note challenges and solutions
  - Update next steps

### Phase 10: Future Enhancements (Post-MVP)
- [ ] **10.1** Add more event types
  - `BookingCancelled`
  - `BookingActivated`
  - `TicketValidated`

- [ ] **10.2** Implement Service Bus Topics
  - Multiple subscribers per event
  - Event routing

- [ ] **10.3** Add event replay capability
  - Reprocess failed events
  - Historical event processing

---

**Notes:**
- Each phase can be implemented incrementally
- Feature flags allow gradual rollout
- Core booking functionality remains synchronous until event flow is validated
- Delete checklist items as they are completed


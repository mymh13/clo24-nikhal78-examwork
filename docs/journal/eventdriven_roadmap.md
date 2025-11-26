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
- [ ] **3.1** Create `IOutboxService` interface
  - `AddEventAsync<T>(T eventData)` method
  - `GetPendingEventsAsync()` method
  - `MarkAsProcessedAsync(string eventId)` method

- [ ] **3.2** Implement `OutboxService` class
  - Cosmos DB integration for storing events
  - JSON serialization of event data
  - Transaction support (if using same container as bookings)

- [ ] **3.3** Integrate Outbox into `BookingsController`
  - After successful booking creation, add event to outbox
  - Use same transaction/operation context as booking creation
  - Log outbox event creation

- [ ] **3.4** Register `OutboxService` in dependency injection
  - Add to `ServiceCollectionExtensions`
  - Configure as scoped service

### Phase 4: Feature Flag Integration (Permanent Dual-System Support)

**Recommendation: Use Azure App Configuration (not Bicep) for feature flags**

**Why App Configuration over Bicep:**
- **Runtime configuration changes** - Toggle between modes without redeployment (critical for demonstrations)
- **Live switching** - Can demonstrate both architectures in real-time during presentations
- **Environment-specific values** - Different flags for dev vs prod
- **Feature flag management UI** - Azure Portal provides easy management interface
- **More realistic** - Matches production scenarios where flags are managed operationally
- **No code changes needed** - Change behavior by updating configuration only

**Note:** Bicep will still be used to CREATE the App Configuration resource (infrastructure), but flag values will be managed in App Configuration (configuration).

- [ ] **4.1** Create Azure App Configuration resource via Bicep
  - Add App Configuration module to infrastructure
  - Create App Configuration instance
  - Configure connection string in Key Vault
  - Set up managed identity access

- [ ] **4.2** Integrate App Configuration into application
  - Add `Microsoft.Extensions.Configuration.AzureAppConfiguration` NuGet package
  - Configure App Configuration connection in `Program.cs`
  - Set up feature flag provider
  - **Implement sentinel key pattern** for hot-reload (no restart required)
    - Create sentinel key (e.g., `Settings:Sentinel`) in App Configuration
    - Configure `Refresh()` with sentinel key watch: `.ConfigureRefresh(refresh => refresh.Register("Settings:Sentinel", refreshAll: true))`
    - When sentinel key value changes, all configuration (including feature flags) refreshes automatically
    - This enables runtime feature flag toggling without service restart
  - Add fallback to appsettings.json for local development

- [ ] **4.3** Create feature flag configuration
  - Add `BookingEvents:Enabled` flag to App Configuration
  - Set initial value to `false` (synchronous mode by default)
  - Create `IFeatureFlagService` interface
  - Implement feature flag service with App Configuration integration
  - **Design for permanence:** Flags will remain in system to allow switching between architectures

- [ ] **4.4** Integrate feature flag check in booking flow
  - **Synchronous path (default):** When flag is `false`, bookings work as before (chained API)
  - **Event-driven path:** When flag is `true`, events are published to Service Bus
  - Always write to outbox (for audit and future activation)
  - Log feature flag status and which path was taken
  - **Both paths must work independently** - no breaking changes to existing flow
  - **Hot-reload via sentinel key** - Update sentinel key in App Configuration to refresh feature flags without restart

- [ ] **4.5** Design dual-system architecture
  - Ensure synchronous flow remains fully functional when events are disabled
  - Event-driven flow operates in parallel when enabled
  - No performance degradation when events are disabled
  - Clear separation of concerns between both paths
  - **Both systems coexist permanently** - this is the core refactoring simulation

- [ ] **4.6** Document feature flag usage in ADR-006
  - Update ADR-006 with implementation details
  - Document how to enable/disable event flow via App Configuration
  - Explain permanent dual-system approach
  - Document use cases for each mode (demonstration, testing, production)
  - Add instructions for switching modes during demonstrations

### Phase 5: Service Bus Integration
- [ ] **5.1** Create `IEventPublisher` interface
  - `PublishEventAsync<T>(T eventData)` method
  - Error handling and retry logic

- [ ] **5.2** Implement `ServiceBusEventPublisher` class
  - Service Bus client initialization
  - Message serialization (JSON)
  - Connection string from Key Vault
  - Error handling and logging

- [ ] **5.3** Create background service for processing outbox
  - `OutboxProcessorService` (IHostedService)
  - Poll outbox for pending events
  - Publish to Service Bus when feature flag enabled
  - Mark events as processed after successful publish
  - Handle failures and retries

- [ ] **5.4** Register Service Bus services in DI
  - Register `ServiceBusClient`
  - Register `IEventPublisher`
  - Register `OutboxProcessorService` as hosted service

### Phase 6: Azure Function Implementation
- [ ] **6.1** Create Azure Functions project structure
  - `src/functions/Ticketing.Functions` project
  - Configure function app settings
  - Set up local development (local.settings.json)

- [ ] **6.2** Implement `OnBookingCreated` function
  - Service Bus trigger binding
  - Deserialize `BookingCreated` event
  - Log event processing
  - Update booking status (if needed)
  - Send to Application Insights

- [ ] **6.3** Add error handling and dead letter queue
  - Try-catch blocks
  - Dead letter queue configuration
  - Retry policies

- [ ] **6.4** Deploy Function App
  - Configure deployment pipeline
  - Test deployment to dev environment

### Phase 7: Testing & Validation (Dual-System Testing)
- [ ] **7.1** Test synchronous flow (feature flag disabled)
  - Verify bookings work exactly as before (chained API mode)
  - Verify outbox events are created (for audit)
  - Verify no Service Bus messages sent
  - Verify no performance impact from event infrastructure
  - **This is the "before refactoring" state**

- [ ] **7.2** Test event-driven flow (feature flag enabled)
  - Create booking
  - Verify outbox event created
  - Verify Service Bus message sent
  - Verify Function receives and processes event
  - Verify Application Insights logs
  - **This is the "after refactoring" state**

- [ ] **7.3** Test switching between modes
  - Toggle feature flag at runtime (if supported)
  - Verify system handles mode switch gracefully
  - Test bookings created in both modes
  - Verify no data loss or corruption

- [ ] **7.4** Test error scenarios
  - Service Bus connection failure (event-driven mode)
  - Function processing failure
  - Dead letter queue handling
  - Outbox retry logic
  - Verify synchronous mode unaffected by event system failures

- [ ] **7.5** Performance testing
  - Compare performance: synchronous vs event-driven
  - Multiple concurrent bookings in both modes
  - Outbox processing throughput
  - Function scaling behavior
  - Document performance characteristics of each approach

### Phase 8: Monitoring & Observability
- [ ] **8.1** Add Application Insights custom events
  - Track event publishing
  - Track function execution
  - Track outbox processing

- [ ] **8.2** Create Application Insights dashboard
  - Event publishing metrics
  - Function execution metrics
  - Error rates
  - Processing latency

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


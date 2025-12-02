# ADR-015 – Application Insights Telemetry Strategy: Custom Events for Dual-System Architecture

**Status:** Accepted  
**Date:** 2025-12-01  
**Author:** Niklas Häll

---

## Context

The ticketing system implements a **permanent dual-system coexistence** architecture where both synchronous (chained API) and event-driven architectures run side-by-side. Feature flags control which architecture path is active, enabling runtime switching between modes.

**The Problem:**
- For live demonstrations and operational monitoring, we need to **clearly visualize** which architecture mode is active
- Standard Application Insights telemetry doesn't differentiate between Synchronous and Event-Driven modes
- We need **custom business events** that show the architecture path taken for each booking
- We need to **demonstrate the difference** between modes in real-time during presentations

---

## Decision

The system implements a **custom telemetry abstraction layer** using Application Insights with **domain-specific custom events** that clearly differentiate between Synchronous and Event-Driven architectures.

**Implementation:**

1. **Telemetry Abstraction Pattern:**
   - `ITelemetryService` interface abstracts telemetry implementation
   - `ApplicationInsightsTelemetryService` implementation using `TelemetryClient`
   - Registered as singleton in dependency injection
   - Allows future replacement of telemetry provider without changing business logic

2. **Custom Events Tracked:**

   | Event Name | Triggered When | Key Properties | Purpose |
   |------------|----------------|----------------|---------|
   | `BookingCreated` | Every booking creation (both modes) | `BookingId`, `SystemType` ("Synchronous" or "Event-Driven") | Shows which mode processed the booking |
   | `OutboxEventCreated` | Outbox event creation (both modes, for audit) | `OutboxEventId`, `BookingId`, `Status` ("Pending") | Audit trail of all outbox events |
   | `OutboxEventProcessed` | Outbox event processed (Event-Driven only) | `OutboxEventId`, `ProcessingTimeMs` metric | Confirms event-driven flow is working |
   | `ServiceBusEventPublished` | Event published to Service Bus (Event-Driven only) | `EventId`, `QueueName` | Confirms events reach Service Bus |
   | `FunctionBookingCreatedProcessed` | Function App processes event (Event-Driven only) | `BookingId`, `ProcessingTimeMs` metric | Confirms Function App processing |
   | `FeatureFlagToggled` | Feature flag toggled via admin | `FromMode`, `ToMode`, `UserId` | Tracks mode switch initiation |
   | `ModeSwitch` | Architecture mode successfully switches | `FromMode`, `ToMode`, `SystemType` | Tracks successful mode transitions |

3. **Key Design Decisions:**
   - **`SystemType` property** - Every event includes `SystemType` to enable filtering and visualization
   - **Immediate flushing** - `TelemetryClient.Flush()` called after critical events for demo visibility
   - **Error handling** - Telemetry failures are logged but don't affect business logic (fail-safe)
   - **Unified telemetry** - Both App Service and Function App send to same Application Insights resource

4. **Integration Points:**
   - `BookingsController` → `BookingCreated`, `OutboxEventCreated`
   - `OutboxProcessorService` → `OutboxEventProcessed` (with timing)
   - `ServiceBusEventPublisher` → `ServiceBusEventPublished`
   - `FeatureFlagController` → `FeatureFlagToggled`, `ModeSwitch`
   - `OnBookingCreatedFunction` → `FunctionBookingCreatedProcessed` (with timing)

---

## Consequences

**Advantages:**
- **Clear Architecture Differentiation** - `SystemType` property enables easy filtering and visualization of Synchronous vs Event-Driven flows
- **Complete Event Lifecycle Tracking** - Can trace bookings from creation through outbox, Service Bus, and Function processing
- **Performance Comparison** - `ProcessingTimeMs` metrics enable comparison between architectures
- **Live Demonstration Support** - Custom events with immediate flushing enable real-time visualization in Application Insights Workbooks
- **Abstraction Layer** - Interface allows changing telemetry provider without modifying business logic
- **Unified Monitoring** - Single Application Insights resource collects telemetry from both App Service and Function App

**Disadvantages:**
- **Additional Code Complexity** - Custom telemetry service adds abstraction layer requiring maintenance
- **Application Insights Dependency** - Locked into Application Insights (mitigated by abstraction layer)
- **Ingestion Delay** - 2-5 minute delay makes true real-time visualization challenging (acceptable for demos with auto-refresh)
- **Manual Event Tracking** - Requires explicit tracking calls in business logic

---

## Risks / Mitigations

- **Risk:** Telemetry failures could impact application performance.  
  **Mitigation:** All telemetry calls wrapped in try-catch. Failures are logged but don't throw exceptions. Fail-safe design.

- **Risk:** Application Insights ingestion delay (2-5 minutes) makes real-time demos difficult.  
  **Mitigation:** `TelemetryClient.Flush()` sends events immediately. Application Insights Workbooks with 1-minute auto-refresh provide near real-time visibility for demos.

- **Risk:** High event volume could increase Application Insights costs.  
  **Mitigation:** Monitor telemetry volume via Azure Cost Management. Current MVP scale is minimal.

- **Risk:** Missing telemetry calls in new code paths could create monitoring gaps.  
  **Mitigation:** Document telemetry requirements in code reviews. Include in development checklist.

---

## Alternatives

- **Direct TelemetryClient Usage** - Use `TelemetryClient` directly without abstraction.  
  **Rejected:** Creates tight coupling to Application Insights. Abstraction provides flexibility with minimal overhead.

- **Standard Application Insights Only** - Rely on automatic instrumentation without custom events.  
  **Rejected:** Standard telemetry doesn't differentiate between Synchronous and Event-Driven modes. Custom events essential for dual-system visualization.

- **Structured Logging Only** - Use `ILogger` instead of custom events.  
  **Rejected:** Logs are harder to query and visualize. Custom events provide better dashboard support and metrics.

- **External Telemetry Service** - Use third-party service (Datadog, New Relic) instead of Application Insights.  
  **Rejected:** Application Insights is native to Azure, integrates seamlessly, and is included in App Service plan.

---

## Visualization Strategy

For live demonstrations, Application Insights **Workbooks** are used with three key sections:

1. **Current Mode Indicator** - Shows active architecture mode (Synchronous or Event-Driven) using latest `ModeSwitch` or `BookingCreated` event
2. **Latest Booking Flow Timeline** - Displays complete event flow for most recent booking, showing full chain in Event-Driven mode vs minimal events in Synchronous mode
3. **Events by Type** - Column chart showing event counts, visually demonstrating which events appear in each mode

Workbooks are configured with:
- **Time range:** Last 15 minutes
- **Auto-refresh:** 1 minute (accounts for ingestion delay)
- **Real-time updates:** Events propagate within 30-120 seconds after creation

This setup enables live demonstrations where mode switching and booking creation are visible in near real-time, clearly showing the architectural difference between Synchronous and Event-Driven flows.

---

## References
- [ADR-005 - Azure Services](./ADR-005-azureservices.md) - Application Insights service selection
- [ADR-006 - Event-Driven Architecture](./ADR-006-eventdriven.md) - Dual-system coexistence architecture
- [ADR-012 - Azure App Configuration](./ADR-012-azure-app-configuration.md) - Feature flags for mode switching
- [ADR-013 - Outbox Pattern](./ADR-013-outbox-pattern.md) - Outbox event processing
- [ADR-018 - Error Handling & Logging Strategy](./ADR-018-error-handling-logging-strategy.md) - Logging integration with Application Insights
- [Event-Driven Architecture Roadmap](../journal/eventdriven_roadmap.md) - Phase 8.1 implementation details
- [Microsoft Docs – Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Microsoft Docs – Custom Events](https://learn.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics)

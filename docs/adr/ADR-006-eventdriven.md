# ADR-006 – Event-driven architecture: Azure Service Bus + Azure Function + Outbox Pattern

**Status:** Accepted (Dual-System Implementation)  
**Date:** 2025-10-30  
**Last Updated:** 2025-11-27  
**Author:** Niklas Häll

---

## Context
The existing system that the project is inspired by is built with sequential and chained API calls (API chaining).  
This leads to high coupling between services and difficulties with error handling and scaling.  
To modernize the architecture, a transition to **event-driven communication** is planned, where the system reacts to published events rather than synchronous API calls.

---

## Decision
The system implements **permanent dual-system coexistence** - both synchronous (chained API) and event-driven architectures run side-by-side permanently. This simulates a real-world refactoring scenario where old and new systems coexist.

**Implementation:**
- **Outbox Pattern** implemented in the API (booking events are logged simultaneously as data is written to the database)
- **Azure Service Bus** created for publishing and subscribing to events
- **Azure Function** will react to selected events (e.g., `BookingCreated`) and perform post-processing, notification, or audit logging
- **Feature flags** in Azure App Configuration control which architecture path is active
- **Sentinel key pattern** enables hot-reload of feature flags without service restart

**Dual-System Architecture:**
- **Synchronous Path (default):** When `BookingEvents:Enabled = false`, bookings work exactly as before via chained API calls. No breaking changes.
- **Event-Driven Path:** When `BookingEvents:Enabled = true`, events are published to Service Bus for asynchronous processing.
- **Both paths coexist permanently** - feature flags remain in the system to allow runtime switching between architectures.
- **Always write to outbox** - events are stored in outbox regardless of feature flag state (for audit and future activation).

This approach enables:
- Live demonstrations of both architectures without code changes
- Gradual migration and testing
- Comparison of both approaches side-by-side
- Zero-downtime switching between modes

---

## Consequences
**Advantages:**  
- Looser coupling between components.  
- Easy to build out new features that subscribe to existing events.  
- Improved robustness and scalability under high load.  
- A realistic model for simulating modernization of older API-based systems.
- **Dual-system coexistence** allows comparison and demonstration of both architectures.
- **No breaking changes** - synchronous flow remains fully functional when events are disabled.
- **Hot-reload support** - switch between architectures at runtime without service restart.
- **Performance isolation** - no performance degradation when event-driven path is disabled.

**Disadvantages:**  
- Increased complexity in troubleshooting (the flow becomes asynchronous).  
- Requires more Azure resources and can increase costs.  
- Functions must be designed idempotent to avoid duplicate events.
- **Dual-system overhead** - outbox is always written (minimal performance impact).
- **Feature flag management** - requires operational discipline to manage flags correctly.  

---

## Risks / Mitigations
- **Risk:** Events can be lost in case of errors in Service Bus or Function.  
  **Mitigation:** Use Dead Letter Queue and monitoring via Application Insights.  

- **Risk:** Too early activation of the event flow can increase costs in the MVP phase.  
  **Mitigation:** Keep Service Bus and Functions provisioned but inactive until test or demo.  

- **Risk:** Asynchronicity makes system behavior harder to predict.  
  **Mitigation:** Keep the core flow synchronous and log events separately in Outbox until maturity is reached.

- **Risk:** Feature flag failures could break the system.  
  **Mitigation:** Feature flag service defaults to `false` (synchronous mode) if flag check fails, ensuring system remains functional.

- **Risk:** Dual-system complexity could confuse developers.  
  **Mitigation:** Clear logging of which architecture path is taken, comprehensive documentation, and feature flag status visible in health endpoint.  

---

## Alternatives
- **Pure API-based architecture:** Simpler but difficult to scale and handle errors.  
- **Azure Event Grid:** More advanced and a good service in itself, but unnecessary for MVP and risks increasing costs when multiple event types are introduced.   
- **Service Bus Topics + multiple Functions:** Can be introduced in a later version for broader event distribution.  
  
Note: each additional messaging service (Service Bus, Event Grid, Event Hubs) entails additional costs over time. Therefore, the MVP is kept to Service Bus + Function as the minimum event-driven core.  

---

## Implementation Details

### Feature Flag Configuration
- **Flag Name:** `BookingEvents:Enabled`
- **Default Value:** `false` (synchronous mode)
- **Location:** Azure App Configuration
- **Hot-Reload:** Enabled via sentinel key pattern (`Settings:Sentinel`)

### Architecture Flow

**Synchronous Path (BookingEvents:Enabled = false):**
1. Booking created in Cosmos DB
2. Outbox event created (for audit)
3. Booking returned to client
4. No Service Bus publishing
5. No Function processing

**Event-Driven Path (BookingEvents:Enabled = true):**
1. Booking created in Cosmos DB
2. Outbox event created
3. Event published to Service Bus (Phase 5)
4. Azure Function processes event (Phase 6)
5. Booking returned to client

### Separation of Concerns
- **Feature Flag Service:** Isolated service for flag checking, defaults to safe mode on failure
- **Outbox Service:** Always writes events regardless of flag state
- **Event Publisher:** (Phase 5) Only publishes when flag is enabled
- **Booking Service:** Unchanged - no knowledge of event system
- **Clear boundaries:** Each component has single responsibility

### Performance Characteristics
- **Feature flag check:** Async but lightweight (~1ms overhead)
- **Outbox write:** Always performed (~5-10ms overhead)
- **Service Bus publish:** Only when flag enabled (Phase 5)
- **No blocking:** All event operations are fire-and-forget, don't block booking creation

## References
- [Architecture overview](../initial_outtakes/architecture.md)  
- [System overview](../initial_outtakes/system_overview.md)  
- [Event-Driven Roadmap](../journal/eventdriven_roadmap.md)
- [Microsoft Docs – Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview)  
- [Microsoft Docs – Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview)  
- [Transactional outbox pattern with Azure Cosmos DB](https://learn.microsoft.com/sv-se/azure/architecture/databases/guide/transactional-outbox-cosmos)
- [ADR-012: Azure App Configuration](../adr/ADR-012-azure-app-configuration.md)
- [ADR-013: Outbox Pattern](../adr/ADR-013-outbox-pattern.md)  
  
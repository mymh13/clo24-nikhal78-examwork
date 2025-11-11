# ADR-006 – Event-driven architecture: Azure Service Bus + Azure Function + Outbox Pattern

**Status:** Planned  
**Date:** 2025-10-30  
**Author:** Niklas Häll

---

## Context
The existing system that the project is inspired by is built with sequential and chained API calls (API chaining).  
This leads to high coupling between services and difficulties with error handling and scaling.  
To modernize the architecture, a transition to **event-driven communication** is planned, where the system reacts to published events rather than synchronous API calls.

---

## Decision
The system is prepared for **event-driven architecture** by:
- introducing the **Outbox Pattern** in the API (booking events are logged simultaneously as data is written to the database),  
- creating an **Azure Service Bus** for publishing and subscribing to events,  
- letting an **Azure Function** react to selected events (e.g., `BookingCreated`) and perform post-processing, notification, or audit logging.  

The entire flow can be activated or deactivated via **feature flags** in App Configuration, which allows the MVP to run completely without these components during development.

---

## Consequences
**Advantages:**  
- Looser coupling between components.  
- Easy to build out new features that subscribe to existing events.  
- Improved robustness and scalability under high load.  
- A realistic model for simulating modernization of older API-based systems.

**Disadvantages:**  
- Increased complexity in troubleshooting (the flow becomes asynchronous).  
- Requires more Azure resources and can increase costs.  
- Functions must be designed idempotent to avoid duplicate events.  

---

## Risks / Mitigations
- **Risk:** Events can be lost in case of errors in Service Bus or Function.  
  **Mitigation:** Use Dead Letter Queue and monitoring via Application Insights.  

- **Risk:** Too early activation of the event flow can increase costs in the MVP phase.  
  **Mitigation:** Keep Service Bus and Functions provisioned but inactive until test or demo.  

- **Risk:** Asynchronicity makes system behavior harder to predict.  
  **Mitigation:** Keep the core flow synchronous and log events separately in Outbox until maturity is reached.  

---

## Alternatives
- **Pure API-based architecture:** Simpler but difficult to scale and handle errors.  
- **Azure Event Grid:** More advanced and a good service in itself, but unnecessary for MVP and risks increasing costs when multiple event types are introduced.   
- **Service Bus Topics + multiple Functions:** Can be introduced in a later version for broader event distribution.  
  
Note: each additional messaging service (Service Bus, Event Grid, Event Hubs) entails additional costs over time. Therefore, the MVP is kept to Service Bus + Function as the minimum event-driven core.  

---

## References
- [Architecture overview](../architecture.md)  
- [System overview](../system_overview.md)  
- [Microsoft Docs – Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview)  
- [Microsoft Docs – Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview)  
- [Transactional outbox pattern with Azure Cosmos DB](https://learn.microsoft.com/sv-se/azure/architecture/databases/guide/transactional-outbox-cosmos)  
  
## Architecture Overview
### Purpose
  
This document describes how the application is structured and what the data flow looks like both from the user's perspective and from a technical perspective. The system is designed to be modular, event-ready, and cost-effective with a focus on simple operations in Azure. 
  
1. User Flow
```java
[ Customer ] 
   │
   │ 1. Logs in with customer account (ASP.NET Identity) (Admin/Inspector logs in via Entra ID)
   │
   ▼
[ Blazor Server UI ]
   │ 2. Shows available trips (possibly also zones)
   │ 3. Customer selects trip and books ticket
   │
   ▼
[ Ticketing API ]
   │ 4. Receives booking request
   │ 5. Creates booking in database
   │
   ▼
[ Cosmos DB (Serverless) ]
   │ 6. Stores booking, trip, zone and reference to user:
            - customer: userAccountId (GUID)
            - admin/inspector: oid (Entra ID)
   │
   ▼
[ Application Insights ]
   │ 7. Logs user flow for monitoring and statistics
   │
   ▼
[ Blazor Server UI ]
   │ 8. Shows confirmation and "My bookings"
```
 
2. Technical Flow (Internal Architecture)
```java
                      ┌─────────────────────────────────────┐
                      │         User Interface               │
                      │─────────────────────────────────────│
                      │  Blazor Server (.NET 8)             │
                      │  Handles UI, login and session      │
                      │  Auth: ASP.NET Identity + Entra ID  │
                      └─────────────────────────────────────┘
                                      │
                                      ▼
                      ┌─────────────────────────────────────┐
                      │        Application Layer            │
                      │─────────────────────────────────────│
                      │  ASP.NET Controller API             │
                      │  • Validates request                │
                      │  • Calls domain services            │
                      │  • Creates outbox event (optional)  │
                      └─────────────────────────────────────┘
                                      │
                                      ▼
                      ┌─────────────────────────────────────┐
                      │         Database Layer              │
                      │─────────────────────────────────────│
                      │  Azure Cosmos DB (Serverless)       │
                      │  • Trip-, Booking-, Zone-data       │
                      │  • Minimal cost when idle            │
                      └─────────────────────────────────────┘
                                      │
                                      ▼
           ┌────────────────────────────────────────────────────────┐
           │    Event and Integration Layer (optional)                │
           │────────────────────────────────────────────────────────│
           │  • Azure Service Bus  – buffers events                 │
           │  • Azure Function     – handles e.g. BookingCreated    │
           │  • Outbox Pattern     – ensures delivery               │
           └────────────────────────────────────────────────────────┘
                                      │
                                      ▼
                      ┌─────────────────────────────────────┐
                      │   Configuration and Security        │
                      │─────────────────────────────────────│
                      │  Azure App Configuration            │
                      │  Azure Key Vault                    │
                      │  Application Insights (telemetry)    │
                      └─────────────────────────────────────┘
```

### Summary
The application follows a clear layer structure: 
- Blazor Server handles interaction with the user.
- Ticketing API implements business logic and communicates with the database.
- Cosmos DB serves as central storage.
- Service Bus and Functions can be activated via feature flag to transition to an event-driven approach.
- App Configuration, Key Vault, and Application Insights are used across the entire system for configuration, security, and monitoring.
 
### Appendix - Draft Event Flow (Future Module)
```java
[ Ticketing API ]
    │
    │ 1. Customer creates booking via Blazor interface
    │
    ├─► Creates booking in Cosmos DB
    │
    ├─► Adds entry to Outbox (type: BookingCreated)
    │
    └─► If feature flag "BookingEvents.Enabled" = true:
            ▼
            [ Azure Service Bus ]
                │
                │ 2. Receives message "BookingCreated"
                │
                ▼
            [ Azure Function: OnBookingCreated ]
                │
                │ 3. Handles the event:
                │     - updates status / notifies / writes log
                │     - can trigger new events (e.g. BookingConfirmed)
                │
                ▼
            [ Application Insights ]
                │
                │ 4. Logs entire event chain for tracking and analysis
                ▼
            [ Cosmos DB ]
                │
                │ 5. Any updates to data storage
                ▼
            [ Blazor UI ]
                │
                │ 6. Customer receives updated status (e.g. "Confirmed booking")
```
### Event Flow Summary
This flow illustrates how the system can be extended to an event-driven approach without changing the existing core logic. In the synchronous version, the booking is written directly to the database via the API. 
 
When the event flow is activated, the Outbox pattern is used to create an event entry in the same transaction as the data storage. This event is then sent to Azure Service Bus, where an Azure Function reacts to the message and performs post-processing, such as logging, notification, or status updates. 
 
The advantages of this approach are:
- The system becomes more loosely coupled and can be extended incrementally.
- New features can be added as separate subscribers (e.g., email, statistics, reporting).
- The event flow can be activated or paused via feature flags without affecting the base system.
- It enables scalable parallel processing without the API being burdened by long-running operations. 
 
This makes the architecture future-proof and ready for a more distributed, event-driven infrastructure when the need arises. It also means we can simulate an environment where an existing system, built on chained API calls (API chaining), can be refactored and replaced with a more modern and event-driven architecture.
 
### Disclaimer
I asked an LLM to draw the ASCII flows according to my description. 
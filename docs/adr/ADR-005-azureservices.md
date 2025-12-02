# ADR-005 – Cloud services choice: Azure App Service, App Configuration, Key Vault, Application Insights, API Management, Cosmos DB, Service Bus, and Azure Functions

**Status:** Accepted  
**Date:** 2025-10-30  
**Last Updated:** 2025-11-27  
**Author:** Niklas Häll

---

## Context
The system is built on the Azure platform and needs several complementary services to handle operations, security, configuration, and monitoring.  
The goal is to create a solution that:
- is **cheap to run** during development (Free or Serverless plans),
- **easy to scale up** for demonstration or production, and
- **prepared for CI/CD** and future DevOps integration.

Since the organization already uses Azure as the main platform, it is natural to also utilize Microsoft's own cloud services for infrastructure, configuration, and logging.

---

## Decision
The following Azure services are used in MVP and future expansion:

|          Service           |               Purpose                                                          | Tier/Plan |
|---------------------------|------------------------------------------------------------------------------|-----------|
| **App Service**           | Run Blazor Server and API application in the same App Service plan.  | Basic B1 (dev) |
| **App Configuration**     | Centralized management of settings and feature flags between environments. Hot-reload via sentinel key pattern. | Free (dev) |
| **Key Vault**             | Secure storage of secrets and connection strings. Managed identity access via RBAC. | Standard |
| **Application Insights**  | Logging, monitoring, and performance measurement via KQL. Used by both App Service and Function App. Custom events track dual-system architecture flows. | Pay-as-you-go |
| **API Management (APIM)** | Gateway for public GET endpoints, caching, and rate limiting. | (Future) |
| **Cosmos DB**             | Serverless NoSQL database for bookings, users, and outbox events. Partition key strategy for efficient querying. | Serverless |
| **Service Bus**           | Message queue for event-driven architecture. `booking-events` queue with dead letter queue support. | Basic (dev) |
| **Azure Functions**       | Serverless event processing. Processes Service Bus messages for event-driven flow. | Basic B1 (dev) |
| **Storage Account**       | Required for Azure Functions runtime and state management. | Standard LRS |

---

## Consequences
**Advantages:**
- Full integration with other Azure services and DevOps flows.  
- Low entry cost (Free/Serverless/Basic tiers for dev).  
- Centralized and secure management of configuration and secrets.  
- Easy transition to production via scaling in the App Service plan.  
- APIM provides the ability to future-proof the APIs with versioning and access control.
- **Event-driven architecture support** - Service Bus and Functions enable asynchronous processing and loose coupling.
- **Managed identity everywhere** - All services use RBAC with managed identity, eliminating connection strings in code.
- **Unified monitoring** - Application Insights collects telemetry from both App Service and Functions.
- **Dual-system coexistence** - Feature flags in App Configuration enable runtime switching between synchronous and event-driven modes.
- **Custom telemetry events** - Application Insights custom events clearly differentiate between Synchronous and Event-Driven architectures for visualization and monitoring (see [ADR-015](./ADR-015-application-insights-telemetry-strategy.md)).

**Disadvantages:**
- Locked into the Azure ecosystem.  
- Limited resources in Free/Basic plans (can "sleep" during inactivity for Free tier).  
- APIM in Consumption mode can become slower than direct API access.
- **Additional services increase complexity** - More services to manage, monitor, and secure.
- **Cost accumulation** - Multiple services (even at low tiers) can add up. Requires cost monitoring.
- **Service Bus Basic tier limitations** - Basic tier has message size and throughput limits. May need Standard tier for production.  

---

## Risks / Mitigations
- **Risk:** App Service in Free tier shuts down during inactivity.  
  **Mitigation:** Using Basic B1 tier for dev environment. Upgrade to Standard for production.

- **Risk:** Incorrect handling of secrets can lead to exposure.  
  **Mitigation:** Store all connection strings in Key Vault and use Managed Identity with RBAC for access. No secrets in code or configuration files.

- **Risk:** Unnecessary costs when testing multiple services simultaneously.  
  **Mitigation:** Use Free/Serverless/Basic tiers for dev. Feature flags allow disabling event-driven flow without deleting infrastructure. Monitor costs via Azure Cost Management.

- **Risk:** Service Bus Basic tier may not handle production load.  
  **Mitigation:** Basic tier sufficient for dev/demo. Upgrade to Standard tier for production with higher throughput and message size limits.

- **Risk:** Function App Basic plan may have scaling limitations.  
  **Mitigation:** Basic B1 plan sufficient for dev. Consider Consumption plan (Y1) for production auto-scaling, or Premium plan for advanced features.

- **Risk:** Cosmos DB Serverless can have unpredictable costs at scale.  
  **Mitigation:** Serverless ideal for dev (pay per operation). Monitor RU consumption. Consider provisioned throughput for production with predictable costs.

- **Risk:** Multiple services increase attack surface.  
  **Mitigation:** All services use managed identity with RBAC. No public endpoints except App Service. Network security groups and private endpoints can be added for production.  

---

## Alternatives
- **Container-based operations (Azure Container Apps or Azure Kubernetes Service):** Powerful but overdimensioned for MVP.  
- **Static hosting (e.g., Blazor WASM + Blob Storage):** Cheap, but lacks support for server-based login and real-time. 
- **External logging (e.g., Grafana Cloud, Elastic):** Flexible but increases complexity and operational costs.  

---

## Implementation Details

### Service Tiers and Configuration (Dev Environment)
- **App Service:** Basic B1 plan (Linux, .NET 8, Docker from GHCR)
- **App Configuration:** Free tier with managed identity access
- **Key Vault:** Standard tier with RBAC authorization
- **Application Insights:** Pay-as-you-go (included in App Service plan)
- **Cosmos DB:** Serverless mode (pay per operation)
- **Service Bus:** Basic tier with `booking-events` queue
- **Azure Functions:** Basic B1 plan (Linux, .NET 8, isolated worker)
- **Storage Account:** Standard LRS for Function App runtime

### Managed Identity and RBAC
All services use **managed identity** with **RBAC** for secure access:
- App Service has "App Configuration Data Reader" role
- App Service has "Azure Service Bus Data Owner" role
- Function App has "Azure Service Bus Data Receiver" role
- Function App has "DocumentDB Account Contributor" role for Cosmos DB
- No connection strings stored in code or configuration files

**See [ADR-016 - Managed Identity & RBAC Strategy](./ADR-016-managed-identity-rbac-strategy.md) for detailed implementation and rationale.**

### Service Integration
- **App Service** → App Configuration (feature flags), Key Vault (secrets), Application Insights (telemetry)
- **Function App** → Service Bus (event triggers), Cosmos DB (data access), Application Insights (telemetry)
- **Service Bus** → Function App (event processing via queue triggers)
- **Cosmos DB** → App Service (bookings, users), Function App (event data), Outbox Pattern storage

## References
- [System overview](../initial_outtakes/system_overview.md)  
- [Event-Driven Architecture Roadmap](../journal/eventdriven_roadmap.md)
- [ADR-001 - Cosmos DB](./ADR-001-cosmosdb.md)
- [ADR-006 - Event-Driven Architecture](./ADR-006-eventdriven.md)
- [ADR-012 - Azure App Configuration](./ADR-012-azure-app-configuration.md)
- [ADR-013 - Outbox Pattern](./ADR-013-outbox-pattern.md)
- [ADR-015 - Application Insights Telemetry Strategy](./ADR-015-application-insights-telemetry-strategy.md)
- [ADR-016 - Managed Identity & RBAC Strategy](./ADR-016-managed-identity-rbac-strategy.md)
- [Microsoft Docs – Azure App Service](https://learn.microsoft.com/en-us/azure/app-service/overview)  
- [Microsoft Docs – Azure App Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/overview)  
- [Microsoft Docs – Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/basic-concepts)  
- [Microsoft Docs – Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)  
- [Microsoft Docs – API Management](https://learn.microsoft.com/en-us/azure/api-management/api-management-key-concepts)
- [Microsoft Docs – Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/introduction)
- [Microsoft Docs – Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview)
- [Microsoft Docs – Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview)

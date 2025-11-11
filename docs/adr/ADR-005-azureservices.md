# ADR-005 – Cloud services choice: Azure App Service, App Configuration, Key Vault, Application Insights and API Management

**Status:** Accepted  
**Date:** 2025-10-30  
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

|          Service           |               Purpose                                                          |
|---------------------------|------------------------------------------------------------------------------|
| **App Service**           | Run Blazor Server and API application in the same App Service plan (Free/B1).  |
| **App Configuration**     | Centralized management of settings and feature flags between environments. |
| **Key Vault**             | Secure storage of secrets and connection strings.                            |
| **Application Insights**  | Logging, monitoring, and performance measurement via KQL.                          |
| **API Management (APIM)** | Gateway for public GET endpoints, caching, and rate limiting.                |

---

## Consequences
**Advantages:**
- Full integration with other Azure services and DevOps flows.  
- Low entry cost (F1/Serverless).  
- Centralized and secure management of configuration and secrets.  
- Easy transition to production via scaling in the App Service plan.  
- APIM provides the ability to future-proof the APIs with versioning and access control.

**Disadvantages:**
- Locked into the Azure ecosystem.  
- Limited resources in F1 plans (can "sleep" during inactivity).  
- APIM in Consumption mode can become slower than direct API access.  

---

## Risks / Mitigations
- **Risk:** App Service in F1 mode shuts down during inactivity.  
  **Mitigation:** Upgrade to B1 before demo or higher load.  

- **Risk:** Incorrect handling of secrets can lead to exposure.  
  **Mitigation:** Store all connection strings in Key Vault and use Managed Identity for access.  

- **Risk:** Unnecessary costs when testing multiple services simultaneously.  
  **Mitigation:** Activate only necessary resources during MVP, turn off Service Bus/Function until the event flow is to be demonstrated.  

---

## Alternatives
- **Container-based operations (Azure Container Apps or Azure Kubernetes Service):** Powerful but overdimensioned for MVP.  
- **Static hosting (e.g., Blazor WASM + Blob Storage):** Cheap, but lacks support for server-based login and real-time. 
- **External logging (e.g., Grafana Cloud, Elastic):** Flexible but increases complexity and operational costs.  

---

## References
- [System overview](../system_overview.md)  
- [Microsoft Docs – Azure App Service](https://learn.microsoft.com/en-us/azure/app-service/overview)  
- [Microsoft Docs – Azure App Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/overview)  
- [Microsoft Docs – Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/basic-concepts)  
- [Microsoft Docs – Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)  
- [Microsoft Docs – API Management](https://learn.microsoft.com/en-us/azure/api-management/api-management-key-concepts)

# ADR-012 – Azure App Configuration for Feature Flags: Dual-System Coexistence

**Status:** Accepted  
**Date:** 2025-11-25  
**Author:** Niklas Häll

---

## Context

The ticketing system requires a refactoring from a chained API architecture to an event-driven architecture using Azure Service Bus, Azure Functions, and the Outbox Pattern. This refactoring simulates a real-world modernization scenario where both systems must coexist permanently to enable:

- **Demonstration and comparison** of both architectures without code changes
- **Gradual migration** from synchronous to event-driven processing
- **Runtime switching** between architectures for testing and presentations
- **Production-like scenarios** where feature flags are managed operationally

The system needs a feature flag mechanism that allows toggling between:
- **Synchronous mode (default):** Traditional chained API calls, existing behavior
- **Event-driven mode:** Asynchronous event processing via Service Bus and Azure Functions

The feature flags must be:
- **Permanent** - Both systems will coexist indefinitely, not a temporary migration tool
- **Runtime-configurable** - Switch between modes without code deployment
- **Environment-specific** - Different values for dev, staging, and production
- **Easy to manage** - Accessible via Azure Portal for operational control
- **Secure** - Use managed identity authentication, no connection strings in code

---

## Decision

We use **Azure App Configuration** for managing feature flag values, with the App Configuration resource created via Bicep (infrastructure as code) but flag values managed directly in App Configuration (configuration management).

**Implementation Details:**
- **Infrastructure (Bicep):**
  - App Configuration resource created via `infra/modules/appconfiguration.bicep`
  - Free tier for dev environment, Standard tier for production
  - Managed identity access configured via RBAC (App Service gets "App Configuration Data Reader" role)
  - Endpoint and name stored in Key Vault for application access
- **Configuration (App Configuration UI):**
  - Feature flag values managed in Azure Portal App Configuration UI
  - Example: `BookingEvents:Enabled` flag (boolean) to toggle event-driven flow
  - Environment-specific values via labels (dev, staging, prod)
  - No code changes required to update flag values
- **Application Integration:**
  - `Microsoft.Extensions.Configuration.AzureAppConfiguration` NuGet package
  - Configured in `Program.cs` with managed identity authentication
  - **Sentinel key pattern** for hot-reload (no restart required)
    - Sentinel key (e.g., `Settings:Sentinel`) created in App Configuration
    - Configuration refresh watches sentinel key: when value changes, all configuration refreshes
    - Enables runtime feature flag toggling without service restart
  - Fallback to `appsettings.json` for local development
- **Dual-System Architecture:**
  - **Synchronous path (flag = false):** Bookings work as before, chained API calls
  - **Event-driven path (flag = true):** Events published to Service Bus, processed by Azure Functions
  - Both paths operate independently - no breaking changes to existing flow
  - Outbox pattern always writes events (for audit), but only publishes when flag is enabled

**Separation of Concerns:**
- **Bicep creates the resource** (infrastructure provisioning)
- **App Configuration stores the values** (configuration management)
- This follows the principle: infrastructure vs configuration separation

---

## Consequences

**Advantages:**
- **Runtime Configuration Changes** – Toggle between synchronous and event-driven modes without redeployment. Critical for demonstrations and live presentations where you need to show both architectures.
- **Live Switching** – Can demonstrate both architectures in real-time during presentations, showcasing the refactoring journey and comparing both approaches side-by-side.
- **Environment-Specific Values** – Different feature flag values for dev, staging, and production environments via labels. Enables safe testing in dev before enabling in production.
- **Feature Flag Management UI** – Azure Portal provides an intuitive interface for managing feature flags, accessible to operations teams without requiring code access or deployments.
- **Production-Like Scenarios** – Matches real-world production scenarios where feature flags are managed operationally by DevOps teams, not developers. More realistic than hardcoded values or Bicep parameters.
- **No Code Changes Needed** – Change system behavior by updating configuration only. Reduces deployment frequency and enables faster experimentation.
- **Managed Identity Authentication** – Secure access using Azure AD managed identities, no connection strings stored in code or configuration files. Follows security best practices.
- **Centralized Configuration** – All feature flags in one place, making it easy to see current system state and manage multiple flags consistently.
- **Audit Trail** – App Configuration provides change history and audit logs for feature flag modifications, useful for compliance and troubleshooting.

**Disadvantages:**
- **Additional Azure Service** – Adds another Azure resource to manage and monitor. However, Free tier is available for dev environments, minimizing cost.
- **Dependency on Azure** – Local development requires fallback to `appsettings.json` or connection to Azure. This is acceptable as the system is Azure-native.
- **Potential Latency** – Configuration reads from App Configuration may have slight latency compared to in-memory configuration. This is negligible for feature flags that are read infrequently.
- **Learning Curve** – Team needs to understand App Configuration concepts (keys, labels, feature flags) and how to manage them via Azure Portal.
- **Cost at Scale** – Free tier has limits (10,000 requests/month). Standard tier required for production workloads, but cost is reasonable for configuration management.

---

## Risks / Mitigations

- **Risk:** Feature flag changes may not propagate immediately, causing inconsistent behavior.  
  **Mitigation:** Use App Configuration refresh patterns (polling or push notifications) to ensure timely updates. Document refresh intervals and test flag propagation in dev environment.

- **Risk:** Accidental flag changes in production could disable critical functionality.  
  **Mitigation:** Implement proper RBAC on App Configuration (only authorized users can modify flags). Use labels to separate dev/staging/prod values. Consider approval workflows for production flag changes.

- **Risk:** App Configuration service outage could prevent application startup or cause runtime failures.  
  **Mitigation:** Implement fallback to `appsettings.json` for local development. Use resilient configuration loading with retry logic. Cache flag values in memory to reduce dependency on App Configuration availability.

- **Risk:** Feature flags may become technical debt if not cleaned up after migration.  
  **Mitigation:** This is intentional - flags are permanent to support dual-system coexistence. Document that both systems will coexist indefinitely as part of the refactoring simulation. Flags are not temporary migration tools.

- **Risk:** Configuration drift between environments if flags are managed manually.  
  **Mitigation:** Use labels consistently (dev, staging, prod). Document flag values in infrastructure documentation. Consider exporting/importing configuration for environment consistency.

---

## Alternatives

- **Bicep Parameters for Feature Flags** – Rejected. Bicep parameters are deployment-time only and cannot be changed without redeployment. This defeats the purpose of runtime switching and live demonstrations. Bicep is appropriate for creating the App Configuration resource (infrastructure), but not for managing flag values (configuration).

- **appsettings.json for Feature Flags** – Rejected. Requires code deployment to change flag values. Cannot support environment-specific values without multiple configuration files. No centralized management interface. Not suitable for production scenarios where operations teams need to manage flags independently of developers.

- **Environment Variables in App Service** – Rejected. Similar limitations to appsettings.json - requires deployment to change. No centralized management. Difficult to manage multiple flags across environments consistently.

- **Database-Stored Feature Flags** – Rejected. Adds unnecessary database queries for configuration reads. Requires custom management UI. App Configuration is purpose-built for this use case with better performance and built-in management tools.

- **Azure Key Vault for Feature Flags** – Rejected. Key Vault is designed for secrets, not configuration. Feature flags are not secrets - they are operational configuration. App Configuration provides better feature flag management features (labels, feature flag UI, refresh patterns).

- **Third-Party Feature Flag Services (LaunchDarkly, etc.)** – Rejected. Adds external dependency and cost. Azure App Configuration is native to Azure ecosystem, integrates with managed identity, and provides sufficient functionality for MVP needs. Can be reconsidered if advanced features (A/B testing, gradual rollouts) are needed in future.

---

## References

- [Event-Driven Architecture Roadmap](../journal/eventdriven_roadmap.md)
- [ADR-005 - Azure Services Choice](./ADR-005-azureservices.md)
- [ADR-006 - Event-Driven Architecture](./ADR-006-eventdriven.md)
- [App Configuration Bicep Module](../../infra/modules/appconfiguration.bicep)
- [Azure App Configuration Documentation](https://learn.microsoft.com/en-us/azure/azure-app-configuration/)


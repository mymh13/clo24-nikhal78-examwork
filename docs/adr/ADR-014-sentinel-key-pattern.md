# ADR-014 – Sentinel Key Pattern: Hot-Reload Configuration Without Service Restart

**Status:** Accepted  
**Date:** 2025-11-27  
**Author:** Niklas Häll

---

## Context

The ticketing system implements a **permanent dual-system coexistence** architecture where both synchronous (chained API) and event-driven architectures run side-by-side. Feature flags in Azure App Configuration control which architecture path is active. This enables live demonstrations and runtime switching between modes without code changes.

**The Problem:**
- Azure App Configuration supports configuration refresh, but by default requires watching individual keys
- Feature flags and configuration values can change in App Configuration, but the application won't pick them up until restart
- For live demonstrations and presentations, we need to **toggle feature flags at runtime** without restarting the service
- Restarting the service causes:
  - **Downtime** - service unavailable during restart
  - **Lost connections** - active user sessions are terminated
  - **Poor user experience** - cannot demonstrate real-time switching
  - **Operational overhead** - requires deployment or manual restart

**Without Sentinel Key Pattern:**
```
1. Update feature flag in App Configuration (BookingEvents_Enabled = true)
2. Application still uses cached value (false)
3. Must restart service to pick up new value
4. Service unavailable during restart
5. Users lose active sessions
Result: Cannot demonstrate live switching, poor UX
```

**The Requirement:**
- **Hot-reload configuration** - pick up App Configuration changes without service restart
- **Zero-downtime switching** - toggle between architectures while service is running
- **Live demonstrations** - show both architectures in real-time during presentations
- **Operational flexibility** - change feature flags without deployment
- **Automatic refresh** - application should detect and apply changes automatically

---

## Decision

We implement the **Sentinel Key Pattern** for hot-reloading Azure App Configuration values. This pattern uses a dedicated "sentinel" key that, when its value changes, triggers a refresh of all configuration values (including feature flags).

**Implementation Details:**
- **Sentinel Key:** `Settings:Sentinel` in App Configuration
- **Refresh Configuration:** `refresh.Register("Settings:Sentinel", refreshAll: true)`
  - When sentinel key value changes, all configuration refreshes automatically
  - `refreshAll: true` ensures all keys (not just watched keys) are refreshed
- **Refresh Interval:** `SetRefreshInterval(TimeSpan.FromMinutes(1))` - checks for changes every minute
- **Middleware:** `app.UseAzureAppConfiguration()` must be added to the HTTP pipeline
  - This middleware triggers the refresh check on each HTTP request
  - Without this middleware, hot-reload will not work (only restart will pick up changes)
  - Should be placed early in the pipeline, after static files but before routing
- **Cache Expiration:** 1 minute for feature flags and configuration refresh
- **Fallback:** Sentinel key also exists in `appsettings.json` with initial value "1" for local development

**Flow:**
```
1. Feature flag changed in App Configuration (BookingEvents_Enabled = true)
   ↓
2. Update sentinel key value (Settings:Sentinel = "2" → "3")
   ↓
3. Application polls sentinel key (every 1 minute)
   ↓
4. Detects sentinel value changed
   ↓
5. Refreshes ALL configuration (refreshAll: true)
   ↓
6. Feature flag value updated in application
   ↓
7. Next booking uses new architecture path
   ↓
8. No service restart required - zero downtime
```

**Key Properties:**
- **Single Trigger Key:** One sentinel key triggers refresh of all configuration
- **Atomic Refresh:** All configuration refreshes together, preventing partial updates
- **Automatic Detection:** Application polls for changes, no manual intervention needed
- **Zero Downtime:** Configuration updates without service restart
- **Operational Simplicity:** Update sentinel value to trigger refresh (simple increment)

**Usage:**
1. Change feature flag value in App Configuration UI
2. Update sentinel key value (increment: "1" → "2" → "3", etc.)
3. Wait up to 1 minute for automatic refresh
4. Configuration and feature flags are updated
5. Application uses new values immediately

---

## Consequences

**Advantages:**
- **Zero-Downtime Updates** – Configuration and feature flags can be updated without service restart. Critical for production environments and live demonstrations.
- **Live Demonstrations** – Can switch between synchronous and event-driven architectures in real-time during presentations. Shows both approaches side-by-side without code changes.
- **Operational Flexibility** – Operators can change feature flags without deployment or code changes. Enables A/B testing, gradual rollouts, and emergency toggles.
- **Better User Experience** – No service interruption when updating configuration. Active user sessions remain connected.
- **Automatic Refresh** – Application automatically detects and applies changes. No manual intervention required.
- **Atomic Updates** – All configuration refreshes together (`refreshAll: true`), preventing partial updates that could cause inconsistent state.
- **Simple Operation** – Updating sentinel key value is straightforward (just increment a number). No complex procedures required.
- **Cost Effective** – No additional infrastructure needed. Uses existing App Configuration refresh capabilities.

**Disadvantages:**
- **Polling Overhead** – Application polls App Configuration every minute, consuming resources even when no changes occur. However, this is minimal (single key check).
- **Refresh Delay** – Changes take up to 1 minute to propagate (refresh interval). For immediate updates, would need to reduce interval (increases polling overhead).
- **Sentinel Key Management** – Requires discipline to update sentinel key when changing configuration. If forgotten, changes won't be picked up until next sentinel update.
- **Cache Complexity** – Configuration is cached, and refresh logic adds complexity to configuration pipeline. Requires understanding of refresh behavior.
- **Potential Race Conditions** – If multiple configuration values change simultaneously, refresh happens atomically but timing could cause brief inconsistencies (mitigated by `refreshAll: true`).

---

## Risks / Mitigations

- **Risk:** Sentinel key value not updated when changing feature flags, causing changes to not be picked up.  
  **Mitigation:** Document the process clearly. Consider automation (script or Azure Function) to automatically update sentinel when feature flags change. Add monitoring/alerting if sentinel hasn't changed in expected timeframe.

- **Risk:** Refresh interval too long (1 minute) causes delays in configuration updates during demonstrations.  
  **Mitigation:** Can reduce refresh interval to 30 seconds or less for faster updates. Balance between responsiveness and polling overhead. For production, 1 minute is acceptable.

- **Risk:** Multiple operators updating sentinel key simultaneously could cause refresh conflicts.  
  **Mitigation:** Sentinel key updates are simple increments, conflicts are unlikely. App Configuration handles concurrent updates. Refresh is idempotent - multiple refreshes are safe.

- **Risk:** Application crashes or restarts before refresh completes, losing configuration updates.  
  **Mitigation:** Configuration is persisted in App Configuration. On restart, application loads latest values. Refresh is for runtime updates, not persistence.

- **Risk:** Polling overhead increases costs or impacts performance at scale.  
  **Mitigation:** Polling is lightweight (single key check). Refresh interval can be increased if needed. Consider push-based refresh (Azure Functions or Logic Apps) for future enhancement if polling becomes bottleneck.

- **Risk:** Configuration refresh fails silently, leaving application with stale values.  
  **Mitigation:** Implement health checks to verify configuration refresh is working. Monitor sentinel key value in health endpoint. Log refresh events for observability.

---

## Alternatives

- **Individual Key Watching** – Rejected. Would require watching every feature flag and configuration key individually. Complex to maintain, easy to miss keys. Sentinel pattern is simpler and more maintainable.

- **Push-Based Refresh (Webhooks)** – Considered for future. Azure App Configuration can trigger webhooks when values change. Would eliminate polling overhead. However, requires additional infrastructure (webhook endpoint, Azure Functions, or Logic Apps). Can be implemented as Phase 10 enhancement if polling becomes bottleneck.

- **Manual Service Restart** – Rejected. Causes downtime, poor user experience, and operational overhead. Not suitable for production or live demonstrations.

- **Configuration Reload Endpoint** – Rejected. Requires exposing administrative endpoint, security concerns, and manual intervention. Sentinel pattern is automatic and doesn't require API changes.

- **Change Feed (Cosmos DB Style)** – Not applicable. App Configuration doesn't have change feed. Would need to build custom solution, adding complexity.

- **Longer Refresh Intervals** – Considered. Could increase to 5-10 minutes to reduce polling. However, 1 minute provides good balance between responsiveness and overhead. Can be adjusted per environment (shorter for dev, longer for prod).

- **No Refresh (Restart Required)** – Rejected. Doesn't meet requirement for zero-downtime updates and live demonstrations. Poor operational experience.

---

## References

- [Event-Driven Architecture Roadmap](../journal/eventdriven_roadmap.md)
- [ADR-006 - Event-Driven Architecture](./ADR-006-eventdriven.md)
- [ADR-012 - Azure App Configuration](./ADR-012-azure-app-configuration.md)
- [Azure App Configuration - Refresh Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-dotnet-core)
- [Sentinel Key Pattern - Microsoft Docs](https://learn.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-dotnet-core#refreshall)


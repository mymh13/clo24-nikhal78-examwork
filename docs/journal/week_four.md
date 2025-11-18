# Week 4 – Infrastructure Expansion and Application Integration

## Overview

During week 4, work focused on expanding the infrastructure foundation with Azure Key Vault and preparing for application integration with Cosmos DB. The goal was to establish secure secret management capabilities and begin connecting the application to data storage.

---

## Completed Activities

### Infrastructure Setup
- **Key Vault Module:** Created `infra/modules/keyvault.bicep` with modular architecture following the established pattern.
- **Key Vault Deployment:** Successfully deployed Azure Key Vault `examwork-kv-dev` in Sweden Central.

### Azure Key Vault Setup
- **Resource:** Deployed Key Vault `examwork-kv-dev` in Sweden Central (Standard SKU).
- **Configuration:** 
  - Soft delete enabled (7-day retention period) for data protection.
  - Purge protection disabled for dev environment (can be enabled for production).
  - RBAC-ready configuration (no access policies; will use Azure RBAC when granting access).
  - Standard SKU selected for cost-effective secret management.
- **Integration:** Added Key Vault module to `infra/env/dev/main.bicep` deployment.
- **Deployment Challenge:** Initial deployment failed due to Azure API restriction - cannot explicitly set `enablePurgeProtection: false`. Resolved by conditionally including the property only when `true` using Bicep's `union()` function to merge objects.
- **Outputs:** Key Vault name and URI exposed as deployment outputs for future reference.
- **Status:** Key Vault is ready for secret storage. Access policies/RBAC can be configured when secrets are added (e.g., Cosmos DB connection strings).

### Application Insights Verification
- **Configuration Check:** Verified `APPLICATIONINSIGHTS_CONNECTION_STRING` is correctly set in App Service app settings.
- **Resource Status:** Confirmed Application Insights resource `examwork-insights-dev` is active and accessible.
- **Application Status:** Verified application is running and responding (HTTP 200) at `https://ticket.mymh.dev`.
- **Code Integration:** Confirmed Application Insights SDK (`Microsoft.ApplicationInsights.AspNetCore` v2.22.0) is installed and `AddApplicationInsightsTelemetry()` is configured in `Program.cs`.
- **Telemetry Collection:** Application is configured to automatically send telemetry. Telemetry data should appear in Application Insights portal within 2-5 minutes after requests are made.
- **Verification Method:** Used Azure CLI to verify configuration and resource status. Portal verification recommended for viewing actual telemetry data (requests, page views, performance metrics).
- **Status:** Application Insights is properly configured and ready to collect telemetry. Telemetry will be visible in Azure Portal after application usage.

### API Containerization
- [Document API Docker setup if completed]

### Cosmos DB Integration
- **Approach:** Implemented minimal Cosmos DB integration using iterative, test-driven approach (small steps with validation).
- **Booking Model:** Created minimal `Booking` entity in `Ticketing.Contracts/Bookings/Booking.cs` with essential properties:
  - `Id` (GUID, document ID)
  - `CustomerId` (partition key)
  - `CustomerName`
  - `BookingDate`
- **SDK Integration:** Added `Microsoft.Azure.Cosmos` v3.45.0 NuGet package to `Ticketing.Web`.
- **Configuration:** 
  - Added minimal Cosmos DB client configuration in `Program.cs`.
  - Connection string loaded from `appsettings.Development.local.json` (gitignored, contains secrets).
  - CosmosClient registered as singleton in dependency injection.
- **Connection Validation:** 
  - Implemented startup connection test to validate Cosmos DB access.
  - Test verifies connection to `ticketing` database and `bookings` container.
  - Connection test runs asynchronously on application startup and logs success/failure.
- **Local Testing:** 
  - Tested locally with connection string from Azure Cosmos DB account.
  - Verified connection string loading, client registration, and successful database/container access.
  - Connection test confirmed: "Cosmos DB connected successfully: Database 'ticketing' Container 'bookings'".
- **Status:** Minimal integration complete and validated. Ready for service layer implementation in next iteration.

### Frontend & UI
- [Document any Blazor landing page or UI improvements]

### CI/CD Pipelines
- [Document any pipeline updates or optimizations]

### Documentation
- [Document any ADRs, documentation updates, or journal entries]

---

## Reflection

[Reflect on the week's work, challenges encountered, lessons learned, and key achievements. What went well? What could be improved?]

---

## Next Steps (Week 5)

### Planned Development Phases (Iterative Approach)

**Phase 1: Basic Authentication Structure**
- Implement minimal authentication system with hardcoded admin user
- Use same structure as future ASP.NET Identity/Entra ID integration
- Add `[Authorize]` attributes to protect endpoints from day one
- Create simple test page to validate auth structure works
- **Goal:** Establish auth architecture early to avoid refactoring later

**Phase 2: Basic Data Operations**
- Create minimal Cosmos DB service with `CreateBookingAsync` method
- Test creating booking documents in Cosmos DB
- Add query method to read bookings by customerId
- Validate data operations work end-to-end

**Phase 3: Secrets Management**
- Store Cosmos DB connection string in Azure Key Vault
- Update application to read connection string from Key Vault
- Test locally first, then update App Service configuration
- Validate secure secret management workflow

---


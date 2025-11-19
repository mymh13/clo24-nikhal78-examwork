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
- **Service Layer Implementation:**
  - Created `IBookingService` interface and `BookingService` implementation.
  - Implemented `CreateBookingAsync` method (creates bookings with partition key support).
  - Implemented `GetBookingsByCustomerIdAsync` method (queries bookings by customerId using partition key).
  - Registered `BookingService` as scoped service in dependency injection.
- **API Endpoints:**
  - Created `BookingsController` with `[Authorize]` protection.
  - `POST /api/bookings` - Create new booking.
  - `GET /api/bookings/customer/{customerId}` - Get all bookings for a customer.
- **Testing:**
  - Created test page `/test-bookings` for manual API testing.
  - Configured HttpClient with base address for Blazor Server API calls.
  - Validated service layer architecture and endpoint structure.
- **Local Testing:** 
  - Tested locally with connection string from Azure Cosmos DB account.
  - Verified connection string loading, client registration, and successful database/container access.
  - Connection test confirmed: "Cosmos DB connected successfully: Database 'ticketing' Container 'bookings'".
- **Status:** Service layer complete and validated. Ready for secrets management integration (Phase 3).
- **Secrets Management Integration:**
  - Connection string now stored in Azure Key Vault (`CosmosDb--ConnectionString`).
  - Application configured to read from Key Vault using `DefaultAzureCredential`.
  - Local development: Falls back to `appsettings.Development.local.json` if Key Vault unavailable (requires `az login` for Key Vault access).
  - App Service: Uses managed identity to automatically access Key Vault (no secrets in app settings).
  - Key Vault configured with RBAC authorization (modern approach, no access policies).
  - **Status:** Secrets management complete. Connection string securely stored in Key Vault, application configured for both local and Azure environments.

### Authentication (Phase 1)
- **Approach:** Implemented minimal authentication structure following iterative, test-driven approach.
- **Implementation:**
  - Created cookie-based authentication using ASP.NET Core's built-in authentication middleware.
  - Same structure as ASP.NET Identity/Entra ID (can swap authentication scheme later without code changes).
  - Hardcoded admin user for development/testing (will be replaced with real auth later).
- **Components:**
  - `AuthController` with `/api/auth/login` and `/api/auth/logout` endpoints.
  - `Login.razor` page with form-based login.
  - `TestAuth.razor` protected page with `[Authorize]` attribute for validation.
- **Testing:** 
  - Validated login flow: form submission → cookie creation → authenticated state.
  - Validated logout flow: cookie deletion → unauthenticated state.
  - Validated protected page access: redirects to login when not authenticated.
- **Status:** Phase 1 complete. Authentication structure established and validated. All future endpoints can use `[Authorize]` from day one.

### System Architecture
- **Code Organization:** Refactored `Program.cs` to use extension method pattern (reduced from 95 to 19 lines).
  - Created `ServiceCollectionExtensions` for service registration (Application Insights, Cosmos DB, Authentication, etc.).
  - Created `WebApplicationExtensions` for pipeline configuration and startup validation.
  - Created `ConfigurationExtensions` for configuration loading.
- **Benefits:** Improved code readability, maintainability, and separation of concerns. Follows ASP.NET Core conventions and makes `Program.cs` minimal and focused.
- **Decision:** Documented in ADR-009: Extension Methods Pattern for Application Startup Configuration.

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

**Phase 1: Basic Authentication Structure** **COMPLETE**
- Implemented minimal authentication system with hardcoded admin user
- Using same structure as future ASP.NET Identity/Entra ID integration
- `[Authorize]` attributes working to protect endpoints
- Test page validates auth structure works
- **Goal:** Establish auth architecture early to avoid refactoring later - **ACHIEVED**

**Phase 2: Basic Data Operations** **COMPLETE**
- Created minimal Cosmos DB service (`IBookingService` interface and `BookingService` implementation)
- Implemented `CreateBookingAsync` method for creating booking documents in Cosmos DB
- Implemented `GetBookingsByCustomerIdAsync` method to query bookings by customerId
- Created `BookingsController` with POST and GET endpoints (`/api/bookings` and `/api/bookings/customer/{customerId}`)
- Registered `BookingService` as scoped service in dependency injection
- Created test page (`/test-bookings`) for manual API testing
- Validated service layer architecture and Cosmos DB integration
- **Goal:** Establish data operations foundation - **ACHIEVED**

**Phase 3: Secrets Management** **COMPLETE**
- Added Azure Key Vault configuration provider NuGet packages (`Azure.Extensions.AspNetCore.Configuration.Secrets`, `Azure.Identity`)
- Updated `ConfigurationExtensions` to load secrets from Key Vault using `DefaultAzureCredential`
- Stored Cosmos DB connection string in Key Vault as `CosmosDb--ConnectionString`
- Enabled RBAC authorization on Key Vault (updated Bicep to enable by default)
- Enabled managed identity on App Service (SystemAssigned)
- Granted App Service managed identity "Key Vault Secrets User" role for Key Vault access
- Updated Bicep to pass Key Vault name to App Service app settings
- Created `appsettings.json` with Key Vault name configuration (safe to commit, no secrets)
- Validated local testing with fallback to `appsettings.Development.local.json` when Key Vault unavailable
- **Goal:** Secure secret management with Key Vault integration - **ACHIEVED**

---


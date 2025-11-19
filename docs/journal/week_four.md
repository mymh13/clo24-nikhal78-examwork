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
- **Cosmos DB Serialization Fix:**
  - Fixed `id` property serialization issue - Cosmos DB requires lowercase `id` property in JSON documents.
  - Configured `CosmosClient` with `CosmosPropertyNamingPolicy.CamelCase` to automatically convert C# property names (e.g., `Id`) to camelCase JSON (e.g., `id`).
  - Ensured `Id` and `BookingDate` are always set before sending to Cosmos DB (form submissions may not include these).
  - **Status:** Booking creation fully functional. Documents correctly stored in Cosmos DB with proper `id` property.
- **Secrets Management Integration:**
  - Connection string now stored in Azure Key Vault (`CosmosDb--ConnectionString`).
  - Application configured to read from Key Vault using `DefaultAzureCredential`.
  - Local development: Falls back to `appsettings.Development.local.json` if Key Vault unavailable (requires `az login` for Key Vault access).
  - App Service: Uses managed identity to automatically access Key Vault (no secrets in app settings).
  - Key Vault configured with RBAC authorization (modern approach, no access policies).
  - **Azure Deployment Issue:** Initially, App Service managed identity lacked "Key Vault Secrets User" role, preventing connection string retrieval.
  - **Resolution:** Granted App Service managed identity the "Key Vault Secrets User" role on Key Vault using Azure CLI, then restarted App Service.
  - **Status:** Secrets management complete. Connection string securely stored in Key Vault, application configured for both local and Azure environments. Cosmos DB client now registers correctly in Azure.

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
- **Code Cleanup:**
  - Removed debug logging and excessive comments from production code.
  - Cleaned up verbose console output while keeping essential startup validation logs.
  - Simplified error handling messages.
  - Removed redundant inline comments.
  - **Status:** Code is production-ready and clean, ready for Azure deployment.

### Frontend & UI
- **Health Check Endpoint:** Created `/api/health` endpoint and `/health` page for system health monitoring.
  - Shows non-sensitive configuration status (Key Vault name, Cosmos DB connection status, Application Insights configuration).
  - Tests Cosmos DB connection without exposing secrets.
  - Visual health status page with color-coded indicators.
  - Useful for verifying Key Vault integration and overall system health after deployments.

### CI/CD Pipelines
- **Workflow Cleanup:** Simplified CI/CD workflows by removing unnecessary complexity and bloat.
  - Removed timestamp-based versioning attempts, reverted to simple SHA-based tagging.
  - Cleaned up CD workflow to use straightforward `az webapp config set` command.
  - CI workflow now pushes images with SHA tag (`web:SHORT_SHA`) and `latest` tag.
  - CD workflow extracts SHA from CI workflow run and updates App Service to use specific SHA tag.
- **Deployment Issues & Resolution:**
  - Encountered persistent "Bad Request" errors when trying to update `linuxFxVersion` via Azure CLI.
  - Issue occurred after enabling managed identity for Key Vault access.
  - Root cause: Cached state in App Service configuration preventing updates.
  - **Solution:** Deleted and recreated App Service via Bicep to clear cached state.
  - After recreation, CD workflow successfully updates container image tags.
- **Configuration Fix:**
  - Fixed Bicep app setting: Changed `KeyVault:Name` to `KeyVault__Name` (Azure doesn't allow colons in app setting names).
  - ASP.NET Core automatically converts `__` to `:` when reading configuration, so code remains unchanged.
- **Status:** CI/CD pipelines working correctly. SHA-based versioning ensures unique tags for each deployment, forcing Azure to pull new images.

### Documentation
- **ADR-008:** Documented Docker container deployment strategy (migration from Oryx/zip-deploy to GHCR containerization).
- **ADR-009:** Documented Extension Methods Pattern for Application Startup Configuration.
- **Week 4 Journal:** Comprehensive documentation of infrastructure expansion, Cosmos DB integration, authentication implementation, and secrets management.

---

## Reflection

### What Went Well
- **Infrastructure Expansion:** Successfully deployed Key Vault and integrated it with the application using managed identities and RBAC. The modern RBAC approach is cleaner than legacy access policies.
- **Iterative Development:** The phased approach (Authentication → Data Operations → Secrets Management) worked well, allowing us to validate each component before moving to the next.
- **Code Organization:** Refactoring `Program.cs` to use extension methods significantly improved code readability and maintainability. The separation of concerns makes the codebase easier to understand and modify. I want to highlight that this was the plan already from the start, but I do not start off by creating the extension methods until Program.c starts to become unreadable.
- **Cosmos DB Integration:** Successfully integrated Cosmos DB with proper serialization configuration. The camelCase naming policy automatically handles the `id` property requirement.
- **Health Monitoring:** The health check endpoint and page proved great for diagnosing configuration issues in Azure, especially the missing RBAC permission.

### Challenges Encountered
- **Cosmos DB Serialization:** Initial issue with `id` property not being serialized correctly. Resolved by configuring `CosmosPropertyNamingPolicy.CamelCase` on the CosmosClient.
- **Key Vault RBAC Permissions:** Discovered that granting managed identity permission to Key Vault was a manual step not included in the initial Bicep deployment. This caused the Cosmos DB client to not register in Azure, leading to "Unable to resolve service" errors.
- **Configuration Key Formats:** Had to handle both `KeyVault:Name` (from appsettings.json) and `KeyVault__Name` (from Azure app settings) due to Azure's restriction on colons in app setting names.

### Lessons Learned
- **Always verify RBAC permissions:** When using managed identities with Key Vault, the role assignment must be explicitly granted. This should ideally be automated in Bicep or documented as a required manual step.
- **Health checks are essential:** The health check endpoint quickly revealed the root cause of the Cosmos DB registration issue, saving significant debugging time.
- **Configuration flexibility:** Supporting both `:` and `__` formats for configuration keys provides better compatibility between local development and Azure environments.
- **Code cleanup matters:** Removing debug logging and excessive comments before production deployment improves code quality and maintainability.

### Key Achievements
- ✅ Complete infrastructure foundation: App Service, Cosmos DB, Application Insights, and Key Vault all deployed and integrated.
- ✅ Secure secrets management: Connection strings stored in Key Vault, accessed via managed identity (no secrets in code or app settings).
- ✅ Functional booking system: Create and retrieve bookings working end-to-end with Cosmos DB.
- ✅ Authentication structure: Cookie-based auth with `[Authorize]` protection working correctly.
- ✅ Production-ready code: Clean, well-organized codebase following ASP.NET Core best practices.
- ✅ CI/CD pipelines: Automated Docker builds and deployments working correctly with SHA-based versioning.

### What Could Be Improved
- **Automate RBAC assignments:** Consider adding role assignments to Bicep templates or documenting them as required post-deployment steps.
- **Error handling:** Could improve error messages when services fail to register (e.g., more descriptive errors when Cosmos DB connection string is missing).
- **Testing:** Add automated integration tests for the booking API endpoints to catch issues earlier.

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
- Fixed Cosmos DB serialization: Configured camelCase naming policy to ensure `id` property is correctly serialized
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


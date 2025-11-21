# Week 4 – Infrastructure Expansion and Application Integration

## Overview

During week 4, work focused on expanding the infrastructure foundation with Azure Key Vault and preparing for application integration with Cosmos DB. The goal was to establish secure secret management capabilities and begin connecting the application to data storage.

---

## Completed Activities

### Infrastructure Setup
- **Key Vault Module:** Created `infra/modules/keyvault.bicep` with modular architecture following the established pattern.
- **Key Vault Deployment:** Successfully deployed Azure Key Vault `examwork-kv-dev` in Sweden Central.

### Azure Key Vault Setup
- **Resource:** Deployed Key Vault `examwork-kv-dev` in Sweden Central (Standard SKU) with soft delete enabled (7-day retention) and RBAC authorization.
- **Deployment Challenge:** Initial deployment failed due to Azure API restriction on `enablePurgeProtection: false`. Resolved by conditionally including the property only when `true` using Bicep's `union()` function.
- **Status:** Key Vault ready for secret storage with RBAC configuration.

### Application Insights Verification
- **Configuration:** Verified Application Insights SDK integration and connection string configuration. Application is running and responding at `https://ticket.mymh.dev`.
- **Status:** Application Insights properly configured and ready to collect telemetry data.

### API Containerization
- [Document API Docker setup if completed]

### Cosmos DB Integration
- **Approach:** Implemented minimal Cosmos DB integration using iterative, test-driven approach.
- **Booking Model & Service:** Created `Booking` entity with essential properties (Id, CustomerId, CustomerName, BookingDate) and implemented `IBookingService` with create and query methods.
- **API Endpoints:** Created `BookingsController` with `POST /api/bookings` and `GET /api/bookings/customer/{customerId}` endpoints, protected with `[Authorize]`.
- **Serialization Fix:** Fixed `id` property serialization by configuring `CosmosPropertyNamingPolicy.CamelCase` on CosmosClient to ensure proper JSON document format.
- **Secrets Management:** Connection string stored in Key Vault (`CosmosDb--ConnectionString`), accessed via managed identity in Azure and `DefaultAzureCredential` locally. Resolved initial RBAC permission issue by granting App Service managed identity "Key Vault Secrets User" role.
- **Status:** Booking system fully functional with secure secrets management. Cosmos DB client registers correctly in both local and Azure environments.

### Authentication (Phase 1)
- **Approach:** Implemented minimal cookie-based authentication structure using ASP.NET Core's built-in middleware, designed to be easily swapped for Entra ID later.
- **Components:** Created `AuthController` with login/logout endpoints, `Login.razor` page with form-based login, and protected test page with `[Authorize]` validation.
- **Status:** Phase 1 complete. Authentication structure established and validated. All future endpoints can use `[Authorize]` from day one.

### Azure Entra ID Authentication (Admin Users)
- **Approach:** Implemented Azure Entra ID authentication for administrator users following ADR-002 dual authentication model. Added OpenID Connect package and configured dual authentication schemes (Cookie default, OpenID Connect challenge).
- **Azure AD App Registration:** Created app registration `examwork-web-dev` with "Web" platform, configured redirect URIs for local and production environments, and set front-channel logout URL (single URL limitation). Created client secret for authorization code flow.
- **Configuration:** OpenID Connect configured with authorization code flow, client secret authentication, token validation, automatic Admin role mapping, and claims extraction. Secrets (Client ID, Tenant ID, Client Secret) stored in Key Vault, accessed via managed identity in Azure.
- **Authentication Flow:** Fixed redirect URI to use HTTPS behind Azure App Service load balancer using forwarded headers. Implemented cookie sign-in after OpenID Connect token validation to ensure user is recognized as authenticated. Configured redirect to `/admin` after successful authentication.
- **UI & Flow:** Login page conditionally shows "Sign in with Microsoft" button when Azure AD is configured. Logout redirects to login page with success message. Standard login form remains for future non-Entra ID users. Created Error page for proper error handling.
- **Status:** Azure Entra ID authentication fully implemented and working. Admin users can authenticate via Microsoft accounts with proper role mapping, cookie sign-in, and redirect flow. Authentication restricted to single tenant (only users in the Azure AD tenant can log in).

### System Architecture
- **Code Organization:** Refactored `Program.cs` to use extension method pattern (reduced from 95 to 19 lines). Created extension methods for service registration, pipeline configuration, and configuration loading. Documented in ADR-009.
- **Code Cleanup:** Removed debug logging and excessive comments, cleaned up verbose output while keeping essential validation logs, simplified error handling.
- **Status:** Code is production-ready and clean, following ASP.NET Core best practices.

### Frontend & UI
- **Health Check Endpoint:** Created `/api/health` endpoint and `/health` page showing non-sensitive configuration status and Cosmos DB connection health with color-coded indicators.
- **Login Page Enhancements:** Added functional login form with conditional Entra ID button, error/success message display, styled to match dark theme.
- **Admin Landing Page:** Merged test pages into `AdminLandingPage.razor` displaying user information and booking management tools with logout functionality. Enhanced booking UI with table display for bookings and user-friendly success/error messages.
- **CSS Refactoring:** Refactored `custom.css` to use CSS variables (custom properties) for all colors, spacing, and common values. Reorganized into component-based sections (Base Styles, Components, Layout, Page-Specific, Responsive) eliminating repetition and improving maintainability. Single-file approach chosen for project scale with clear structure for future expansion.

### CI/CD Pipelines
- **Workflow Cleanup:** Simplified workflows by removing unnecessary complexity, reverted to SHA-based tagging. CI pushes images with SHA and `latest` tags, CD extracts SHA and updates App Service.
- **Deployment Issues & Resolution:** Encountered persistent "Bad Request" errors due to cached App Service state after enabling managed identity. Resolved by deleting and recreating App Service via Bicep to clear cached state.
- **Configuration Fix:** Fixed Bicep app setting format (`KeyVault:Name` to `KeyVault__Name`) due to Azure's restriction on colons in app setting names. ASP.NET Core automatically converts `__` to `:` when reading configuration.
- **Status:** CI/CD pipelines working correctly with SHA-based versioning ensuring unique tags for each deployment.

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
- **CSS Architecture:** Refactoring CSS to use variables and component-based organization eliminated repetition and created a maintainable styling foundation. Theme changes can now be made by updating a few CSS variables at the top of the file.
- **Cosmos DB Integration:** Successfully integrated Cosmos DB with proper serialization configuration. The camelCase naming policy automatically handles the `id` property requirement.
- **Health Monitoring:** The health check endpoint and page proved great for diagnosing configuration issues in Azure, especially the missing RBAC permission.

### Challenges Encountered
- **Cosmos DB Serialization:** Initial issue with `id` property not being serialized correctly. Resolved by configuring `CosmosPropertyNamingPolicy.CamelCase` on the CosmosClient.
- **Key Vault RBAC Permissions:** Discovered that granting managed identity permission to Key Vault was a manual step not included in the initial Bicep deployment. This caused the Cosmos DB client to not register in Azure, leading to "Unable to resolve service" errors.
- **Configuration Key Formats:** Had to handle both `KeyVault:Name` (from appsettings.json) and `KeyVault__Name` (from Azure app settings) due to Azure's restriction on colons in app setting names.
- **Azure AD Platform Configuration:** Initially selected "Public client/native" platform instead of "Web" platform for redirect URIs. Corrected to use "Web" platform which is appropriate for Blazor Server applications.
- **Front-Channel Logout URL Limitation:** Azure AD app registration only allows one front-channel logout URL per app. Cannot configure separate URLs for local and production environments. Using production URL for both (works for local testing as well).
- **Client Secret Requirement:** Azure AD requires client secret for authorization code flow. Initially missing client secret configuration caused authentication failures. Resolved by creating client secret in Azure AD and storing it in Key Vault.
- **Redirect URI HTTPS Issue:** App Service behind load balancer was sending HTTP redirect URIs instead of HTTPS. Resolved by configuring forwarded headers middleware and explicitly setting HTTPS redirect URI in OpenID Connect events.
- **Cookie Sign-In After OpenID Connect:** Users authenticated via Entra ID were not recognized as logged in because they were only authenticated via OpenID Connect scheme, not the default Cookie scheme. Resolved by implementing cookie sign-in in `OnTokenValidated` event.

### Lessons Learned
- **Always verify RBAC permissions:** When using managed identities with Key Vault, the role assignment must be explicitly granted. This should ideally be automated in Bicep or documented as a required manual step.
- **Health checks are essential:** The health check endpoint quickly revealed the root cause of the Cosmos DB registration issue, saving significant debugging time.
- **Configuration flexibility:** Supporting both `:` and `__` formats for configuration keys provides better compatibility between local development and Azure environments.
- **Code cleanup matters:** Removing debug logging and excessive comments before production deployment improves code quality and maintainability.

### Key Achievements
- Complete infrastructure foundation: App Service, Cosmos DB, Application Insights, and Key Vault all deployed and integrated.
- Secure secrets management: Connection strings and Azure AD secrets stored in Key Vault, accessed via managed identity (no secrets in code or app settings).
- Functional booking system: Create and retrieve bookings working end-to-end with Cosmos DB.
- Authentication structure: Cookie-based auth with `[Authorize]` protection working correctly.
- Azure Entra ID integration: Admin users can authenticate via Microsoft accounts with proper role mapping, cookie sign-in, and redirect flow. Authentication fully functional in production.
- Production-ready code: Clean, well-organized codebase following ASP.NET Core best practices. Removed debug logging and unnecessary code.
- CI/CD pipelines: Automated Docker builds and deployments working correctly with SHA-based versioning.
- Dual authentication model: Foundation established for both Entra ID (admin/inspector) and local Identity (customers) as per ADR-002.

### What Could Be Improved
- **Automate RBAC assignments:** Consider adding role assignments to Bicep templates or documenting them as required post-deployment steps.
- **Error handling:** Could improve error messages when services fail to register (e.g., more descriptive errors when Cosmos DB connection string is missing).
- **Testing:** Add automated integration tests for the booking API endpoints to catch issues earlier.

---

## Ongoing Work (Week 4 - Continued)

### Frontend Improvements
- **Landing Page Navigation:** Added functional buttons to home page for navigation to login and health check pages, styled to match dark theme.
- **Authentication UI:** Login page with conditional Entra ID button, admin landing page with user info and booking management, logout flow with success message.
- **Error Handling:** Created Error page for proper error display. Improved authentication error handling with user-friendly error messages on login page.
- **CSS Architecture:** Completed CSS refactoring with CSS variables and component-based organization. All styling now uses centralized design tokens, making theme changes and maintenance significantly easier.

---

### Planned Development Phases (Iterative Approach)

**Phase 1: Basic Authentication Structure** **COMPLETE**
- Implemented minimal cookie-based authentication with hardcoded admin user, using same structure as future Entra ID integration. `[Authorize]` attributes working to protect endpoints.
- **Goal:** Establish auth architecture early to avoid refactoring later - **ACHIEVED**

**Phase 2: Basic Data Operations** **COMPLETE**
- Created `IBookingService` and `BookingService` with create and query methods. Implemented `BookingsController` with POST and GET endpoints. Fixed Cosmos DB serialization with camelCase naming policy.
- **Goal:** Establish data operations foundation - **ACHIEVED**

**Phase 3: Secrets Management** **COMPLETE**
- Integrated Azure Key Vault using `DefaultAzureCredential`, stored Cosmos DB connection string in Key Vault, enabled RBAC authorization and managed identity on App Service. Validated local testing with fallback to local config.
- **Goal:** Secure secret management with Key Vault integration - **ACHIEVED**

---

## Next Steps (Week 5)
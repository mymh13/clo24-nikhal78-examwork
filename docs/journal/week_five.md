# Week 5 – Feature Development and Ticket Management

## Brainstorming & Planned Progression

### Core Features (Priority Order)
1. **Complete login options** for users and inspectors (staff)
2. **Ticket attributes** - regions, zones, age, price (with student/child/pensioner discounts via percentage adjustments)
3. **Ticket activation timer** - dual triggers:
   - Manual start time selection (user landing page interface under "active tickets")
   - Secondary trigger on "activation" (QR code scan when boarding transportation)
4. **Event triggers API functionality** - implement event-driven ticket validation
5. **QR code generation** for ticket scanning

### Bonus Features
- **Bonus A:** Task Completion Source pattern for booking API (Railway-oriented programming)
- **Bonus B:** Deployment staging slots for zero-downtime deployments
- **Bonus C:** Feature flags (evaluate Bicep vs Config-based approach, document in ADR)
- **Bonus D:** Railway-oriented build patterns (Task/Result) - can complement Bonus A
- **Bonus E:** Add unit and/or integration testing
- **Bonus F:** Shopping cart functionality - allow users to add (one or multiple) tickets before payment
- **Bonus G:** Multi-zone ticket selection - select multiple zones for a single ticket purchase

### Additional Considerations & Suggestions
- **Ticket validation service** - separate service/endpoint for QR code validation with rate limiting
- **Price calculation service** - centralized logic for discount calculations (student/child/pensioner)
- **Region/Zone data model** - define data structure for transportation zones and regions
- **Ticket state machine** - states: Created → Activated → Valid → Expired (with timestamps)
- **Audit logging** - track ticket activations, scans, and state changes for compliance
- **Rate limiting** - protect QR code validation endpoint from abuse
- **Ticket expiration logic** - time-based expiration after activation (e.g., 90 minutes for single-use tickets)
- **User role management** - complete Inspector role implementation and User role creation
- **Error handling** - user-friendly messages for expired/invalid tickets, network issues during scanning
- **Testing strategy** - unit tests for price calculations, integration tests for ticket lifecycle

---

## Overview

During week 5, work focused on completing login functionality for regular users, creating role-based landing pages, and implementing user management for administrators. The goal was to establish a complete authentication and user management system that supports administrators (via Entra ID), inspectors, and regular users (via email/password), while maintaining GDPR compliance and preventing bot registrations.

---

## Completed Activities

### User Login Functionality
- **Email-Only Login:** Changed login form from "Username or Email" to "Email" only for simplicity and GDPR compliance (reduces personal data collection).
- **Forgot Password Feature:** Added checkbox to toggle password reset form. Dummy implementation returns success message (functionality not yet implemented).
- **Registration Feature:** Added checkbox to toggle registration form. Registration is currently blocked to prevent bot registrations - returns message directing users to contact support. User accounts will be managed by administrators.
- **Backend Endpoints:** Created `POST /api/auth/forgot-password` (dummy) and `POST /api/auth/register` (blocked) endpoints. Updated `POST /api/auth/login` to accept `email` parameter instead of `username`.
- **Standard Login Implementation:** Implemented email/password authentication flow. Login form converted from traditional POST to Blazor form handling. `AuthController.Login` now verifies credentials against Cosmos DB users using BCrypt password verification. Creates authentication cookie with proper claims (NameIdentifier, Email, Name, Role) and redirects to appropriate landing page based on role.
- **Error Message Persistence:** Fixed login error messages disappearing immediately. Converted form to use Blazor `@onsubmit` handler instead of traditional form POST, allowing error messages to persist on page until cleared or successful login.
- **UI Styling:** Added CSS for checkboxes and conditional sections with dark theme styling. Sections appear with subtle background and border when toggled.
- **Status:** Complete login functionality operational. Users and inspectors can authenticate via email/password, admins via Entra ID. Error messages persist correctly.

### Role-Based Landing Pages
- **Admin Landing Page:** Updated with sections for ticket management and user management (no checkboxes, always visible). Links to dedicated pages for bookings and users. Logout button aligned to the right.
- **User Landing Page:** Created `/user` page with sections for "My Information" and "My Tickets" (no checkboxes, always visible). Added ticket creation form in "My Tickets" section. Form auto-fills with user's ID and name. Logout button aligned to the right.
- **Inspector Landing Page:** Created `/inspector` page with sections for user viewing and ticket inspection (no checkboxes, always visible). Restricted to Inspector role. Logout button aligned to the right.
- **Navigation:** All landing pages use consistent layout with always-visible sections. Role-based routing via `NavigationHelper` utility. Logout buttons consistently positioned on the right side of user info sections.
- **Status:** All three role-based landing pages implemented with consistent styling and functional ticket creation for users.

### User Management System
- **User Service:** Created `IUserService` and `UserService` with CRUD operations for ticketing system users. Password hashing with BCrypt, email uniqueness validation, Cosmos DB storage with email as partition key.
- **Users API:** Created `UsersController` with endpoints for create, read all, read by ID, update, and delete. All endpoints restricted to Admin role. Password hashes excluded from responses. Audit logging included.
- **User Management UI:** Created `/users` page for admin user management. Create user form with email, password confirmation, name, and role selection (Admin, Inspector, User). Users table with edit and delete functionality. Styled to match existing theme.
- **User Edit Functionality:** Implemented full edit functionality for user management. Edit form appears when "Edit" button is clicked, pre-populated with user data. Email field disabled (cannot be changed). Name validation (min 2 chars, letters/spaces/hyphens only). Optional password update (leave empty to keep current). Role can be changed via dropdown. Cancel button to close edit form. Success/error messages displayed after update.
- **Name Validation:** Added name validation requiring minimum 2 characters and only allowing letters, spaces, and hyphens (no special characters). Real-time validation with error messages.
- **Email Validation:** Implemented strict email validation requiring top-level domain (TLD) to prevent invalid emails like `user@domain`. Validation applied to both user creation and registration forms with real-time error messages.
- **Password Validation:** Added password confirmation field with real-time validation to ensure passwords match before submission. Minimum password length validation (6 characters, no complexity requirements for MVP).
- **Delete Confirmation:** Implemented JavaScript confirmation dialog for user deletion showing user email and warning about irreversible action.
- **Cosmos DB Container:** Added auto-creation logic for `users` container in `ValidateCosmosConnection` method. Container created on app startup if missing, preventing 404 errors on first user creation.
- **UI Improvements:** Removed expandable checkboxes from all landing pages - sections now always visible with direct links. Fixed focus highlight issue on page load by adding CSS and JavaScript to blur focused elements. Logout buttons aligned consistently to the right.
- **Status:** Complete user management system operational. Admins can create, edit, and delete user accounts with proper validation and confirmation dialogs.

### Ticket Management Enhancements
- **Ticket Viewing for Users:** Added `GET /api/bookings/my-bookings` endpoint (User role only) to retrieve user's own tickets. Updated User landing page to display tickets in a table format with booking ID, customer email, customer name, and booking date. Tickets automatically load on page initialization and refresh after creating new tickets. Shows informative message when no tickets exist.
- **Ticket Creation Security:** Fixed ticket creation to prevent users from modifying their name or email. Name field in ticket creation form is now read-only and displays user's actual name from their account. `CreateBooking` endpoint enforces that users can only create tickets for themselves, always using their account data (ID, email, name) regardless of form input. Admin/Inspector roles can still create tickets for any user.
- **Ticket Deletion for Admins:** Added `DELETE /api/bookings/{bookingId}?customerId={customerId}` endpoint restricted to Admin role. Implemented `DeleteBookingAsync` in `BookingService` for Cosmos DB deletion using partition key. Added "Actions" column to bookings table on `/bookings` page (visible only to Admins). Delete button with JavaScript confirmation dialog. Automatically refreshes bookings list after successful deletion. Enables cleanup of errant tickets with incorrect user IDs.
- **Multi-Zone Ticket Selection:** Replaced zone dropdown with checkbox selection allowing users to select multiple zones per ticket. Zones are stored as comma-separated values (e.g., "Zone A, Zone B"). Updated both admin booking form (`/bookings`) and user landing page (`/user`) with checkbox interface. Added real-time price display showing base price calculation (20 SEK × number of zones). Validation ensures at least one zone is selected before ticket creation.
- **Price Calculation Updates:** Updated base price from 25 SEK to 20 SEK per zone. Price calculation now multiplies base price by number of zones before applying discount modifier. Total price formula: `(20 SEK × numberOfZones) × priceModifier`. Updated `PriceCalculationHelper` default parameter and `BookingsController` calculation logic. Booking table displays both total price and base price for transparency.
- **UI Container Width:** Increased container max-width from 900px to 1300px (44% wider) to improve booking table readability. Table now displays all columns (Customer ID, Email, Zone, Price Modifier, Total Price, Booking ID, Date, Actions) without crowding. Better suited for multi-zone ticket information display.
- **Status:** Complete ticket viewing and management functionality. Users can view their own tickets, and admins can delete tickets with proper confirmation. Multi-zone selection and improved pricing calculation fully operational.

---

## Reflection

### What Went Well
- **User Management Foundation:** The CRUD system for users is robust, leveraging existing patterns (Cosmos DB, services, controllers) and incorporating security best practices like password hashing and role-based access.
- **Email Validation:** Implementing strict email validation with TLD requirement prevents invalid email addresses from being stored, improving data quality and user experience.
- **Password Confirmation:** Real-time password matching validation provides immediate feedback to users, reducing errors during account creation.
- **UI/UX Improvements:** Removing unnecessary checkboxes and fixing focus highlights creates a cleaner, more professional user experience.
- **Auto-Container Creation:** Adding container creation logic prevents deployment issues and makes the system more resilient.

### Challenges Encountered
- **Cosmos DB Container Missing:** Initial user creation attempts failed with 404 errors because the `users` container didn't exist. Resolved by adding auto-creation logic in `ValidateCosmosConnection`.
- **Razor Syntax Issues:** Encountered compilation errors when trying to use HTML pattern attributes with square brackets in Razor syntax. Resolved by removing pattern attributes and using C# regex validation instead.
- **Focus Highlight Issue:** Browser was highlighting page titles on load, creating a distracting white box. Resolved with CSS and JavaScript to blur focused elements on page load.
- **Login Error Message Disappearing:** Error messages from login were disappearing immediately due to traditional form POST causing page redirect. Resolved by converting to Blazor form handling with `@onsubmit` to keep error messages visible.
- **Cosmos DB Partition Key Mismatch (Outbox Pattern):** When implementing the Outbox Pattern, Cosmos DB threw "PartitionKey extracted from document doesn't match the one specified in the header" errors. Root cause: `OutboxEventStatus` enum was serialized as integer (0, 1, 2) by default, but partition key was passed as string ("Pending", "Processed", "Failed"). Cosmos DB requires exact match between document field and partition key header. Solution: Created custom `CosmosJsonSerializer` with `JsonStringEnumConverter` to serialize all enums as strings globally. Added `allowIntegerValues: true` for backward compatibility. Moved serializer to Helpers directory. This ensures enum partition keys match between document and header, enabling successful outbox event storage.
- **Azure App Configuration Feature Flag Naming:** Health endpoint showed error "The value ':' is not allowed in the feature name" when checking `BookingEvents:Enabled` flag. Root cause: Azure App Configuration feature flags don't allow colons (`:`) in feature flag names. Solution: Changed feature flag name from `BookingEvents:Enabled` to `BookingEvents_Enabled` (underscore instead of colon) throughout codebase and documentation. Updated `FeatureFlagService`, `HealthController`, and all ADR documentation. Feature flag now works correctly and displays in health endpoint.

### Lessons Learned
- **Container Management:** Cosmos DB containers should be created either via infrastructure (Bicep) or auto-created on first use. Auto-creation provides better developer experience but should be documented.
- **Email Validation:** HTML5 email input type alone is not sufficient - custom validation with regex is needed to enforce TLD requirements and prevent invalid formats.
- **Blazor Bind Events:** When using `@bind` with validation, use `@bind:event` and `@bind:after` instead of `@onchange` to avoid conflicts.
- **Form Handling:** Traditional HTML form POST causes page redirects which clear error messages. Blazor form handling with `@onsubmit` and `@onsubmit:preventDefault` allows error messages to persist on the page.
- **User Experience:** Small UI details like focus highlights, error message persistence, and confirmation dialogs significantly impact perceived quality and professionalism of the application.
- **Security in API Endpoints:** When allowing multiple roles to access an endpoint, add role-specific checks within the endpoint to ensure users can only perform actions on their own data (e.g., users can only create tickets for themselves).
- **Cosmos DB Enum Serialization:** When using enums as partition keys in Cosmos DB, they must be serialized as strings to match the partition key header. The default `CosmosSerializationOptions` doesn't respect `[JsonConverter]` attributes. A custom `CosmosSerializer` with `JsonStringEnumConverter` is required. Always use `allowIntegerValues: true` for backward compatibility when reading existing data. The serializer must be applied globally to the `CosmosClient`, affecting all containers, so ensure it doesn't break existing data models.
- **Enum vs String for Simple Status Values:** For simple status values used as partition keys (like `OutboxEventStatus`), consider whether enums are necessary. Enums provide type safety, IntelliSense, and compile-time error checking, but require custom serialization for Cosmos DB. Strings with constants (`public const string Pending = "Pending"`) would be simpler and avoid serialization complexity, but lose compile-time safety. For this use case (simple status partition key), strings might have been the simpler choice. However, enums add value through type safety and prevent typos. The trade-off: simpler code (strings) vs. type safety (enums). Consider the use case complexity and team preferences when choosing. For complex state machines or many valid values, enums are worth the serialization overhead. For simple partition keys with 2-3 values, strings may be sufficient.
- **Azure App Configuration Feature Flag Naming Restrictions:** Azure App Configuration feature flags have strict naming rules - colons (`:`) are not allowed in feature flag names. Use underscores (`_`) or hyphens (`-`) instead. When designing feature flag names, check Azure documentation for allowed characters. Common patterns: `FeatureName_Enabled`, `Feature-Name-Enabled`, or `FeatureNameEnabled`. The error message "The value ':' is not allowed in the feature name" is clear, but it's better to follow naming conventions from the start to avoid runtime errors.
- **Azure App Configuration Hot-Reload Implementation:** The `IConfigurationRefresherProvider` may not be automatically registered in the service container when using `AddAzureAppConfiguration`. Instead of relying on service locator, store the refresher directly during configuration using `options.GetRefresher()` and access it via a static variable. This ensures the refresher is available to middleware for hot-reload functionality. The refresh interval can be reduced to 30 seconds for faster testing, but 1 minute is recommended for production to reduce API calls. Always update the sentinel key after changing feature flags to trigger the refresh mechanism.

### Key Achievements
- **Complete User Management:** Full CRUD system for users with proper validation, password hashing, and role-based access control. Edit functionality fully implemented.
- **Standard Login Implementation:** Email/password authentication working for users and inspectors. BCrypt password verification, proper cookie-based session management, role-based redirects.
- **Ticket Creation for Users:** Users can create tickets from their landing page. Form auto-fills with user information. Security check ensures users can only create tickets for themselves. Name field is read-only to prevent tampering.
- **Ticket Viewing for Users:** Users can now view all their existing tickets in a table format on their landing page. Tickets automatically refresh after creation.
- **Ticket Deletion for Admins:** Admins can delete tickets with confirmation dialogs. Enables cleanup of errant tickets and testing of deletion functionality.
- **Multi-Zone Ticket Selection:** Implemented checkbox-based zone selection allowing multiple zones per ticket. Real-time price calculation and validation. Zones stored as comma-separated values for flexibility.
- **Price Calculation System:** Updated pricing model to 20 SEK per zone with automatic calculation based on user attributes (age, student status). Price modifier system (0.0 for children, 0.5 for students/pensioners, 1.0 for standard) applied after base price calculation.
- **Email Validation:** Strict email format validation prevents invalid data entry across all user creation and registration forms.
- **Password Security:** Password confirmation and length validation ensure users create secure accounts without typos.
- **Improved UX:** Fixed focus issues, login error message persistence, and streamlined all landing pages for better user experience. Consistent logout button positioning. Wider container for better table readability.
- **Resilient Infrastructure:** Auto-creation of Cosmos DB containers prevents deployment failures and improves system reliability.

### What Could Be Improved
- **Password Strength Requirements:** Currently only validates minimum length. Could add complexity requirements (uppercase, numbers, special characters) for production.
- **Email Uniqueness Feedback:** Could provide more immediate feedback when checking if an email already exists (e.g., on blur event).
- **Container Creation in Bicep:** Consider adding `users` container to Cosmos DB Bicep template for infrastructure-as-code approach instead of runtime creation.
- **Ticket Search Functionality:** Add search and filtering capabilities to the admin booking management page for better ticket discovery.
- **Price Configuration:** Base price (20 SEK) is currently hardcoded. Could be moved to configuration (appsettings.json or Azure App Configuration) for easier adjustment without code deployment.
- **Zone Data Model:** Zones are currently stored as comma-separated strings. Consider creating a proper zone data model or array structure for better type safety and validation.

---

## Ongoing Work

- **Event-Driven Architecture Infrastructure:** Phase 1 complete - App Configuration, Service Bus, Function App infrastructure deployed, and all NuGet packages added.
- **Event-Driven Architecture Contracts:** Phase 2 complete - Event contracts, Outbox model, and Cosmos DB container configured.
- **Outbox Pattern Implementation:** Phase 3 complete - Outbox service implemented, integrated into booking creation, custom Cosmos serializer created for enum string serialization. Events successfully stored in outbox container with "Pending" status.
- **Feature Flag Integration:** Phase 4 complete - Azure App Configuration integrated with hot-reload via sentinel key pattern, feature flag service created, booking flow integrated with dual-system architecture support, and comprehensive documentation added to ADR-006.
- **Service Bus Integration:** Phase 5 complete - Created `IEventPublisher` interface and `ServiceBusEventPublisher` implementation with managed identity authentication. Implemented `OutboxProcessorService` background service that polls outbox every 30 seconds and publishes events to Service Bus when feature flag is enabled. All services registered in dependency injection. Health endpoint enhanced to display Service Bus status. Event-driven publishing fully operational.
- **Azure Function Implementation:** Phase 6 complete - Created Functions project structure, implemented `OnBookingCreated` function with Service Bus trigger, added comprehensive error handling with retry policies and dead letter queue support, and configured deployment pipeline via GitHub Actions. Function App deploys automatically after successful CI builds using zip deploy. Event-driven architecture fully operational end-to-end.

---

## Event-Driven Architecture Infrastructure (Week 5)

**Detailed roadmap:** See [Event-Driven Architecture Roadmap](eventdriven_roadmap.md) for complete implementation details and step-by-step progress.

### Phase 1: Infrastructure & Foundation (Complete)
Infrastructure setup for event-driven architecture. Created Azure App Configuration, Service Bus namespace with `booking-events` queue, and Azure Function App via Bicep. Added all required NuGet packages to API and Functions projects (Service Bus, App Configuration, Feature Management, Azure Functions Worker). All resources configured with managed identity and RBAC roles. Infrastructure verified and operational.

### Phase 2: Event Contracts & Data Models (Complete)
Created event contracts and data models for the event-driven system. Implemented base `Event` class and `BookingCreated`/`BookingCancelled` event classes in `Ticketing.Contracts.Events`. Created `OutboxEvent` model with `OutboxEventStatus` enum for the Outbox Pattern. Added outbox container to Cosmos DB Bicep template with partition key `/status`. All contracts ready for Outbox Pattern implementation.

### Phase 3: Outbox Pattern Implementation (Complete)
Implemented the Transactional Outbox Pattern to ensure reliable event publishing. Created `IOutboxService` and `OutboxService` with methods to add events, retrieve pending events, and mark events as processed. Integrated outbox event creation into `BookingsController.CreateBooking` - after successful booking creation, a `BookingCreated` event is automatically added to the outbox. Created custom `CosmosJsonSerializer` in Helpers directory to handle enum serialization as strings (required for partition key matching). Events are successfully stored in the outbox container with `status: "Pending"` and can be queried.

### Phase 4: Feature Flag Integration (Complete)
Implemented permanent dual-system coexistence using Azure App Configuration feature flags. Integrated App Configuration with hot-reload support via sentinel key pattern (`Settings:Sentinel`) - configuration refreshes automatically within 1 minute without service restart. Created `IFeatureFlagService` and `FeatureFlagService` to check `BookingEvents_Enabled` flag. Integrated feature flag check into booking flow - when disabled (default), system operates in synchronous mode (chained API calls); when enabled, system operates in event-driven mode (Service Bus publishing). Both paths coexist permanently, allowing runtime switching without code changes. Outbox always writes events regardless of flag state for audit purposes. Enhanced health endpoint to display App Configuration status and feature flag values. Comprehensive feature flag usage documentation added to ADR-006, including operational instructions, use cases, and demonstration guide. Dual-system architecture designed and documented - ready for Phase 5 (Service Bus Integration).

**Hot-Reload Issue & Fix:** Initially, hot-reload was not working - feature flag changes required service restart to take effect. 

**First Issue:** Missing refresh middleware in the HTTP pipeline. Added custom middleware to `WebApplicationExtensions.cs` that calls `TryRefreshAsync()` on each HTTP request.

**Second Issue:** `IConfigurationRefresherProvider` was not found in the service container, preventing refresh from working. Root cause: The refresher provider wasn't being registered automatically by `AddAzureAppConfiguration`, or wasn't accessible via service locator.

**Solution:** Store the refresher directly during configuration using `options.GetRefresher()` and access it via a static variable in the middleware. This bypasses the service container registration issue. Modified `ConfigurationExtensions.cs` to store the refresher in a static variable when configuring App Configuration, and updated middleware to access it via `ConfigurationExtensions.GetConfigurationRefresher()`.

**Result:** Hot-reload now works correctly - updating sentinel key triggers automatic refresh within 30 seconds (reduced from 1 minute for faster testing) without restart. Feature flags can be toggled at runtime for live demonstrations. Documented in ADR-014 with correct implementation details.

### Phase 5: Service Bus Integration (Complete)
Completed full Service Bus integration for event publishing. Created `IEventPublisher` interface and `ServiceBusEventPublisher` implementation using managed identity authentication (`DefaultAzureCredential`) with fully qualified namespace. Service Bus client retrieves namespace from Key Vault or appsettings.json. Implemented `OutboxProcessorService` as background hosted service that polls outbox every 30 seconds for pending events. Service checks feature flag before processing - only publishes to Service Bus when `BookingEvents_Enabled = true`. Deserializes event data from JSON and publishes to `booking-events` queue. Marks events as processed after successful publish. Comprehensive error handling with logging and retry logic. Enhanced health endpoint to display Service Bus client registration, event publisher status, and operational status with queue name. All services registered in dependency injection. Event-driven publishing fully operational - when feature flag is enabled, pending outbox events are automatically published to Service Bus for consumption by Azure Functions.

### Phase 6: Azure Function Implementation (Complete)
Completed Azure Functions implementation for event consumption. Created Functions project structure with all required NuGet packages (Service Bus extensions, Application Insights). Implemented `OnBookingCreatedFunction` with Service Bus trigger binding to `booking-events` queue using managed identity authentication. Function deserializes `BookingCreated` events from JSON with camelCase naming policy (matches publisher). Comprehensive error handling with specific exception types (`ArgumentException`, `InvalidOperationException`, `JsonException`) and message metadata tracking (deliveryCount, enqueuedTimeUtc, messageId). Configured retry policy in `host.json` with exponential backoff (3 retries, 5 seconds to 5 minutes). Dead letter queue support via Service Bus queue configuration (maxDeliveryCount: 10). Application Insights integration configured for automatic telemetry. Created GitHub Actions deployment pipeline (`.github/workflows/cd-functions-dev.yaml`) using zip deploy via Azure CLI. Function App deploys automatically after successful CI builds. Event-driven architecture fully operational end-to-end: bookings → outbox → Service Bus → Azure Functions → Application Insights.

### Phase 7: Testing & Validation (In Progress)
**Phase 7.1 (Complete):** Tested synchronous flow with feature flag disabled. Verified bookings work exactly as before, outbox events created but not published to Service Bus, no performance impact, and system operates as "before refactoring" state. All tests passed.

**Phase 7.2 (Complete):** Tested event-driven flow with feature flag enabled. Verified hot-reload works correctly (feature flag updates without restart), outbox events are processed by `OutboxProcessorService` and published to Service Bus, Function App receives and processes events, and complete end-to-end event flow operational. Event-driven architecture fully validated and operational.

**Phase 7.3 (Complete):** Tested switching between modes at runtime. Verified hot-reload works in both directions (enabled → disabled → enabled) within 30 seconds without service restart. Created bookings in both modes - event-driven mode processes events and publishes to Service Bus, synchronous mode creates outbox events but doesn't process them. When feature flag is re-enabled, pending events from synchronous mode are automatically processed (backlog processing). All bookings created successfully regardless of mode. No data loss or corruption during mode switches. Zero-downtime switching confirmed - perfect for live demonstrations.

---

## Next Steps

1. **Event-Driven Architecture:** Continue with Phase 7 (Testing & Validation) - Phase 7.1 and 7.2 complete. Remaining: Test switching between modes (7.3), test error scenarios (7.4), and performance testing (7.5). Then proceed to Phase 8 (Monitoring & Observability) for Application Insights dashboards and alerts.
2. **Ticket Activation:** Implement ticket activation timer with dual triggers (manual and QR code scan).
3. **QR Code Generation:** Generate QR codes for tickets to enable scanning functionality.
4. **Ticket Search Functionality:** Add search and filtering capabilities to the admin booking management page.
5. **Shopping Cart (Bonus F):** Implement shopping cart functionality to allow users to add multiple tickets before payment.
6. **Price Configuration:** Move base price to configuration for runtime adjustment without code deployment.

---
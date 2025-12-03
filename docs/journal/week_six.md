# Week 6 – Event-Driven Architecture Refinement & Demo Enhancements

## Brainstorming & Planned Progression

### Core Features (Priority Order)
1. **Ticket activation timer** - dual triggers:
   - Manual start time selection (user landing page interface under "active tickets")
   - Secondary trigger on "activation" (QR code scan when boarding transportation)
2. **Event triggers API functionality** - implement event-driven ticket validation **completed**
3. **QR code generation** for ticket scanning

### Bonus Features
- **Bonus A:** Task Completion Source pattern for booking API (Railway-oriented programming)
- **Bonus B:** Deployment staging slots for zero-downtime deployments
- **Bonus C:** Feature flags (evaluate Bicep vs Config-based approach, document in ADR) **completed**
- **Bonus D:** Railway-oriented build patterns (Task/Result) - can complement Bonus A
- **Bonus E:** Add unit and/or integration testing **in progress** (unit tests for price calculations **completed**)
- **Bonus F:** Shopping cart functionality - allow users to add (one or multiple) tickets before payment
- **Bonus G:** Multi-zone ticket selection - select multiple zones for a single ticket purchase **completed**

### Additional Considerations & Suggestions
- **Ticket validation service** - separate service/endpoint for QR code validation with rate limiting
- **Region/Zone data model** - define data structure for transportation zones and regions
- **Ticket state machine** - states: Created → Activated → Valid → Expired (with timestamps)
- **Audit logging** - track ticket activations, scans, and state changes for compliance
- **Rate limiting** - protect QR code validation endpoint from abuse
- **Ticket expiration logic** - time-based expiration after activation (e.g., 90 minutes for single-use tickets)
- **Error handling** - user-friendly messages for expired/invalid tickets, network issues during scanning
- **Testing strategy** - unit tests for price calculations, integration tests for ticket lifecycle

---

## Overview

During week 6, work focused on simplifying the demo page and removing over-engineered features. The goal was to create a clean, maintainable MVP that focuses on core functionality: event-driven architecture status and booking management.

### Scope Clarification - Users & Inspectors

**Users & Inspectors are COMPLETE for demo purposes:**
- **User Role Management:** Complete implementation of Admin, Inspector, and User roles
- **User Functionality:** Users can book tickets today - booking system is fully operational
- **Inspector Role:** Inspector role implemented with ability to view bookings and manage users
- **Out of Scope:** User registration flow (users are created by Admin only for demo purposes)
- **Out of Scope:** QR code scanning system for Inspectors (no physical QR readers available for demo)

**Rationale:** The core functionality for Users and Inspectors is complete and sufficient for demonstration. Registration and scanning systems would add complexity without proportional value for the MVP demo. These features can be added in future iterations if needed.

---

## Completed Activities

### Demo Page Simplification
- **Removed Application Insights Integration:** Removed the Application Insights query section from the demo page to reduce complexity and improve maintainability.
  - **Rationale:**
    - **A) Code Complexity:** The Application Insights integration added over 800 lines of code, making the MVP unsustainable and difficult to maintain. The demo page grew from ~280 lines to over 1,100 lines with complex KQL query handling, table rendering, modal popups, and data transformation logic.
    - **B) Visualization Limitations:** Application Insights queries return JSON strings that are difficult to visualize effectively in a web page. Displaying query results as tables feels more like reading logs than a proper demo visualization, making it unsuitable for live demonstrations.
    - **Better Alternative:** The Azure Portal's Application Insights interface provides superior graphical visualization with charts, graphs, and interactive dashboards that are much more suitable for live demos. Using the portal side-by-side with the demo page provides a better demonstration experience.
- **Simplified Page Structure:** 
  - **Event-Driven Architecture Status** section at the top (full width) - shows current mode and allows toggling
  - **Booking Management** section below (full width) - create, view, and manage bookings
  - Removed all Application Insights query code, KQL handling, table rendering, and modal popups
  - File size reduced from 1,130 lines to ~280 lines (75% reduction)
- **Maintained Core Functionality:**
  - Feature flag toggle with propagation polling
  - Health status display
  - Booking creation and management
  - Clean, focused user interface

**Status:** Demo page simplified and streamlined. Core functionality maintained while removing over-engineered features. Page is now maintainable, focused, and suitable for MVP demonstrations. Application Insights can be viewed in Azure Portal for better visualization during demos.

### Application Insights Workbook Setup

- **Created Custom Workbook:** Set up an Application Insights Workbook in Azure Portal to visualize the dual-system architecture (Synchronous vs Event-Driven modes) during live demonstrations.
  - **Workbook Purpose:** Visualize two distinct processing paths:
    - **Synchronous mode:** Direct API execution
    - **Event-Driven mode:** Outbox → Service Bus → Azure Function pipeline
  - **Workbook Sections:**
    1. **Current Mode Indicator** - Shows active architecture mode (Synchronous/Event-Driven) using latest `ModeSwitch` or `FeatureFlagToggled` event with 24h time window
    2. **Latest Booking Flow Timeline** - Time chart showing chronological flow of booking-related events (1h time window), visually distinguishing between Synchronous path (minimal events) and Event-Driven path (full pipeline)
    3. **Events by Type (with Mode Encoding)** - Bar chart showing event counts grouped by event type and mode, with mode encoded in category name (e.g., "BookingCreated (Synchronous)" vs "BookingCreated (Event-Driven)")
  - **Workbook Configuration:**
    - Time range: Last 15 minutes (adjustable per query)
    - Auto-refresh: 1 minute (accounts for 2-5 minute ingestion delay)
    - All visuals consume `customEvents` from the same Application Insights resource
  - **Telemetry Events Used:**
    - `ModeSwitch`, `FeatureFlagToggled` (for mode detection)
    - `BookingCreated`, `OutboxEventCreated`, `OutboxEventProcessed`, `ServiceBusEventPublished`, `FunctionBookingCreatedProcessed` (for flow visualization)
  - **Custom Dimensions:**
    - `ToMode` - Architecture mode ("Synchronous" or "Event-Driven")
    - `SystemType` - System type for filtering and categorization
  - **Demo Strategy:** Workbook displayed side-by-side with demo page during presentations, providing superior graphical visualization compared to JSON query results in web tables.

**Status:** Application Insights Workbook configured and ready for live demonstrations. Provides clear visual distinction between Synchronous and Event-Driven architectures with near real-time updates.

### Price Configuration Implementation

- **Moved Base Price to Configuration:** Implemented configurable base price per zone to enable runtime adjustment without code deployment.
  - **Configuration Added:** Added `Pricing:BasePricePerZone` to `appsettings.json` (default: 20.0 SEK)
  - **Code Updates:**
    - Updated `BookingsController` to inject `IConfiguration` and read base price from configuration
    - Updated `PriceCalculationHelper.CalculateTotalPrice()` to accept base price as parameter (removed hardcoded default)
    - Base price now configurable via `appsettings.json` with fallback to default value
  - **Benefits:**
    - Runtime price adjustment without code deployment
    - Foundation for future Azure App Configuration integration (hot-reload capability)
    - Addresses ADR-011 recommendation to move base price from hardcoded values
  - **Documentation:** Updated ADR-011 to reflect that base price is now configurable

**Status:** Price configuration implemented and tested. Base price can now be adjusted via configuration without code changes.

### Unit Testing Implementation

- **Test Project Setup:** Created dedicated test project (`Ticketing.Web.Tests`) for unit and integration tests.
  - **Framework:** xUnit for test framework
  - **Mocking:** NSubstitute for mocking dependencies
  - **Project Structure:** Located in `src/tests/Ticketing.Web.Tests/` following standard .NET test project conventions
  - **Integration:** Added to solution file and configured for CI/CD pipeline
- **Price Calculation Unit Tests:** Implemented comprehensive unit tests for price calculation logic.
  - **Test Coverage:**
    - `CalculatePriceModifier()`: 12 test cases covering all age groups, student status, edge cases, and null handling
    - `CalculateTotalPrice()`: 10 test cases covering zero zones, single/multiple zones, different modifiers, and custom base prices
  - **Test Results:** All 22 tests passing
  - **Test Organization:** Tests organized in `Helpers/PriceCalculationHelperTests.cs` following project structure

**Status:** Test project established with comprehensive unit tests for price calculations. Foundation laid for future integration tests.

### Integration Testing Implementation

- **Integration Test Infrastructure:** Set up comprehensive integration test infrastructure using `WebApplicationFactory` to test the full application stack.
  - **Test Framework:** xUnit with `Microsoft.AspNetCore.Mvc.Testing` for integration testing
  - **Test Authentication:** Custom test authentication handler that bypasses Azure AD and provides Admin role for testing
  - **Database Configuration:** Cosmos DB configured for testing (uses emulator or test connection string via `TEST_COSMOS_CONNECTION_STRING` environment variable)
  - **Service Isolation:** External services disabled during tests (Application Insights telemetry, Service Bus client, OutboxProcessor background service)
  - **Test Fixture:** `WebApplicationFactoryFixture` class provides configured test client with authentication
- **Booking Lifecycle Integration Tests:** Implemented 7 comprehensive integration tests covering the full booking lifecycle:
  1. **CreateBooking_WithValidData_ReturnsCreatedBooking** - Verifies booking creation with valid data and proper HTTP status (201 Created)
  2. **CreateBooking_CalculatesPriceCorrectly** - Verifies price calculation (base price × zones × modifier) is applied correctly
  3. **GetBookingById_WithValidId_ReturnsBooking** - Verifies retrieving bookings by customer ID with proper filtering
  4. **ActivateBooking_WithValidBooking_UpdatesStatus** - Verifies ticket activation updates status, sets validity period (90 minutes), and calculates timestamps correctly
  5. **ActivateBooking_AlreadyActivated_ReturnsBadRequest** - Verifies duplicate activation is prevented with appropriate error response
  6. **DeleteBooking_WithValidId_RemovesBooking** - Verifies booking deletion and removal from database
  7. **GetBookingsByCustomer_ReturnsOnlyCustomerBookings** - Verifies customer-specific booking filtering works correctly
- **Test Results:** All integration tests passing successfully
- **Test Documentation:** Created `Integration/README.md` with setup instructions, prerequisites (Cosmos DB Emulator), and CI/CD considerations
- **Helper Classes:** Created `ProgramReference.cs` to make `Program` class accessible for `WebApplicationFactory` testing

**Status:** Integration test suite complete and passing. Full booking lifecycle tested end-to-end with proper authentication, database interaction, and API validation. Tests ready for CI/CD pipeline integration.

### Ticket Activation Timer Implementation (Steps 1-3 Complete)

- **Step 1: Model Updates & Ticket Status Constants:**
  - **Booking Model Extensions:** Added activation and validity fields to `Booking` model:
    - `ActivatedAt` (DateTime?, nullable) - When ticket was activated
    - `ValidFrom` (DateTime?, nullable) - Start of validity period
    - `ValidTo` (DateTime?, nullable) - End of validity period
    - `Status` (string) - Ticket status using string constants (changed from enum to avoid serialization issues)
  - **TicketStatus Constants:** Created `TicketStatus` static class with string constants:
    - `Created` - Initial state when ticket is booked
    - `Activated` - Ticket has been activated by user
    - `Valid` - Ticket is currently valid (within validity period)
    - `Expired` - Ticket has expired
  - **Validation Helpers:** Added `TicketIsValid()` and `GetAllTicketStatuses()` methods for runtime validation
  - **Design Decision:** Used string constants instead of enum to avoid past Cosmos DB serialization issues and maintain flexibility
- **Step 2: Activation API Endpoint:**
  - **New Endpoint:** `POST /api/bookings/{bookingId}/activate`
  - **Functionality:**
    - Validates booking exists and belongs to user (or Admin/Inspector can activate any ticket)
    - Validates ticket status is "Created" (only unactivated tickets can be activated)
    - Sets activation time (from request or current time)
    - Calculates validity period (default 90 minutes from activation)
    - Updates booking status to "Activated"
    - Updates booking in Cosmos DB
  - **Authorization:** Supports User (own tickets), Admin, and Inspector roles
  - **Validation:** Comprehensive validation using `TicketActivationHelper` helper class
- **Step 3: Activation UI Integration:**
  - **UserLandingPage Integration:** Added activation functionality to user landing page
  - **BookingTable Component:** Created reusable `BookingTable` component to eliminate code duplication:
    - Displays booking status with color-coded badges (Created, Activated, Valid, Expired)
    - Shows validity period (activation time, valid until, time remaining/expired)
    - Provides "Activate" button for Created tickets
    - Supports role-based display (customer info, delete buttons, activation)
  - **Component Integration:** Integrated `BookingTable` into:
    - `UserLandingPage.razor` - User's own tickets with activation
    - `BookingManagement.razor` - Demo page booking management
    - `Bookings.razor` - Admin/Inspector booking management
  - **UI Enhancements:** Added CSS styles for status badges and improved button styling
- **Helper Classes:** Created `TicketActivationHelper` static class with:
  - `CalculateValidityPeriod()` - Calculates validity period from activation time
  - `CanActivate()` - Checks if booking can be activated
  - `ValidateActivation()` - Comprehensive validation with error messages

**Status:** Ticket activation functionality implemented (Steps 1-3). Users can now activate tickets manually, and the system tracks ticket status and validity periods. Step 4 (event-driven expiration) remains for future implementation.

### QR Code Implementation ✓ COMPLETE

- **QR Code Generation at Activation:** Implemented QR code generation during ticket activation for instant value and demo readiness.
  - **Library:** QRCoder NuGet package (v1.7.0) for QR code generation
  - **Generation Trigger:** QR code generated automatically when ticket is activated via `POST /api/bookings/{bookingId}/activate`
  - **Data Encoding:** JSON-encoded data containing booking ID, customer ID, activation timestamp, validity period (ValidFrom, ValidTo), status, and version
  - **Storage:** Base64-encoded PNG image stored in `Booking.QrCodeData` field in Cosmos DB for fast retrieval
  - **UI Integration:** 
    - "Show QR Code" button appears for activated tickets in `BookingTable` component
    - Modal popup displays QR code with booking details (Booking ID, Zone, Valid until)
    - QR code displayed as base64 data URL for instant rendering
  - **Admin Support:** Admins and Inspectors can activate any ticket to generate QR codes (not restricted to own tickets)
  - **Helper Classes:** Created `QrCodeHelper.cs` with `GenerateQrCode()` and `GetQrCodeDataUrl()` methods
- **Benefits:**
  - Instant display: QR code stored in Cosmos DB enables fast retrieval without regeneration
  - Activation-time generation: Only activated tickets have QR codes, reducing unnecessary storage
  - Validity embedded: QR code includes validity period information for validation
  - Demo-ready: Visual feedback for activated tickets enhances demonstration experience
- **Documentation:** Created ADR-020 documenting QR code implementation strategy and decisions

**Status:** QR code generation complete and operational. QR codes are generated automatically on ticket activation and can be displayed via modal popup. All activated tickets now have QR codes stored in Cosmos DB for fast retrieval.

### Code Refactoring & Bug Fixes

- **BookingTable Component Refactoring:**
  - **Problem:** Booking display logic was duplicated across three pages (`UserLandingPage.razor`, `BookingManagement.razor`, `Bookings.razor`)
  - **Solution:** Created reusable `BookingTable` component to centralize booking display logic
  - **Benefits:**
    - Eliminated code duplication (~150 lines of repeated code)
    - Consistent UI and functionality across all pages
    - Easier maintenance and updates
    - Centralized status badge and validity period display logic
  - **Component Features:**
    - Configurable display options (customer info, delete buttons, activation)
    - Role-based action visibility
    - Status badges with color coding
    - Validity period display with time remaining calculations
- **Booking Loading Bug Fix:**
  - **Problem:** Admin/Inspector pages were attempting to load bookings on initialization, causing error messages
  - **Solution:**
    - Changed `bookings` initialization from `null` to empty list in `Bookings.razor` and `BookingManagement.razor`
    - Removed automatic "No bookings found" message from `BookingTable` component
    - Fixed Razor syntax errors in `Bookings.razor` (improper `else if` statements)
  - **Result:**
    - User page: Still loads bookings automatically on login (as intended)
    - Admin/Inspector pages: Show nothing until "Get Bookings" is clicked
    - Demo page: Shows nothing until "Get Bookings" is clicked
    - No more error messages on page load for Admin/Inspector roles

**Status:** Code refactoring complete. Booking display logic centralized, and booking loading bug fixed. All pages now behave correctly based on user role.

---

## Reflection

### What Went Well
- **Simplification:** Removing the Application Insights integration immediately reduced code complexity and made the page much more maintainable. The file size reduction from 1,130 lines to 280 lines (75% reduction) makes the codebase more sustainable for an MVP.
- **Focus on Core Functionality:** The simplified page now focuses on the two essential features: event-driven architecture status and booking management. This makes the demo clearer and easier to follow.
- **Better Demo Strategy:** Using Azure Portal's Application Insights interface for visualization provides a superior demonstration experience with proper charts and graphs, rather than trying to display JSON query results in a web table.
- **Configuration Over Hardcoding:** Moving base price to configuration was a quick win that adds flexibility without complexity. Sets foundation for future Azure App Configuration integration.
- **Component Reusability:** Creating `BookingTable` component eliminated ~150 lines of duplicated code across three pages. Centralized logic makes future updates much easier.
- **Incremental Feature Development:** Breaking ticket activation into small steps (model → API → UI) made the feature manageable and testable at each stage.
- **Integration Test Coverage:** Setting up integration tests with `WebApplicationFactory` provides confidence in the full application stack. Testing authentication, database interaction, and API endpoints end-to-end catches issues that unit tests alone would miss.

### Challenges Encountered
- **Over-Engineering:** The Application Insights integration grew too complex, adding over 800 lines of code for KQL query handling, table rendering, modal popups, and data transformation. This made the codebase difficult to maintain and unsustainable for an MVP.
- **Visualization Limitations:** Application Insights queries return JSON strings that are difficult to visualize effectively. Displaying query results as tables feels like reading logs rather than a proper demo visualization, making it unsuitable for live demonstrations.
- **KQL Query Complexity:** The KQL queries required complex joins and data transformation, making them error-prone and difficult to debug. Syntax errors in KQL queries were hard to troubleshoot.
- **String Constants vs Enums:** Chose string constants over enums for `TicketStatus` to avoid past Cosmos DB serialization issues. Trade-off: less type safety but more flexibility and fewer serialization headaches.
- **Role-Based Loading Logic:** Admin/Inspector pages attempting to auto-load bookings caused confusion. Simple fix: initialize with empty list instead of null, only load on explicit user action.

### Lessons Learned
- **MVP Philosophy:** For an MVP, it's important to focus on core functionality and avoid over-engineering. Features that add significant complexity without proportional value should be reconsidered or removed.
- **Right Tool for the Job:** Some features are better suited for specialized tools (like Azure Portal for Application Insights visualization) rather than trying to replicate them in a web application. Using the right tool provides a better user experience.
- **Code Maintainability:** Code complexity should be carefully managed. A feature that adds 800+ lines of code needs to provide significant value to justify its inclusion in an MVP.
- **Demo Strategy:** For live demonstrations, it's often better to use multiple tools side-by-side (e.g., demo page + Azure Portal) rather than trying to integrate everything into a single page. This provides better visualization and a clearer demonstration flow.
- **Simplicity Over Features:** A simple, focused page is often more effective than a complex page with many features. Removing features can improve the overall user experience.
- **Test Early, Test Often:** Setting up the test project early and writing unit tests for price calculations caught edge cases and provided confidence in refactoring. Small test suite is better than no tests. In this case we added the tests way too late, had to focus on core functionality to know the project was going to be near the end before tests were added.
- **Component Extraction:** Identifying duplicated code early and extracting it into reusable components pays off quickly. The `BookingTable` component saved significant maintenance effort.
- **Configuration as Code:** Moving hardcoded values to configuration is a small change with big benefits. Enables runtime adjustments and sets foundation for feature flags and hot-reload capabilities.
- **Integration Test Infrastructure:** Setting up `WebApplicationFactory` with proper test authentication and service isolation is crucial for reliable integration tests. Disabling external services (Application Insights, Service Bus) during tests prevents test failures due to external dependencies and makes tests faster and more reliable.

### Key Achievements
- **Simplified Codebase:** Reduced demo page from 1,130 lines to 280 lines (75% reduction), making it much more maintainable and sustainable for an MVP.
- **Focused Functionality:** Page now focuses on core features: event-driven architecture status and booking management, making the demo clearer and easier to follow.
- **Better Demo Strategy:** Established a better approach for demonstrations using Azure Portal's Application Insights interface for visualization, providing superior charts and graphs.
- **Maintainable MVP:** Created a clean, focused demo page that is sustainable and easy to maintain.
- **Test Foundation:** Established test project with passing unit tests for price calculations and comprehensive integration tests for booking lifecycle. Full test coverage for core functionality.
- **Ticket Activation:** Implemented manual ticket activation with status tracking and validity periods. Users can now activate tickets and see real-time status updates.
- **QR Code Generation:** Implemented QR code generation at activation time with Cosmos DB storage. QR codes include validity period information and can be displayed via modal popup for activated tickets.
- **Code Quality:** Eliminated code duplication through component extraction and fixed role-based loading bugs. Improved maintainability across booking-related pages.
- **Test Coverage:** Comprehensive test suite with 22 unit tests and 7 integration tests covering price calculations and full booking lifecycle. All tests passing and ready for CI/CD integration.

### What Could Be Improved
- **Future Enhancements:** If Application Insights integration is needed in the future, consider using Azure Portal dashboards or Power BI for visualization rather than building custom table rendering in the web application.
- **Documentation:** Document the decision to use Azure Portal for Application Insights visualization in architecture documentation for future reference.

---

## Ongoing Work

### Event-Driven Architecture Infrastructure
- **Phase 1-7:** Complete - All infrastructure, contracts, outbox pattern, feature flags, Service Bus, Azure Functions, and testing completed.
- **Phase 8:** Monitoring & Observability - Phase 8.1 (Application Insights custom events) and Phase 8.2 (Demo page) complete. Remaining: Phase 8.3 (Alerts setup) and Phase 9 (Documentation & Cleanup).

---

## Event-Driven Architecture Progress (Week 6)

**Detailed roadmap:** See [Event-Driven Architecture Roadmap](eventdriven_roadmap.md) for complete implementation details and step-by-step progress.

### Phase 8: Monitoring & Observability (In Progress - Phase 8.1 & 8.2 Complete)

**Phase 8.2 Enhancements (Week 6):** 
- **Demo Page Simplification:** Removed Application Insights integration from demo page. The integration added too much complexity (800+ lines of code) and the JSON query results were difficult to visualize effectively. Decided to use Azure Portal's Application Insights interface for visualization during demos, which provides superior charts and graphs. Demo page now focuses on core functionality: event-driven architecture status and booking management.
- **Application Insights Workbook:** Created custom Application Insights Workbook in Azure Portal with three key sections:
  1. **Current Mode Indicator** - Shows active architecture mode using latest mode switch events
  2. **Latest Booking Flow Timeline** - Time chart showing chronological event flow, distinguishing Synchronous vs Event-Driven paths
  3. **Events by Type** - Bar chart with mode-encoded categories showing event frequency comparison
  - Workbook configured with 1-minute auto-refresh and appropriate time windows for each section
  - Provides superior visualization for live demonstrations compared to web-based query results

**Remaining Work:**
- **Phase 8.3:** Set up Application Insights alerts (dead letter queue messages, function failures, outbox processing delays)
- **Phase 9:** Documentation & Cleanup (update ADR-006, architecture.md, create developer guide, create comparison documentation, update week journal)

---

## Next Steps

1. **Event-Driven Architecture:** Complete Phase 8.3 (Application Insights alerts setup), then Phase 9 (Documentation & Cleanup).
2. **Ticket Activation - Step 4:** Implement event-driven ticket expiration (Azure Functions with Timer Trigger to check and expire tickets automatically).
3. ~~**QR Code Generation:** Generate QR codes for tickets to enable scanning functionality (secondary activation trigger).~~ ✓ **COMPLETE**
4. ~~**Integration Tests:** Implement integration tests for ticket lifecycle (create, activate, expire, delete).~~ ✓ **COMPLETE**
5. **Ticket Search Functionality:** Add search and filtering capabilities to the admin booking management page.
6. **Shopping Cart (Bonus F):** Implement shopping cart functionality to allow users to add multiple tickets before payment.
7. **Error Handling Review:** Review error handling across the application and ensure user-friendly error messages.
8. **Code & Documentation Cleanup:** Final cleanup of code and documentation before demo handover.

---


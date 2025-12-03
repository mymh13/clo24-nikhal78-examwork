# Week 6 Action Plan – Pre-Demo Preparation

**Goal:** Clean up code, implement small value-add features, and prepare documentation before demo handover.

**Status:** Planning Phase  
**Created:** 2025-12-02  
**Target Completion:** Before demo preparation

---

## Scope Clarification

### Completed Features (Documentation Needed)

**Users & Inspectors - COMPLETE:**
- User role management complete (Admin, Inspector, User roles implemented)
- Users can book tickets today (booking functionality operational)
- Inspector role implemented (can view bookings, manage users)
- **Will NOT implement:** User registration flow (users are created by Admin)
- **Will NOT implement:** QR code scanning system for Inspectors (no physical QR readers available for demo)

**Documentation Task:** Add note to journals that Users and Inspectors are complete for demo purposes. Registration and scanning systems are out of scope.

---

## Priority 1: Critical Bugfixes & Code Quality

### 1.1 Bugfixes Review ✓ COMPLETE
**Time Estimate:** 2-4 hours  
**Priority:** HIGH  
**Status:** ✓ Complete (2025-12-03)

**Tasks:**
- [x] Review application logs for any runtime errors
- [x] Test all user flows (Admin, Inspector, User)
- [x] Verify feature flag toggle works correctly
- [x] Test booking creation in both Synchronous and Event-Driven modes
- [x] Check Application Insights telemetry is being sent correctly
- [x] Verify all API endpoints return appropriate status codes
- [x] Test error scenarios (invalid inputs, missing data, etc.)
- [x] Fixed bug #1: Login page redirecting all users to Admin Dashboard
- [x] Fixed bug #2: Integration tests creating real users in Cosmos DB

**Deliverable:** ✓ All bugs from bug backlog fixed

**Bugs Fixed:**
- ✓ **Bug #1:** Login page hardcoded "/admin" link - Fixed by using `NavigationHelper.GetLandingPageUrl()` to redirect users to their role-specific landing page
- ✓ **Bug #2:** Integration tests creating real users in Cosmos DB - Fixed by implementing in-memory mock services

---

## Priority 2: Testing Implementation

### 2.1 Unit Tests - Price Calculations ✓ COMPLETE
**Time Estimate:** 3-4 hours  
**Priority:** HIGH  
**Dependencies:** None  
**Status:** ✓ Complete (2025-12-02)

**Tasks:**
- [x] Create test project (`Ticketing.Web.Tests`)
- [x] Add xUnit and NSubstitute packages
- [x] Write unit tests for `PriceCalculationHelper.CalculatePriceModifier()`:
  - [x] Child (<12 years) → 0.0 modifier
  - [x] Student (12-65 years) → 0.5 modifier
  - [x] Pensioner (65+ years) → 0.5 modifier
  - [x] Standard (12-65, non-student) → 1.0 modifier
  - [x] Null DateOfBirth with IsStudent → 0.5 modifier
  - [x] Null DateOfBirth without IsStudent → 1.0 modifier
  - [x] Edge cases (exactly 12, exactly 65, birthday today)
- [x] Write unit tests for `PriceCalculationHelper.CalculateTotalPrice()`:
  - [x] Zero zones → 0m
  - [x] Single zone with different modifiers
  - [x] Multiple zones with different modifiers
  - [x] Custom base price per zone
- [x] Run tests and verify coverage

**Deliverable:** ✓ Test project with passing unit tests for price calculations (22 tests, all passing)

**Files Created:**
- ✓ `src/tests/Ticketing.Web.Tests/Ticketing.Web.Tests.csproj`
- ✓ `src/tests/Ticketing.Web.Tests/Helpers/PriceCalculationHelperTests.cs`

---

### 2.2 Integration Tests - Ticket Lifecycle ✓ COMPLETE
**Time Estimate:** 4-6 hours  
**Priority:** MEDIUM  
**Dependencies:** 2.1 (test project setup)
**Status:** ✓ Complete (2025-12-03)

**Tasks:**
- [x] Set up integration test infrastructure:
  - [x] Test database/container setup (Cosmos DB emulator or test container)
  - [x] Test HTTP client setup for API testing (`WebApplicationFactory`)
  - [x] Test user authentication setup (test authentication handler with Admin role)
- [x] Write integration tests for booking lifecycle:
  - [x] Create booking → verify stored in database (`CreateBooking_WithValidData_ReturnsCreatedBooking`)
  - [x] Get booking by ID → verify data matches (`GetBookingById_WithValidId_ReturnsBooking`)
  - [x] Get bookings by customer → verify filtering works (`GetBookingsByCustomer_ReturnsOnlyCustomerBookings`)
  - [x] Delete booking → verify removal from database (`DeleteBooking_WithValidId_RemovesBooking`)
  - [x] Verify price calculation is applied correctly during creation (`CreateBooking_CalculatesPriceCorrectly`)
  - [x] Activate booking → verify status and validity period (`ActivateBooking_WithValidBooking_UpdatesStatus`)
  - [x] Prevent duplicate activation (`ActivateBooking_AlreadyActivated_ReturnsBadRequest`)
- [ ] Test event-driven flow (if time permits):
  - [ ] Create booking in Event-Driven mode → verify outbox event created
  - [ ] Verify telemetry events are sent
- [x] Run tests and document results (All 7 tests passing)

**Deliverable:** ✓ Integration test suite for ticket lifecycle

**Files Created:**
- ✓ `src/tests/Ticketing.Web.Tests/Integration/BookingLifecycleTests.cs`
- ✓ `src/tests/Ticketing.Web.Tests/Integration/WebApplicationFactoryFixture.cs`
- ✓ `src/tests/Ticketing.Web.Tests/Integration/README.md`
- ✓ `src/web/Ticketing.Web/ProgramReference.cs` (for WebApplicationFactory access)

**Test Coverage:**
- ✓ 7 integration tests covering full booking lifecycle
- ✓ Tests use `WebApplicationFactory` to host the application
- ✓ Test authentication bypasses Azure AD (uses test handler)
- ✓ Cosmos DB configured for testing (emulator or test connection string)
- ✓ External services disabled during tests (Application Insights, Service Bus, OutboxProcessor)
- ✓ All tests passing successfully

---

## Priority 3: Feature Implementation

### 3.1 Ticket Activation Timer (Multi-Step Implementation)

**Important:** If ticket activation timers are implemented, they should be tied to the event-driven Azure tools already in place for a minimalistic design.

**Implementation Strategy:** Break down into 4 incremental steps for better manageability and testing.

---

#### 3.1.1 Step 1: Add Activation Fields to Booking Model ✓ COMPLETE
**Time Estimate:** 30-45 minutes  
**Priority:** MEDIUM  
**Dependencies:** None  
**Status:** ✓ Complete (2025-12-02)

**Tasks:**
- [x] Create `TicketStatus` static class with string constants (changed from enum to avoid serialization issues):
  - [x] `Created` - Ticket booked but not activated
  - [x] `Activated` - Ticket activated by user
  - [x] `Valid` - Ticket is currently valid (within validity period)
  - [x] `Expired` - Ticket validity period has passed
  - [x] Added validation helpers: `TicketIsValid()` and `GetAllTicketStatuses()`
- [x] Add fields to `Booking` model:
  - [x] `ActivatedAt` (DateTime?, nullable) - When ticket was activated
  - [x] `ValidFrom` (DateTime?, nullable) - Start of validity period
  - [x] `ValidTo` (DateTime?, nullable) - End of validity period
  - [x] `Status` (string, default: TicketStatus.Created)
- [x] Update Cosmos DB serialization (using string constants avoids enum serialization issues)
- [x] Verify existing bookings still work (backward compatibility - all new fields nullable/default, build successful)

**Deliverable:** ✓ Booking model extended with activation fields

**Files Created:**
- ✓ `src/shared/Ticketing.Contracts/Bookings/TicketStatus.cs`

**Files Modified:**
- ✓ `src/shared/Ticketing.Contracts/Bookings/Booking.cs`

**Backward Compatibility:**
- All new fields are nullable (ActivatedAt, ValidFrom, ValidTo) or have defaults (Status = Created)
- Using string constants instead of enum avoids Cosmos DB serialization issues
- Existing bookings in Cosmos DB will deserialize correctly (missing fields will be null/default)
- Build and tests pass successfully

**Design Decision:** Used string constants instead of enum to avoid past Cosmos DB serialization issues. Trade-off: less compile-time type safety but more flexibility and fewer serialization headaches.

---

#### 3.1.2 Step 2: Add Activation API Endpoint and Logic ✓ COMPLETE
**Time Estimate:** 1-2 hours  
**Priority:** MEDIUM  
**Dependencies:** 3.1.1 (activation fields)
**Status:** ✓ Complete (2025-12-02)

**Tasks:**
- [x] Create activation helper/service:
  - [x] `TicketActivationHelper` static class created
  - [x] Method to calculate validity period (90 minutes from activation)
  - [x] Method to validate activation (booking belongs to user, not already activated, etc.)
  - [x] Use `TicketStatus.TicketIsValid()` for status validation
- [x] Add API endpoint in `BookingsController`:
  - [x] `POST /api/bookings/{bookingId}/activate`
  - [x] Accept activation time from request body (optional, defaults to current time)
  - [x] Validate booking belongs to authenticated user (or Admin/Inspector can activate any)
  - [x] Set `ActivatedAt`, `ValidFrom`, `ValidTo`, and `Status`
  - [x] Update booking in Cosmos DB via `BookingService.UpdateBookingAsync()`
  - [x] Return updated booking
- [x] Add error handling:
  - [x] Booking not found → 404
  - [x] Booking doesn't belong to user → 400 with appropriate message
  - [x] Booking already activated → 400 with appropriate message
- [x] Test activation endpoint manually (verified via UI)

**Deliverable:** ✓ Activation API endpoint operational

**Files Created:**
- ✓ `src/web/Ticketing.Web/Helpers/TicketActivationHelper.cs`

**Files Modified:**
- ✓ `src/web/Ticketing.Web/Controllers/BookingsController.cs`
- ✓ `src/web/Ticketing.Web/Services/BookingService.cs` (added `GetBookingByIdAsync` and `UpdateBookingAsync`)
- ✓ `src/web/Ticketing.Web/Services/IBookingService.cs` (added method signatures)

---

#### 3.1.3 Step 3: Add Activation UI to User Landing Page ✓ COMPLETE
**Time Estimate:** 1-2 hours  
**Priority:** MEDIUM  
**Dependencies:** 3.1.2 (activation API)
**Status:** ✓ Complete (2025-12-02)

**Tasks:**
- [x] Update `UserLandingPage.razor`:
  - [x] Display ticket status for each booking (Created/Activated/Valid/Expired)
  - [x] Add "Activate Ticket" button for bookings with status `Created`
  - [x] Show validity period for activated tickets (`ValidFrom` to `ValidTo`)
  - [x] Display time remaining or "Expired" status for expired tickets
  - [x] Call activation API endpoint on button click
  - [x] Refresh booking list after activation
  - [x] Show success/error messages
- [x] Update booking display:
  - [x] Show status badge (color-coded: Created=gray, Activated=blue, Valid=green, Expired=red)
  - [x] Show activation time if activated
  - [x] Show validity period if valid
- [x] Test UI flow end-to-end (verified working)

**Deliverable:** ✓ User-facing activation UI operational

**Files Created:**
- ✓ `src/web/Ticketing.Web/Components/BookingTable.razor` (reusable component for booking display)

**Files Modified:**
- ✓ `src/web/Ticketing.Web/Pages/UserLandingPage.razor` (integrated BookingTable component)
- ✓ `src/web/Ticketing.Web/Components/BookingManagement.razor` (integrated BookingTable component)
- ✓ `src/web/Ticketing.Web/Pages/Bookings.razor` (integrated BookingTable component)
- ✓ `src/web/Ticketing.Web/wwwroot/css/custom.css` (added status badge styles)

---

#### 3.1.4 Step 4: Add Event-Driven Expiration (Optional)
**Time Estimate:** 2-3 hours  
**Priority:** LOW (Can be deferred)  
**Dependencies:** 3.1.3 (activation UI), Event-driven infrastructure

**Important:** This step integrates with existing Azure event-driven infrastructure for a minimalistic design.

**Tasks:**
- [ ] Create `TicketExpired` event contract:
  - [ ] `src/shared/Ticketing.Contracts/Events/TicketExpired.cs`
  - [ ] Include booking ID, customer ID, expired timestamp
- [ ] Create Timer Function:
  - [ ] `src/functions/Ticketing.Functions/Functions/CheckTicketExpirationFunction.cs`
  - [ ] Timer trigger: cron `0 */5 * * * *` (every 5 minutes)
  - [ ] Query Cosmos DB for tickets where `ValidTo < DateTime.UtcNow` and `Status != Expired`
  - [ ] For each expired ticket:
    - [ ] Update status to `Expired` in Cosmos DB
    - [ ] Publish `TicketExpired` event to Service Bus (reuse `IEventPublisher`)
    - [ ] Track expiration via Application Insights (reuse `ITelemetryService`)
    - [ ] Optionally store expiration event in outbox for audit
- [ ] Test timer function:
  - [ ] Create test booking with past `ValidTo` date
  - [ ] Run function manually or wait for timer
  - [ ] Verify status updated, event published, telemetry tracked
- [ ] Deploy function to Azure (if not already deployed)

**Deliverable:** Event-driven ticket expiration operational

**Files to Create:**
- `src/shared/Ticketing.Contracts/Events/TicketExpired.cs`
- `src/functions/Ticketing.Functions/Functions/CheckTicketExpirationFunction.cs`

**Files to Modify:**
- `src/functions/Ticketing.Functions/Program.cs` (if needed for DI setup)

**Note:** This step can be deferred if time is limited. Steps 1-3 provide core activation functionality without expiration automation.

---

### 3.1 Summary
**Total Time Estimate:** 4.5-7.75 hours (across 4 steps)  
**Priority:** MEDIUM  
**Dependencies:** None (Step 1), then sequential

**Implementation Order:**
1. Step 1: Add activation fields (30-45 min) - Foundation
2. Step 2: Add activation API (1-2 hours) - Backend logic
3. Step 3: Add activation UI (1-2 hours) - User interface
4. Step 4: Event-driven expiration (2-3 hours) - Optional automation

**Benefits of Incremental Approach:**
- Each step is testable independently
- Can stop after Step 3 if time is limited (manual expiration checking)
- Step 4 adds automation but isn't required for basic functionality
- Easier to review and debug smaller changes

**Event-Driven Architecture Integration:**
- **Azure Functions with Timer Trigger** - Use cron-based timer function to periodically check for expired tickets (e.g., every 5 minutes)
- **Azure Service Bus** - Publish `TicketExpired` events to existing `booking-events` queue when tickets expire
- **Application Insights** - Track expiration events via custom telemetry (reuse existing `ITelemetryService`)
- **Outbox Pattern** - Optionally store expiration events in outbox for audit trail (reuse existing `IOutboxService`)

**Minimalistic Design Approach:**
1. **Timer Function** - Create new Azure Function with Timer Trigger (cron: `0 */5 * * * *` for every 5 minutes)
2. **Expiration Check** - Function queries Cosmos DB for tickets where `ValidTo < DateTime.UtcNow` and `Status != Expired`
3. **Event Publishing** - When tickets expire, publish `TicketExpired` event to Service Bus (reuse existing event publisher infrastructure)
4. **Status Update** - Update ticket status in Cosmos DB to `Expired`
5. **Telemetry** - Track expiration via Application Insights custom events

**Benefits:**
- Reuses existing Azure infrastructure (Service Bus, Functions, Application Insights)
- No additional Azure services required
- Consistent with existing event-driven architecture
- Minimal code changes (extend existing Function App)

**Tasks:**
- [ ] Add activation fields to `Booking` model:
  - [ ] `ActivatedAt` (DateTime?, nullable)
  - [ ] `ValidFrom` (DateTime?, nullable)
  - [ ] `ValidTo` (DateTime?, nullable)
  - [ ] `Status` (enum: Created, Activated, Valid, Expired)
- [ ] Create activation service/helper:
  - [ ] Manual activation (user selects start time)
  - [ ] Automatic activation on QR scan (future-proof for now)
  - [ ] Calculate validity period (e.g., 90 minutes from activation)
- [ ] Update `UserLandingPage.razor`:
  - [ ] Add "Activate Ticket" UI under "Active Tickets" section
  - [ ] Allow user to select activation time
  - [ ] Show ticket status (Created/Activated/Valid/Expired)
  - [ ] Display validity period
- [ ] Add API endpoint for activation:
  - [ ] `POST /api/bookings/{bookingId}/activate`
  - [ ] Validate booking belongs to user
  - [ ] Set activation time and calculate validity
- [ ] Update booking display to show status
- [ ] **Event-Driven Expiration (if implementing timers):**
  - [ ] Create `TicketExpired` event contract in `Ticketing.Contracts.Events`
  - [ ] Add new Azure Function with Timer Trigger (`CheckTicketExpirationFunction`)
  - [ ] Function queries Cosmos DB for expired tickets
  - [ ] Publish `TicketExpired` events to Service Bus (reuse existing `IEventPublisher`)
  - [ ] Update ticket status to `Expired` in Cosmos DB (use `TicketStatus.Expired` constant)
  - [ ] Track expiration events via Application Insights (reuse `ITelemetryService`)
  - [ ] Optionally store expiration events in outbox for audit
  - [ ] Use `TicketStatus.TicketIsValid()` for status validation if needed

**Deliverable:** Ticket activation functionality with manual timer + optional event-driven expiration

**Files to Modify:**
- `src/shared/Ticketing.Contracts/Bookings/Booking.cs`
- `src/web/Ticketing.Web/Controllers/BookingsController.cs`
- `src/web/Ticketing.Web/Pages/UserLandingPage.razor`
- `src/web/Ticketing.Web/Services/BookingService.cs` (if needed)

**Files to Create:**
- `src/web/Ticketing.Web/Helpers/TicketActivationHelper.cs` (if needed)
- `src/shared/Ticketing.Contracts/Bookings/TicketStatus.cs` (enum)
- **If implementing event-driven expiration:**
  - `src/shared/Ticketing.Contracts/Events/TicketExpired.cs` (event contract)
  - `src/functions/Ticketing.Functions/Functions/CheckTicketExpirationFunction.cs` (timer function)

---

### 3.2 QR Code Generation ✓ COMPLETE
**Time Estimate:** 2-3 hours  
**Priority:** MEDIUM  
**Dependencies:** 3.1 (ticket activation)
**Status:** ✓ Complete (2025-12-03)

**Tasks:**
- [x] Add QR code library (`QRCoder` NuGet package v1.7.0)
- [x] Generate QR code for booking:
  - [x] Include booking ID, customer ID, activation timestamp, validity period
  - [x] Encode as JSON with version information
- [x] Add QR code display to `BookingTable` component:
  - [x] "Show QR Code" button for activated tickets
  - [x] Modal popup displaying QR code with booking details
- [x] Store QR code data in booking (base64-encoded PNG in `Booking.QrCodeData`)
- [x] Generate QR code during activation (automatic generation on ticket activation)
- [x] Admin/Inspector activation support (can activate any ticket to generate QR codes)

**Deliverable:** ✓ QR code generation and display for tickets

**Files Created:**
- ✓ `src/web/Ticketing.Web/Helpers/QrCodeHelper.cs`
- ✓ `docs/adr/ADR-020-qr-code-implementation.md`

**Files Modified:**
- ✓ `src/shared/Ticketing.Contracts/Bookings/Booking.cs` (added `QrCodeData` field)
- ✓ `src/web/Ticketing.Web/Controllers/BookingsController.cs` (generate QR code on activation)
- ✓ `src/web/Ticketing.Web/Components/BookingTable.razor` (added QR code button and modal)
- ✓ `src/web/Ticketing.Web/Helpers/TicketActivationHelper.cs` (clarified Admin/Inspector activation support)

**NuGet Package:**
- ✓ `QRCoder` (v1.7.0)

**Implementation Details:**
- QR code generated at activation time (not before activation)
- Base64-encoded PNG image stored in Cosmos DB for fast retrieval
- JSON-encoded data includes booking ID, customer ID, activation time, validity period, status, version
- Modal popup displays QR code with booking information
- Text color fixed to black for better readability

---

### 3.3 Shopping Cart Functionality (Simulated)
**Time Estimate:** 4-6 hours  
**Priority:** LOW  
**Dependencies:** None

**Tasks:**
- [ ] Create shopping cart model:
  - [ ] `ShoppingCart` class (in-memory or session-based)
  - [ ] `CartItem` class (zone, quantity, price)
- [ ] Add cart service:
  - [ ] Add item to cart
  - [ ] Remove item from cart
  - [ ] Update item quantity
  - [ ] Calculate cart total
  - [ ] Clear cart
- [ ] Update booking creation flow:
  - [ ] Allow adding multiple tickets to cart
  - [ ] Show cart summary
  - [ ] "Checkout" button creates all bookings at once
  - [ ] No real payment integration (simulated)
- [ ] Add cart UI:
  - [ ] Cart icon with item count
  - [ ] Cart page showing items
  - [ ] Add/remove items
  - [ ] Checkout button

**Deliverable:** Shopping cart functionality (simulated payment)

**Files to Create:**
- `src/web/Ticketing.Web/Models/ShoppingCart.cs`
- `src/web/Ticketing.Web/Services/IShoppingCartService.cs`
- `src/web/Ticketing.Web/Services/ShoppingCartService.cs`
- `src/web/Ticketing.Web/Pages/Cart.razor`

**Files to Modify:**
- `src/web/Ticketing.Web/Pages/UserLandingPage.razor` (add to cart button)
- `src/web/Ticketing.Web/Controllers/BookingsController.cs` (bulk create endpoint)

---

## Priority 3.4: Price Configuration (Quick Win) ✓ COMPLETE

### 3.4 Move Base Price to Configuration
**Time Estimate:** 1-2 hours  
**Priority:** MEDIUM (Quick win, high value)  
**Dependencies:** None  
**Status:** ✓ Complete (2025-12-02)

**Context:** Base price per zone (currently 20 SEK) is hardcoded in `PriceCalculationHelper` and `BookingsController`. ADR-011 identifies this as a disadvantage and recommends moving to configuration for runtime adjustment without code deployment.

**Tasks:**
- [x] Add base price configuration to `appsettings.json`:
  - [x] `"Pricing:BasePricePerZone": 20.0`
- [ ] Add configuration to Azure App Configuration (if using):
  - [ ] Add pricing settings to App Configuration (future enhancement)
  - [ ] Update sentinel key if needed
- [x] Update `PriceCalculationHelper.CalculateTotalPrice()`:
  - [x] Keep method signature (uses default parameter, configuration read in controller)
- [x] Update `BookingsController`:
  - [x] Inject `IConfiguration`
  - [x] Read base price from configuration instead of hardcoded value
  - [x] Pass to `PriceCalculationHelper` method
- [x] Test price calculation with different configuration values (verified via booking creation)
- [x] Document configuration key in ADR-011

**Deliverable:** ✓ Base price configurable via appsettings.json (Azure App Configuration can be added later for hot-reload)

**Files Modified:**
- ✓ `src/web/Ticketing.Web/Controllers/BookingsController.cs`
- ✓ `src/web/Ticketing.Web/appsettings.json`
- ✓ `docs/adr/ADR-011-price-calculation-system.md` (updated)

**Note:** ✓ Quick win completed. Base price is now configurable. Azure App Configuration integration can be added later for runtime adjustment without code deployment.

**Note on TicketStatus:** Changed from enum to string constants (`TicketStatus` static class) to avoid past Cosmos DB serialization issues. This provides more flexibility and fewer serialization headaches, though with slightly less type safety.

---

## Priority 4: Error Handling Review

### 4.1 Error Handling Audit
**Time Estimate:** 2-3 hours  
**Priority:** MEDIUM  
**Dependencies:** None

**Tasks:**
- [ ] Review all API endpoints for error handling:
  - [ ] Verify appropriate HTTP status codes
  - [ ] Check error messages are user-friendly
  - [ ] Ensure sensitive information is not exposed
- [ ] Review UI error handling:
  - [ ] Check error messages in Razor pages
  - [ ] Verify validation messages are clear
  - [ ] Test error scenarios in UI
- [ ] Review global exception handling:
  - [ ] Check `GlobalExceptionHandler` (if exists)
  - [ ] Verify logging is appropriate
  - [ ] Ensure user-facing messages are generic
- [ ] Document missing error handling:
  - [ ] List scenarios not covered
  - [ ] Prioritize critical gaps
  - [ ] Implement high-priority fixes

**Deliverable:** Error handling audit report and fixes

**Files to Review:**
- `src/web/Ticketing.Web/Controllers/*.cs`
- `src/web/Ticketing.Web/Pages/*.razor`
- `src/web/Ticketing.Web/Extensions/*.cs` (exception handling middleware)

---

## Priority 5: Code & Documentation Cleanup

### 5.1 Code Cleanup
**Time Estimate:** 4-6 hours  
**Priority:** HIGH  
**Dependencies:** All implementation tasks complete

**Tasks:**
- [ ] Remove unused code:
  - [ ] Unused using statements
  - [ ] Unused methods/classes
  - [ ] Commented-out code
  - [ ] Dead code paths
- [ ] Code formatting:
  - [ ] Run code formatter (dotnet format)
  - [ ] Ensure consistent naming conventions
  - [ ] Fix code style issues
- [ ] Code organization:
  - [ ] Verify file structure follows ADR-017
  - [ ] Check namespace consistency
  - [ ] Ensure proper separation of concerns
- [ ] Documentation comments:
  - [ ] Add XML comments to public APIs
  - [ ] Document complex logic
  - [ ] Update method documentation
- [ ] Build warnings:
  - [ ] Fix all compiler warnings
  - [ ] Address nullable reference warnings
  - [ ] Resolve code analysis issues

**Deliverable:** Clean, well-documented codebase

---

### 5.2 Journal & Documentation Cleanup
**Time Estimate:** 3-4 hours  
**Priority:** HIGH  
**Dependencies:** All implementation tasks complete

**Tasks:**
- [ ] Update `week_six.md`:
  - [ ] Mark completed features
  - [ ] Document decisions made
  - [ ] Add notes about Users/Inspectors completion
  - [ ] Document out-of-scope items (registration, scanning)
- [ ] Review all journal entries:
  - [ ] Ensure consistency
  - [ ] Fix typos/formatting
  - [ ] Update status of features
- [ ] Update `Index.razor` status:
  - [ ] Reflect current project state
  - [ ] Update next steps
- [ ] Create demo preparation notes:
  - [ ] Key features to highlight
  - [ ] Known limitations
  - [ ] Demo flow suggestions
- [ ] Update ADRs if needed:
  - [ ] Review ADR-017 (Service/Component Organization) if structure changed
  - [ ] Create ADR-020 (Testing Strategy) if testing is implemented
  - [ ] Update any ADRs with new decisions

**Deliverable:** Clean, up-to-date documentation

**Files to Update:**
- `docs/journal/week_six.md`
- `src/web/Ticketing.Web/Pages/Index.razor`
- `docs/adr/README.md` (if new ADRs created)
- `README.md` (if needed)

---

## Implementation Order (Recommended)

1. ✓ **Price Configuration** (Priority 3.4) - ✓ COMPLETE
2. ✓ **Unit Tests - Price Calculations** (Priority 2.1) - ✓ COMPLETE
3. ✓ **Ticket Activation - Step 1** (Priority 3.1.1) - ✓ COMPLETE
4. ✓ **Ticket Activation - Step 2** (Priority 3.1.2) - ✓ COMPLETE
5. ✓ **Ticket Activation - Step 3** (Priority 3.1.3) - ✓ COMPLETE
6. ✓ **Integration Tests** (Priority 2.2) - ✓ COMPLETE - Test ticket lifecycle
7. **Code Cleanup** (Priority 5.1) - Do incrementally as you work
8. **Error Handling Review** (Priority 4.1) - Important for demo quality
9. **Ticket Activation - Step 4** (Priority 3.1.4) - Event-driven expiration (optional, can defer)
10. ✓ **QR Code Generation** (Priority 3.2) - ✓ COMPLETE
11. **Shopping Cart** (Priority 3.3) - Nice to have, low priority
12. ✓ **Bugfixes Review** (Priority 1.1) - ✓ COMPLETE
13. **Documentation Cleanup** (Priority 5.2) - Final step before demo

---

## Time Estimates Summary

| Task | Time Estimate | Priority | Status |
|------|---------------|----------|--------|
| Price Configuration | 1-2 hours | MEDIUM | ✓ COMPLETE |
| Unit Tests (Price) | 3-4 hours | HIGH | ✓ COMPLETE |
| Ticket Activation - Step 1 | 30-45 min | MEDIUM | ✓ COMPLETE |
| Ticket Activation - Step 2 | 1-2 hours | MEDIUM | ✓ COMPLETE |
| Ticket Activation - Step 3 | 1-2 hours | MEDIUM | ✓ COMPLETE |
| Integration Tests | 4-6 hours | MEDIUM | ✓ COMPLETE |
| Ticket Activation - Step 4 | 2-3 hours | LOW | ⏳ Optional |
| QR Code Generation | 2-3 hours | MEDIUM | ✓ COMPLETE |
| Shopping Cart | 4-6 hours | LOW | ⏳ Pending |
| Error Handling Review | 2-3 hours | MEDIUM | ⏳ Pending |
| Code Cleanup | 4-6 hours | HIGH | ⏳ Pending |
| Bugfixes Review | 2-4 hours | HIGH | ✓ COMPLETE |
| Documentation Cleanup | 3-4 hours | HIGH | ⏳ Pending |
| **Total Remaining** | **13-22 hours** | | |

**Note:** Focus on HIGH priority items first. MEDIUM and LOW priority items can be deferred if time is limited.

---

## Success Criteria

**Must Have (Before Demo):**
- No critical bugs
- Unit tests for price calculations
- Code cleanup complete
- Documentation updated (Users/Inspectors completion noted)
- Error handling reviewed and improved

**Nice to Have (If Time Permits):**
- Ticket activation timer
- QR code generation
- Integration tests
- Shopping cart
- Comprehensive error handling

---

## Priority 6: Future Enhancements (Would-Like-To-Do, Low Priority)

### 6.1 Domain Separation for Security
**Time Estimate:** 6-8 hours  
**Priority:** LOW (Future enhancement)  
**Dependencies:** None

**Context:** To improve security and enable multi-tenancy, the system could implement domain separation to isolate data and operations by domain/tenant. This would ensure that users can only access data within their assigned domain.

**Minimalistic Implementation Approach:**

**Option 1: Application-Level Filtering (Easiest)**
- Add `Domain` or `TenantId` field to data models (`Booking`, `User`, `OutboxEvent`)
- Filter all Cosmos DB queries by domain in service layer
- Add domain-based authorization checks in controllers
- Store user's domain in claims/session after authentication
- **Pros:** No infrastructure changes, works with existing partition keys
- **Cons:** Requires filtering in all queries, not enforced at database level

**Option 2: Composite Partition Keys (More Secure)**
- Create new containers with composite partition keys: `/domain/customerId` or `/domain/status`
- Migrate existing data to new containers (one-time migration)
- Update partition key usage in services
- **Pros:** Database-level isolation, better performance for domain-scoped queries
- **Cons:** Requires data migration, more complex partition key logic

**Option 3: Separate Containers per Domain (Most Secure, Most Complex)**
- Create separate Cosmos DB containers per domain (e.g., `bookings-domain1`, `bookings-domain2`)
- Use container selection based on user's domain
- **Pros:** Complete isolation, separate scaling per domain
- **Cons:** Complex infrastructure, higher operational overhead

**Recommended Approach for MVP:** Option 1 (Application-Level Filtering)
- Minimal code changes
- No infrastructure changes required
- Works with existing Cosmos DB partition keys
- Can be enhanced later with Option 2 if needed

**Implementation Tasks (if implementing):**
- [ ] Add `Domain` or `TenantId` field to data models:
  - [ ] `Booking.Domain` (string)
  - [ ] `User.Domain` (string)
  - [ ] `OutboxEvent.Domain` (string)
- [ ] Add domain to user claims/session after authentication
- [ ] Update service layer to filter by domain:
  - [ ] `BookingService` - filter queries by domain
  - [ ] `UserService` - filter queries by domain
  - [ ] `OutboxService` - filter queries by domain
- [ ] Add domain validation in controllers:
  - [ ] Verify user's domain matches requested resource's domain
  - [ ] Return 403 Forbidden if domain mismatch
- [ ] Update Cosmos DB queries to include domain filter:
  - [ ] `WHERE c.domain = @domain` in all queries
  - [ ] Include domain in partition key lookups (if using composite keys)
- [ ] Add domain-based RBAC (optional):
  - [ ] Create domain-specific roles (e.g., `Admin-Domain1`, `Admin-Domain2`)
  - [ ] Assign users to domain-specific roles
- [ ] Update Service Bus events to include domain (if using separate queues):
  - [ ] Add domain to event contracts
  - [ ] Optionally use separate queues per domain
- [ ] Add domain configuration:
  - [ ] Store domain mapping in App Configuration or Key Vault
  - [ ] Default domain for new users

**Security Benefits:**
- Data isolation between domains
- Prevents cross-domain data access
- Enables multi-tenant scenarios
- Better compliance with data residency requirements

**Considerations:**
- Existing partition keys (`/customerId`, `/status`, `/email`) remain unchanged
- Application-level filtering adds minimal overhead
- Can be implemented incrementally without breaking changes
- Future enhancement: migrate to composite partition keys for better performance

**Deliverable:** Domain separation with application-level filtering

**Files to Modify:**
- `src/shared/Ticketing.Contracts/Bookings/Booking.cs`
- `src/shared/Ticketing.Contracts/Users/User.cs`
- `src/shared/Ticketing.Contracts/Outbox/OutboxEvent.cs`
- `src/web/Ticketing.Web/Services/BookingService.cs`
- `src/web/Ticketing.Web/Services/UserService.cs`
- `src/web/Ticketing.Web/Services/OutboxService.cs`
- `src/web/Ticketing.Web/Controllers/*.cs` (add domain validation)

**Note:** This is a future enhancement that can be implemented if multi-tenancy or stricter data isolation is required. For MVP, current authorization (role-based) is sufficient.

---

## Notes

- **Users & Inspectors are COMPLETE** - Document this clearly in journals
- **Registration is OUT OF SCOPE** - Users created by Admin only
- **QR Scanning is OUT OF SCOPE** - No physical QR readers available
- **Focus on value-add features** that don't take too long
- **Code quality is important** - Clean code is easier to demo and maintain
- **Testing adds credibility** - Even basic unit tests show good practices
- **Domain Separation** - Future enhancement for multi-tenancy (low priority, would-like-to-do)

---

## Tracking

**Started:** [Date]  
**Completed:** [Date]  
**Total Time Spent:** [Hours]

**Progress:**
- [x] Priority 1: Bugfixes - ✓ COMPLETE
- [x] Priority 2: Testing (Unit Tests - ✓ Complete)
- [x] Priority 2: Testing (Integration Tests - ✓ COMPLETE)
- [x] Priority 3: Features (Price Configuration - ✓ Complete)
- [x] Priority 3: Features (Ticket Activation - Steps 1-3 ✓ Complete, Step 4 optional)
- [x] Priority 3: Features (QR Code Generation - ✓ COMPLETE)
- [ ] Priority 3: Features (Shopping Cart - ⏳ Pending)
- [ ] Priority 4: Error Handling
- [ ] Priority 5: Cleanup

**Completed:**
- ✓ Price Configuration (Priority 3.4) - 2025-12-02
- ✓ Unit Tests - Price Calculations (Priority 2.1) - 2025-12-02
- ✓ Ticket Activation - Step 1 (Priority 3.1.1) - 2025-12-02
- ✓ Ticket Activation - Step 2 (Priority 3.1.2) - 2025-12-02
- ✓ Ticket Activation - Step 3 (Priority 3.1.3) - 2025-12-02
- ✓ Integration Tests (Priority 2.2) - 2025-12-03
- ✓ QR Code Generation (Priority 3.2) - 2025-12-03
- ✓ Bugfixes Review (Priority 1.1) - 2025-12-03


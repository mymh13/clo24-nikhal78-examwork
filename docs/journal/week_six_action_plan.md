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

### 1.1 Bugfixes Review
**Time Estimate:** 2-4 hours  
**Priority:** HIGH

**Tasks:**
- [ ] Review application logs for any runtime errors
- [ ] Test all user flows (Admin, Inspector, User)
- [ ] Verify feature flag toggle works correctly
- [ ] Test booking creation in both Synchronous and Event-Driven modes
- [ ] Check Application Insights telemetry is being sent correctly
- [ ] Verify all API endpoints return appropriate status codes
- [ ] Test error scenarios (invalid inputs, missing data, etc.)

**Deliverable:** List of bugs found and fixed (if any)

---

## Priority 2: Testing Implementation

### 2.1 Unit Tests - Price Calculations
**Time Estimate:** 3-4 hours  
**Priority:** HIGH  
**Dependencies:** None

**Tasks:**
- [ ] Create test project (`Ticketing.Web.Tests` or similar)
- [ ] Add xUnit and NSubstitute packages
- [ ] Write unit tests for `PriceCalculationHelper.CalculatePriceModifier()`:
  - [ ] Child (<12 years) → 0.0 modifier
  - [ ] Student (12-65 years) → 0.5 modifier
  - [ ] Pensioner (65+ years) → 0.5 modifier
  - [ ] Standard (12-65, non-student) → 1.0 modifier
  - [ ] Null DateOfBirth with IsStudent → 0.5 modifier
  - [ ] Null DateOfBirth without IsStudent → 1.0 modifier
  - [ ] Edge cases (exactly 12, exactly 65, birthday today)
- [ ] Write unit tests for `PriceCalculationHelper.CalculateTotalPrice()`:
  - [ ] Zero zones → 0m
  - [ ] Single zone with different modifiers
  - [ ] Multiple zones with different modifiers
  - [ ] Custom base price per zone
- [ ] Run tests and verify coverage

**Deliverable:** Test project with passing unit tests for price calculations

**Files to Create:**
- `src/tests/Ticketing.Web.Tests/Ticketing.Web.Tests.csproj`
- `src/tests/Ticketing.Web.Tests/Helpers/PriceCalculationHelperTests.cs`

---

### 2.2 Integration Tests - Ticket Lifecycle
**Time Estimate:** 4-6 hours  
**Priority:** MEDIUM  
**Dependencies:** 2.1 (test project setup)

**Tasks:**
- [ ] Set up integration test infrastructure:
  - [ ] Test database/container setup (Cosmos DB emulator or test container)
  - [ ] Test HTTP client setup for API testing
  - [ ] Test user authentication setup
- [ ] Write integration tests for booking lifecycle:
  - [ ] Create booking → verify stored in database
  - [ ] Get booking by ID → verify data matches
  - [ ] Get bookings by customer → verify filtering works
  - [ ] Delete booking → verify removal from database
  - [ ] Verify price calculation is applied correctly during creation
- [ ] Test event-driven flow (if time permits):
  - [ ] Create booking in Event-Driven mode → verify outbox event created
  - [ ] Verify telemetry events are sent
- [ ] Run tests and document results

**Deliverable:** Integration test suite for ticket lifecycle

**Files to Create:**
- `src/tests/Ticketing.Web.Tests/Integration/BookingLifecycleTests.cs`
- `src/tests/Ticketing.Web.Tests/TestHelpers/TestFixture.cs` (if needed)

---

## Priority 3: Feature Implementation

### 3.1 Ticket Activation Timer
**Time Estimate:** 4-6 hours  
**Priority:** MEDIUM  
**Dependencies:** None

**Important:** If ticket activation timers are implemented, they should be tied to the event-driven Azure tools already in place for a minimalistic design.

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
  - [ ] Update ticket status to `Expired` in Cosmos DB
  - [ ] Track expiration events via Application Insights (reuse `ITelemetryService`)
  - [ ] Optionally store expiration events in outbox for audit

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

### 3.2 QR Code Generation
**Time Estimate:** 2-3 hours  
**Priority:** MEDIUM  
**Dependencies:** 3.1 (ticket activation)

**Tasks:**
- [ ] Add QR code library (e.g., `QRCoder` NuGet package)
- [ ] Generate QR code for booking:
  - [ ] Include booking ID, customer ID, activation status
  - [ ] Encode as JSON or simple string
- [ ] Add QR code display to `UserLandingPage.razor`:
  - [ ] Show QR code for activated tickets
  - [ ] Allow download/print of QR code
- [ ] Add QR code to booking details view
- [ ] Store QR code data in booking (optional, for validation later)

**Deliverable:** QR code generation and display for tickets

**Files to Modify:**
- `src/web/Ticketing.Web/Pages/UserLandingPage.razor`
- `src/web/Ticketing.Web/Controllers/BookingsController.cs` (add QR endpoint if needed)

**Files to Create:**
- `src/web/Ticketing.Web/Helpers/QrCodeHelper.cs`

**NuGet Package:**
- `QRCoder` or `ZXing.Net`

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

## Priority 3.4: Price Configuration (Quick Win)

### 3.4 Move Base Price to Configuration
**Time Estimate:** 1-2 hours  
**Priority:** MEDIUM (Quick win, high value)  
**Dependencies:** None

**Context:** Base price per zone (currently 20 SEK) is hardcoded in `PriceCalculationHelper` and `BookingsController`. ADR-011 identifies this as a disadvantage and recommends moving to configuration for runtime adjustment without code deployment.

**Tasks:**
- [ ] Add base price configuration to `appsettings.json`:
  - [ ] `"Pricing:BasePricePerZone": 20.0`
- [ ] Add configuration to Azure App Configuration (if using):
  - [ ] Add pricing settings to App Configuration
  - [ ] Update sentinel key if needed
- [ ] Update `PriceCalculationHelper.CalculateTotalPrice()`:
  - [ ] Accept `IConfiguration` or inject pricing service
  - [ ] Read base price from configuration with fallback to default
- [ ] Update `BookingsController`:
  - [ ] Read base price from configuration instead of hardcoded value
  - [ ] Pass to `PriceCalculationHelper` method
- [ ] Test price calculation with different configuration values
- [ ] Document configuration key in ADR-011 or configuration documentation

**Deliverable:** Base price configurable via appsettings.json/App Configuration

**Files to Modify:**
- `src/web/Ticketing.Web/Helpers/PriceCalculationHelper.cs`
- `src/web/Ticketing.Web/Controllers/BookingsController.cs`
- `src/web/Ticketing.Web/appsettings.json` (or `appsettings.Development.json`)

**Note:** This is a quick win that adds value without significant complexity. Can be done in parallel with other tasks.

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

1. **Bugfixes Review** (Priority 1.1) - Start immediately, identify issues early
2. **Unit Tests - Price Calculations** (Priority 2.1) - Quick win, establishes test infrastructure
3. **Code Cleanup** (Priority 5.1) - Do incrementally as you work
4. **Price Configuration** (Priority 3.4) - Quick win, high value, can be done in parallel
5. **Ticket Activation Timer** (Priority 3.1) - Core feature, enables QR codes
6. **QR Code Generation** (Priority 3.2) - Depends on activation
7. **Error Handling Review** (Priority 4.1) - Important for demo quality
7. **Integration Tests** (Priority 2.2) - If time permits
8. **Shopping Cart** (Priority 3.3) - Nice to have, low priority
9. **Documentation Cleanup** (Priority 5.2) - Final step before demo

---

## Time Estimates Summary

| Task | Time Estimate | Priority |
|------|---------------|----------|
| Bugfixes Review | 2-4 hours | HIGH |
| Unit Tests (Price) | 3-4 hours | HIGH |
| Integration Tests | 4-6 hours | MEDIUM |
| Ticket Activation | 4-6 hours | MEDIUM |
| QR Code Generation | 2-3 hours | MEDIUM |
| Price Configuration | 1-2 hours | MEDIUM |
| Shopping Cart | 4-6 hours | LOW |
| Error Handling Review | 2-3 hours | MEDIUM |
| Code Cleanup | 4-6 hours | HIGH |
| Documentation Cleanup | 3-4 hours | HIGH |
| **Total** | **29-44 hours** | |

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
- [ ] Priority 1: Bugfixes
- [ ] Priority 2: Testing
- [ ] Priority 3: Features
- [ ] Priority 4: Error Handling
- [ ] Priority 5: Cleanup


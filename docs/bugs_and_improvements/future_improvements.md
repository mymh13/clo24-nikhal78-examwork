# Future Improvements & Enhancements

This document tracks potential improvements and enhancements that are out of scope for the MVP but could be valuable additions in future iterations.

**Last Updated:** 2025-12-03

---

## User Experience & Features

### Shopping Cart Functionality
**Priority:** Medium  
**Estimated Effort:** 4-6 hours  
**Status:** Out of scope for MVP

**Description:**  
Allow users to add multiple tickets to a shopping cart before checkout. This would enable users to book multiple tickets in a single transaction rather than creating bookings one at a time.

**Rationale for Deferral:**  
Shopping cart functionality introduces complexity that could slow down quick bookings during demos. The current single-booking flow is simpler and faster for demonstration purposes.

**Implementation Considerations:**
- Create shopping cart model (in-memory or session-based)
- Add cart service with add/remove/update item functionality
- Update booking creation flow to support bulk creation
- Add cart UI with item count indicator
- Simulated payment flow (no real payment integration)

**References:**
- `docs/journal/week_six_action_plan.md` - Priority 3.3

---

### User Registration Flow
**Priority:** Low  
**Estimated Effort:** 2-4 hours  
**Status:** Out of scope for MVP

**Description:**  
Implement a self-service user registration flow where users can create their own accounts instead of requiring Admin to create accounts.

**Rationale for Deferral:**  
For MVP demo purposes, user accounts are managed by administrators to prevent bot registrations and maintain control over the user base.

**Implementation Considerations:**
- Registration form with email/password validation
- Email verification (optional)
- Integration with existing authentication system
- Rate limiting to prevent abuse

**References:**
- `docs/journal/week_six.md` - Scope Clarification section

---

### QR Code Scanning System for Inspectors
**Priority:** Medium  
**Estimated Effort:** 3-5 hours  
**Status:** Out of scope for MVP

**Description:**  
Implement QR code scanning functionality for Inspectors to validate tickets. This would require physical QR code readers or mobile device camera integration.

**Rationale for Deferral:**  
No physical QR readers are available for demo purposes. QR codes are generated and can be displayed, but scanning functionality requires additional hardware/software integration.

**Implementation Considerations:**
- QR code validation endpoint with rate limiting
- Mobile camera integration for scanning
- Ticket validation logic (check validity period, status)
- Inspector UI for scanning and validation results
- Audit logging for validation events

**References:**
- `docs/journal/week_six.md` - Scope Clarification section
- `docs/adr/ADR-020-qr-code-implementation.md`

---

### Ticket Search & Filtering
**Priority:** Medium  
**Estimated Effort:** 2-3 hours  
**Status:** Future enhancement

**Description:**  
Add search and filtering capabilities to the admin booking management page. Allow searching by customer email, booking ID, date range, status, etc.
  
**Rationale for Deferral:**  
Time limits on this project, and emphasis on the MVP aspect.
  
**Implementation Considerations:**
- Search input with filters
- Cosmos DB query optimization for search
- Date range filtering
- Status-based filtering
- Customer email/ID search

**References:**
- `docs/journal/week_six.md` - Next Steps section

---

## Architecture & Infrastructure

### Event-Driven Ticket Expiration (Step 4)
**Priority:** Low  
**Estimated Effort:** 2-3 hours  
**Status:** Optional, can be deferred

**Description:**  
Implement automated ticket expiration using Azure Functions with Timer Trigger. The function would periodically check for expired tickets and update their status automatically.

**Current State:**  
Steps 1-3 of ticket activation are complete (model updates, API endpoint, UI integration). Manual expiration checking is possible, but automated expiration would improve user experience.

**Implementation Considerations:**
- Create `TicketExpired` event contract
- Azure Function with Timer Trigger (cron: every 5 minutes)
- Query Cosmos DB for tickets where `ValidTo < DateTime.UtcNow` and `Status != Expired`
- Update status to `Expired` in Cosmos DB
- Publish `TicketExpired` event to Service Bus
- Track expiration via Application Insights

**References:**
- `docs/journal/week_six_action_plan.md` - Priority 3.1.4
- `docs/journal/week_six.md` - Ticket Activation Timer Implementation

---

### Domain Separation for Multi-Tenancy
**Priority:** Low  
**Estimated Effort:** 6-8 hours  
**Status:** Future enhancement

**Description:**  
Implement domain/tenant separation to enable multi-tenancy. This would isolate data and operations by domain, ensuring users can only access data within their assigned domain.

**Implementation Options:**
1. **Application-Level Filtering (Easiest)** - Add domain field to models, filter queries
2. **Composite Partition Keys** - Use `/domain/customerId` partition keys
3. **Separate Containers per Domain** - Complete isolation with separate containers

**Recommended Approach:** Option 1 (Application-Level Filtering) for MVP, can be enhanced later.

**References:**
- `docs/journal/week_six_action_plan.md` - Priority 6.1

---

### API Versioning
**Priority:** Low  
**Estimated Effort:** 2-3 hours  
**Status:** Future enhancement

**Description:**  
Add API versioning support to enable backward compatibility and gradual migration when making breaking changes to API endpoints.

**Implementation Considerations:**
- Route-based versioning: `[Route("api/v{version:apiVersion}/[controller]")]`
- Query parameter versioning
- Header-based versioning
- Version negotiation logic

**References:**
- `docs/adr/ADR-019-api-design-pattern-controller-based-rest.md` - Future Considerations section

---

### Swagger/OpenAPI Documentation
**Priority:** Low  
**Estimated Effort:** 1-2 hours  
**Status:** Future enhancement

**Description:**  
Add Swagger/OpenAPI documentation for API endpoints. This would enable API exploration, testing, and documentation generation.

**Implementation Considerations:**
- Add `Swashbuckle.AspNetCore` package
- Configure Swagger UI endpoint
- Add API documentation comments
- Generate OpenAPI specification

**References:**
- `docs/adr/ADR-019-api-design-pattern-controller-based-rest.md` - Future Considerations section

---

### Rate Limiting
**Priority:** Medium  
**Estimated Effort:** 2-3 hours  
**Status:** Future enhancement

**Description:**  
Implement rate limiting to protect API endpoints from abuse and ensure fair resource usage.

**Implementation Considerations:**
- Use `Microsoft.AspNetCore.RateLimiting` middleware
- Configure rate limits per endpoint or user
- Return appropriate HTTP status codes (429 Too Many Requests)
- Log rate limit violations

**References:**
- `docs/adr/ADR-019-api-design-pattern-controller-based-rest.md` - Future Considerations section

---

## Configuration & Pricing

### Azure App Configuration Integration for Base Price
**Priority:** Low  
**Estimated Effort:** 1-2 hours  
**Status:** Future enhancement

**Description:**  
Move base price configuration from `appsettings.json` to Azure App Configuration to enable runtime adjustment without code deployment.

**Current State:**  
Base price is configurable via `appsettings.json` (`Pricing:BasePricePerZone`). Moving to App Configuration would enable hot-reload capability.

**Implementation Considerations:**
- Add pricing settings to Azure App Configuration
- Use sentinel key pattern for hot-reload
- Update configuration refresh logic
- Maintain backward compatibility with appsettings.json

**References:**
- `docs/adr/ADR-011-price-calculation-system.md` - Base Price Configuration section
- `docs/journal/week_six_action_plan.md` - Priority 3.4

---

### Student Verification System
**Priority:** Low  
**Estimated Effort:** 3-5 hours  
**Status:** Future enhancement

**Description:**  
Add student verification beyond the simple boolean flag. This could include student ID validation or integration with student registry systems.

**Current State:**  
Student status is a simple boolean flag (`IsStudent`) on the User model. No verification is performed.

**Implementation Considerations:**
- Student ID validation
- Integration with student registry (if available)
- Verification workflow
- Audit logging for verification events

**References:**
- `docs/adr/ADR-011-price-calculation-system.md` - Risks/Mitigations section

---

### Region-Based Pricing
**Priority:** Low  
**Estimated Effort:** 4-6 hours  
**Status:** Future enhancement

**Description:**  
Implement region-based pricing where different regions can have different base prices per zone.

**Current State:**  
`Region` field exists on Booking model but is empty/unused. Base price is uniform across all zones.

**Implementation Considerations:**
- Define region data model
- Update price calculation to consider region
- Add region selection to booking UI
- Configure region-specific pricing in App Configuration

**References:**
- `docs/adr/ADR-011-price-calculation-system.md` - Extensibility section

---

### Multiple Zones Per Ticket
**Priority:** Low  
**Estimated Effort:** 3-4 hours  
**Status:** Future enhancement

**Description:**  
Allow a single ticket to cover multiple zones, with pricing calculated accordingly.

**Current State:**  
Each zone costs one ticket. Multiple zones can be selected, but each creates a separate booking.

**Implementation Considerations:**
- Update booking model to support multiple zones
- Modify price calculation logic
- Update UI to support multi-zone selection
- Update validation logic

**References:**
- `docs/adr/ADR-011-price-calculation-system.md` - Extensibility section

---

## Monitoring & Observability

### Application Insights Alerts
**Priority:** Medium  
**Estimated Effort:** 2-3 hours  
**Status:** Future enhancement

**Description:**  
Set up Application Insights alerts for critical scenarios such as dead letter queue messages, function failures, and outbox processing delays.

**References:**
- `docs/journal/week_six.md` - Event-Driven Architecture Progress section
- `docs/journal/eventdriven_roadmap.md` - Phase 8.3

---

### Push-Based Configuration Refresh (Webhooks)
**Priority:** Low  
**Estimated Effort:** 4-6 hours  
**Status:** Future enhancement

**Description:**  
Replace polling-based configuration refresh with push-based webhooks. Azure App Configuration can trigger webhooks when values change, eliminating polling overhead.

**Current State:**  
Configuration refresh uses polling (1-minute interval) to check sentinel key changes.

**Implementation Considerations:**
- Set up webhook endpoint (Azure Functions or Logic Apps)
- Configure App Configuration webhook triggers
- Update refresh logic to handle webhook events
- Maintain backward compatibility with polling

**References:**
- `docs/adr/ADR-014-sentinel-key-pattern.md` - Risks/Mitigations section

---

### Application Insights Dashboard Integration
**Priority:** Low  
**Estimated Effort:** 2-3 hours  
**Status:** Future enhancement (if needed)

**Description:**  
If Application Insights integration is needed in the web application (beyond Azure Portal), consider using Azure Portal dashboards or Power BI for visualization rather than building custom table rendering.

**Current State:**  
Application Insights queries are viewed in Azure Portal. Demo page was simplified to remove complex query rendering.

**References:**
- `docs/journal/week_six.md` - Reflection section

---

## Security & Authentication

### Full SSO (Single Sign-On) for Customers
**Priority:** Low  
**Estimated Effort:** 4-6 hours  
**Status:** Future enhancement

**Description:**  
Move customer authentication to Entra ID or another external identity provider to enable full SSO functionality.

**Current State:**  
Customers authenticate locally using ASP.NET Identity (BCrypt password hashing). Admins use Entra ID (OpenID Connect).

**Implementation Considerations:**
- Migrate customer accounts to Entra ID
- Update authentication flow
- Handle account migration
- Maintain backward compatibility

**References:**
- `docs/adr/ADR-002-authentication.md` - SSO functionality section

---

### API Management (APIM) Integration
**Priority:** Low  
**Estimated Effort:** 6-8 hours  
**Status:** Future enhancement

**Description:**  
Add Azure API Management (APIM) as a gateway for public GET endpoints, providing caching, rate limiting, and API versioning capabilities.

**Current State:**  
All API endpoints are directly exposed through App Service. No API gateway layer.

**Implementation Considerations:**
- Set up APIM instance
- Configure API policies (caching, rate limiting)
- Route public endpoints through APIM
- Maintain private endpoints for internal use

**References:**
- `docs/adr/ADR-005-azureservices.md` - API Management section

---

## Testing & Quality

### Additional Integration Tests
**Priority:** Medium  
**Estimated Effort:** 4-6 hours  
**Status:** Future enhancement

**Description:**  
Expand integration test coverage to include event-driven flow testing, Service Bus integration, and Azure Functions testing.

**Current State:**  
7 integration tests cover booking lifecycle. Event-driven flow testing is not yet implemented.

**Implementation Considerations:**
- Test outbox event creation and processing
- Test Service Bus event publishing
- Test Azure Function event handling
- Test feature flag toggling and propagation

**References:**
- `docs/journal/week_six_action_plan.md` - Priority 2.2
- `docs/journal/test_activation_endpoint.md`

---

## Data & Storage

### Composite Partition Keys for Domain Separation
**Priority:** Low  
**Estimated Effort:** 4-6 hours  
**Status:** Future enhancement (if domain separation is implemented)

**Description:**  
If domain separation is implemented, consider migrating to composite partition keys (`/domain/customerId`) for better performance and database-level isolation.

**Current State:**  
Partition keys are single-field (`/customerId`, `/status`, `/email`).

**References:**
- `docs/journal/week_six_action_plan.md` - Priority 6.1 (Option 2)

---

## Notes

- **MVP Focus:** All items in this document are explicitly out of scope for the MVP demo
- **Priority Levels:** 
  - **High:** Critical for production but not needed for MVP
  - **Medium:** Valuable enhancement that would improve user experience or maintainability
  - **Low:** Nice-to-have features that can be deferred indefinitely
- **Estimated Effort:** Rough estimates based on complexity and scope
- **Status:** Current state of each improvement (Out of scope, Future enhancement, Optional, etc.)

---

## Related Documentation

- **ADRs:** See `docs/adr/` for architecture decision records
- **Journals:** See `docs/journal/` for implementation details and progress tracking
- **Action Plan:** See `docs/journal/week_six_action_plan.md` for detailed task breakdowns


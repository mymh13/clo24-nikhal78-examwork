# Week 6 – Event-Driven Architecture Refinement & Demo Enhancements

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

During week 6, work focused on refining the demo page user experience and improving the Application Insights integration. The goal was to enhance the demo page's usability for live demonstrations by addressing container sizing issues, implementing text truncation for long values, and adding functionality to view and copy full data.

---

## Completed Activities

### Demo Page UX Improvements
- **Container Width Constraints:** Added `demo-page-container` class with `max-width: 95vw` to prevent horizontal scrolling on wide screens. Applied to main container to ensure page fits within viewport without requiring horizontal scrollbars.
- **Text Truncation:** Implemented automatic truncation for table cell values longer than 20 characters. Long values display with ellipsis (`...`) and show full text on hover via `title` attribute. Added CSS classes: `text-truncate-small` (120px max-width), `text-truncate-medium` (300px max-width).
- **Column Headers:** Enhanced table headers with sticky positioning on scroll. Added clear "Actions" column header for row operations. Headers remain visible when scrolling through long tables.
- **Copy Functionality:** 
  - Added "📋 Copy" button for each row to copy individual row data as formatted JSON
  - Added "📋 Copy Table Data" button for entire tables to copy all table data as JSON
  - Data copied to clipboard using browser Clipboard API with user feedback via alerts
- **Modal Popup for Full Data:** 
  - Added "👁️ View" button for each row to open modal popup
  - Modal displays data in two formats:
    - **Table View:** All columns and values in a readable table format
    - **JSON Format:** Properly formatted JSON with indentation for technical users
  - Modal includes "📋 Copy All Data" button to copy JSON to clipboard
  - Modal is responsive, scrollable, and can be closed via close button or clicking outside
- **Table Styling Improvements:**
  - Created dedicated `insights-table` CSS class with proper overflow handling
  - Cell max-width: 200px to prevent overflow
  - Word-break for long values to handle edge cases
  - Responsive container with horizontal scroll if needed (min-width: 600px)
  - Improved hover states and visual feedback

**Status:** Demo page UX significantly improved. Tables no longer overflow viewport, long values are truncated with clear indicators, and users can easily view and copy full data via modal. Ready for professional live demonstrations.

---

## Reflection

### What Went Well
- **Container Constraints:** Adding max-width constraints immediately resolved horizontal scrolling issues on wide screens, making the page more professional and easier to use.
- **Text Truncation:** Automatic truncation with hover tooltips provides clean table display while maintaining access to full data.
- **Modal Implementation:** Modal popup provides excellent user experience for viewing full data without leaving the page context.
- **Copy Functionality:** Clipboard API integration makes it easy to share data for debugging or documentation purposes.

### Challenges Encountered
- **Container Overflow:** Initial implementation allowed tables to expand beyond viewport width, causing horizontal scrolling. Solution: Added `max-width: 95vw` to main container and `max-width: 200px` to table cells with proper overflow handling.
- **Long Value Display:** Long GUIDs and JSON strings made tables hard to read. Solution: Implemented truncation with ellipsis and modal for full data viewing.
- **Column Header Clarity:** Without clear headers, it was difficult to understand what each column represented. Solution: Enhanced headers with sticky positioning and clear labeling.

### Lessons Learned
- **Viewport Constraints:** Always consider viewport width when designing tables with dynamic data. Use `max-width` constraints and overflow handling to prevent horizontal scrolling.
- **Text Truncation:** For tables with potentially long values, implement truncation with tooltips or modals to maintain readability while preserving access to full data.
- **User Feedback:** Copy operations should provide immediate feedback (alerts, toasts) so users know the action succeeded.
- **Modal Design:** Modals should display data in multiple formats (table for readability, JSON for technical users) to accommodate different use cases.
- **CSS Organization:** Dedicated CSS classes for specific components (e.g., `insights-table`, `modal-*`) improve maintainability and consistency.

### Key Achievements
- **Improved Demo Page UX:** Container constraints, text truncation, and modal popup make the demo page professional and easy to use for live demonstrations.
- **Enhanced Data Accessibility:** Copy functionality and modal viewing make it easy to access and share full data without cluttering the main view.
- **Responsive Design:** Page now works well on different screen sizes without horizontal scrolling issues.
- **Professional Appearance:** Clean table styling with proper overflow handling creates a polished user experience.

### What Could Be Improved
- **Toast Notifications:** Replace alert dialogs with toast notifications for copy operations to provide less intrusive feedback.
- **Export Functionality:** Add CSV/Excel export options for table data in addition to JSON copy.
- **Column Resizing:** Allow users to resize columns in the table for better customization.
- **Filtering/Sorting:** Add client-side filtering and sorting capabilities to tables for better data exploration.
- **Pagination:** For large result sets, implement pagination to improve performance and usability.

---

## Ongoing Work

### Event-Driven Architecture Infrastructure
- **Phase 1-7:** Complete - All infrastructure, contracts, outbox pattern, feature flags, Service Bus, Azure Functions, and testing completed.
- **Phase 8:** Monitoring & Observability - Phase 8.1 (Application Insights custom events) and Phase 8.2 (Demo page) complete. Remaining: Phase 8.3 (Alerts setup) and Phase 9 (Documentation & Cleanup).

---

## Event-Driven Architecture Progress (Week 6)

**Detailed roadmap:** See [Event-Driven Architecture Roadmap](eventdriven_roadmap.md) for complete implementation details and step-by-step progress.

### Phase 8: Monitoring & Observability (In Progress - Phase 8.1 & 8.2 Complete)

**Phase 8.2 Enhancements (Week 6):** Improved demo page user experience with container constraints, text truncation, copy functionality, and modal popup for viewing full data. All improvements focused on making the demo page more professional and easier to use for live demonstrations.

**Remaining Work:**
- **Phase 8.3:** Set up Application Insights alerts (dead letter queue messages, function failures, outbox processing delays)
- **Phase 9:** Documentation & Cleanup (update ADR-006, architecture.md, create developer guide, create comparison documentation, update week journal)

---

## Next Steps

1. **Event-Driven Architecture:** Complete Phase 8.3 (Application Insights alerts setup), then Phase 9 (Documentation & Cleanup).
2. **Ticket Activation:** Implement ticket activation timer with dual triggers (manual and QR code scan).
3. **QR Code Generation:** Generate QR codes for tickets to enable scanning functionality.
4. **Ticket Search Functionality:** Add search and filtering capabilities to the admin booking management page.
5. **Shopping Cart (Bonus F):** Implement shopping cart functionality to allow users to add multiple tickets before payment.
6. **Price Configuration:** Move base price to configuration for runtime adjustment without code deployment.

---


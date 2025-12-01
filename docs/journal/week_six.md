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
- **Layout Reorganization:** Reorganized page layout to optimize space usage:
  - **Event-Driven Architecture Status** moved to top (full width) for quick visibility
  - **Application Insights Query Results** positioned below (full width) to maximize horizontal space for tables
  - This layout provides better use of screen real estate and improves readability
- **Container Width Constraints:** Added `demo-page-container` class with `max-width: 95vw` to prevent horizontal scrolling on wide screens. Applied to main container to ensure page fits within viewport without requiring horizontal scrollbars.
- **Metadata Table Filtering:** Implemented filtering to exclude Application Insights metadata tables that contain raw JSON configuration:
  - Filters out `Visualization` table (contains visualization config JSON)
  - Filters out `QueryProperties` table (contains query metadata)
  - Filters out `QueryStatus` table (contains status info)
  - Filters out tables starting with `@` (extended properties)
  - Only displays `PrimaryResult` and other data tables with actual query results
  - Prevents raw JSON code from appearing as table fields
- **Fixed Table Layout with Column Constraints:**
  - Implemented `table-layout: fixed` with `<colgroup>` to enforce column widths
  - Data columns set to 200px width via colgroup
  - Actions column set to 140px fixed width
  - Prevents columns from expanding beyond defined widths regardless of content
- **Text Truncation:** Implemented automatic truncation for table cell values longer than 15 characters:
  - Long values display with ellipsis (`...`) and show full text on hover via `title` attribute
  - All cells use `text-truncate-cell` class with `white-space: nowrap` and `text-overflow: ellipsis`
  - CSS rules use `!important` flags to ensure truncation overrides any conflicting styles
  - Headers also truncate with same rules for consistency
  - Column width enforced at 200px with `width: 200px !important` (not just max-width)
- **Column Headers:** Enhanced table headers with proper truncation. Added clear "Actions" column header for row operations. Headers respect same width constraints as cells.
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
  - Switched from custom `insights-table` to `bookings-table` class for consistency
  - Added `insights-fixed-table` class with `table-layout: fixed !important`
  - Cell width: 200px enforced with `!important` flags
  - Word-break: normal to prevent breaking in middle of words
  - Position: relative on cells to ensure truncation works properly
  - Colgroup CSS rules to enforce column widths

**Status:** Demo page UX significantly improved and fully operational. Layout optimized for better space usage, metadata tables filtered out, tables properly constrained with fixed layout, long values truncated correctly, and users can easily view and copy full data via modal. Ready for professional live demonstrations.

---

## Reflection

### What Went Well
- **Container Constraints:** Adding max-width constraints immediately resolved horizontal scrolling issues on wide screens, making the page more professional and easier to use.
- **Text Truncation:** Automatic truncation with hover tooltips provides clean table display while maintaining access to full data.
- **Modal Implementation:** Modal popup provides excellent user experience for viewing full data without leaving the page context.
- **Copy Functionality:** Clipboard API integration makes it easy to share data for debugging or documentation purposes.

### Challenges Encountered
- **Container Overflow:** Initial implementation allowed tables to expand beyond viewport width, causing horizontal scrolling. Solution: Added `max-width: 95vw` to main container and enforced fixed table layout with colgroup column widths.
- **Long Value Display:** Long GUIDs and JSON strings (especially query execution statistics) made tables hard to read and broke layout. Solution: Implemented truncation with ellipsis, enforced with `!important` CSS flags, and modal for full data viewing. Used `table-layout: fixed` with colgroup to prevent column expansion.
- **Metadata Tables:** Application Insights API returns multiple tables including metadata tables (Visualization, QueryProperties, QueryStatus) that contain raw JSON configuration. These appeared as broken table displays with raw code. Solution: Added filtering logic to exclude metadata tables and only display actual data tables (PrimaryResult and similar).
- **Truncation Not Working:** Initial truncation CSS wasn't being applied due to table layout allowing columns to expand. Solution: Implemented `table-layout: fixed` with `<colgroup>` to enforce column widths, added `width: 200px !important` (not just max-width), and used `!important` flags on all truncation rules to override conflicting styles.
- **Column Header Clarity:** Without clear headers, it was difficult to understand what each column represented. Solution: Enhanced headers with proper truncation and clear labeling, matching cell truncation behavior.

### Lessons Learned
- **Viewport Constraints:** Always consider viewport width when designing tables with dynamic data. Use `max-width` constraints and overflow handling to prevent horizontal scrolling.
- **Fixed Table Layout:** For tables with dynamic content, use `table-layout: fixed` with `<colgroup>` to enforce column widths. This prevents columns from expanding beyond defined widths regardless of content length. Essential for truncation to work properly.
- **CSS Specificity and !important:** When dealing with table layouts and truncation, sometimes `!important` flags are necessary to override default browser table rendering behavior. Use `width` (not just `max-width`) with `!important` to truly enforce column constraints.
- **API Response Filtering:** When consuming APIs that return multiple result sets (like Application Insights), always filter out metadata/configuration tables that aren't meant for user display. Check table names and content before rendering.
- **Text Truncation:** For tables with potentially long values, implement truncation with tooltips or modals to maintain readability while preserving access to full data. Use `white-space: nowrap` and `text-overflow: ellipsis` together for proper truncation.
- **User Feedback:** Copy operations should provide immediate feedback (alerts, toasts) so users know the action succeeded.
- **Modal Design:** Modals should display data in multiple formats (table for readability, JSON for technical users) to accommodate different use cases.
- **CSS Organization:** Dedicated CSS classes for specific components (e.g., `insights-fixed-table`, `modal-*`) improve maintainability and consistency. Reusing existing classes (like `bookings-table`) when appropriate maintains visual consistency.
- **Layout Optimization:** Reorganizing page layout (moving status to top, giving data tables full width) can significantly improve usability and make better use of screen space.

### Key Achievements
- **Improved Demo Page UX:** Layout reorganization, container constraints, metadata filtering, fixed table layout, text truncation, and modal popup make the demo page professional and easy to use for live demonstrations.
- **Enhanced Data Accessibility:** Copy functionality and modal viewing make it easy to access and share full data without cluttering the main view.
- **Proper Table Rendering:** Fixed table layout with colgroup ensures columns respect defined widths, preventing layout breaks from long content.
- **Metadata Filtering:** Successfully filtered out Application Insights metadata tables, preventing raw JSON code from appearing in the UI.
- **Reliable Truncation:** Long strings (including complex JSON query statistics) are now properly truncated at 200px with ellipsis, maintaining table layout integrity.
- **Responsive Design:** Page now works well on different screen sizes without horizontal scrolling issues.
- **Professional Appearance:** Clean table styling with proper overflow handling and consistent formatting creates a polished user experience.

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


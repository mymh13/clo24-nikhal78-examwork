# Week 6 – Event-Driven Architecture Refinement & Demo Enhancements

## Brainstorming & Planned Progression

### Core Features (Priority Order)
1. **Ticket activation timer** - dual triggers:
   - Manual start time selection (user landing page interface under "active tickets")
   - Secondary trigger on "activation" (QR code scan when boarding transportation)
2. **Event triggers API functionality** - implement event-driven ticket validation
3. **QR code generation** for ticket scanning

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

During week 6, work focused on simplifying the demo page and removing over-engineered features. The goal was to create a clean, maintainable MVP that focuses on core functionality: event-driven architecture status and booking management.

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

---

## Reflection

### What Went Well
- **Simplification:** Removing the Application Insights integration immediately reduced code complexity and made the page much more maintainable. The file size reduction from 1,130 lines to 280 lines (75% reduction) makes the codebase more sustainable for an MVP.
- **Focus on Core Functionality:** The simplified page now focuses on the two essential features: event-driven architecture status and booking management. This makes the demo clearer and easier to follow.
- **Better Demo Strategy:** Using Azure Portal's Application Insights interface for visualization provides a superior demonstration experience with proper charts and graphs, rather than trying to display JSON query results in a web table.

### Challenges Encountered
- **Over-Engineering:** The Application Insights integration grew too complex, adding over 800 lines of code for KQL query handling, table rendering, modal popups, and data transformation. This made the codebase difficult to maintain and unsustainable for an MVP.
- **Visualization Limitations:** Application Insights queries return JSON strings that are difficult to visualize effectively. Displaying query results as tables feels like reading logs rather than a proper demo visualization, making it unsuitable for live demonstrations.
- **KQL Query Complexity:** The KQL queries required complex joins and data transformation, making them error-prone and difficult to debug. Syntax errors in KQL queries were hard to troubleshoot.

### Lessons Learned
- **MVP Philosophy:** For an MVP, it's important to focus on core functionality and avoid over-engineering. Features that add significant complexity without proportional value should be reconsidered or removed.
- **Right Tool for the Job:** Some features are better suited for specialized tools (like Azure Portal for Application Insights visualization) rather than trying to replicate them in a web application. Using the right tool provides a better user experience.
- **Code Maintainability:** Code complexity should be carefully managed. A feature that adds 800+ lines of code needs to provide significant value to justify its inclusion in an MVP.
- **Demo Strategy:** For live demonstrations, it's often better to use multiple tools side-by-side (e.g., demo page + Azure Portal) rather than trying to integrate everything into a single page. This provides better visualization and a clearer demonstration flow.
- **Simplicity Over Features:** A simple, focused page is often more effective than a complex page with many features. Removing features can improve the overall user experience.

### Key Achievements
- **Simplified Codebase:** Reduced demo page from 1,130 lines to 280 lines (75% reduction), making it much more maintainable and sustainable for an MVP.
- **Focused Functionality:** Page now focuses on core features: event-driven architecture status and booking management, making the demo clearer and easier to follow.
- **Better Demo Strategy:** Established a better approach for demonstrations using Azure Portal's Application Insights interface for visualization, providing superior charts and graphs.
- **Maintainable MVP:** Created a clean, focused demo page that is sustainable and easy to maintain.

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
2. **Ticket Activation:** Implement ticket activation timer with dual triggers (manual and QR code scan).
3. **QR Code Generation:** Generate QR codes for tickets to enable scanning functionality.
4. **Ticket Search Functionality:** Add search and filtering capabilities to the admin booking management page.
5. **Shopping Cart (Bonus F):** Implement shopping cart functionality to allow users to add multiple tickets before payment.
6. **Price Configuration:** Move base price to configuration for runtime adjustment without code deployment.

---


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

During week 5, work focused on completing login functionality for regular users and preparing the foundation for ticket management features. The goal was to establish a complete authentication system that supports both administrators (via Entra ID) and regular users (via email/password), while maintaining GDPR compliance and preventing bot registrations.

---

## Completed Activities

### User Login Functionality
- **Email-Only Login:** Changed login form from "Username or Email" to "Email" only for simplicity and GDPR compliance (reduces personal data collection).
- **Forgot Password Feature:** Added checkbox to toggle password reset form. Dummy implementation returns success message (functionality not yet implemented).
- **Registration Feature:** Added checkbox to toggle registration form. Registration is currently blocked to prevent bot registrations - returns message directing users to contact support. User accounts will be managed by administrators in a later step.
- **Backend Endpoints:** Created `POST /api/auth/forgot-password` (dummy) and `POST /api/auth/register` (blocked) endpoints. Updated `POST /api/auth/login` to accept `email` parameter instead of `username`.
- **UI Styling:** Added CSS for checkboxes and conditional sections (forgot-email-section, register-section) with dark theme styling. Sections appear with subtle background and border when toggled.
- **Status:** Login page UI complete with all requested features. Standard login endpoint ready for actual authentication implementation in next step.

---

## Reflection

### What Went Well
[To be filled in]

### Challenges Encountered
[To be filled in]

### Lessons Learned
[To be filled in]

### Key Achievements
[To be filled in]

### What Could Be Improved
[To be filled in]

---

## Ongoing Work

[To be filled in]

---

## Next Steps

[To be filled in]


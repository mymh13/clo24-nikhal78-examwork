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

During week 5, work focused on completing login functionality for regular users, creating role-based landing pages, and implementing user management for administrators. The goal was to establish a complete authentication and user management system that supports administrators (via Entra ID), inspectors, and regular users (via email/password), while maintaining GDPR compliance and preventing bot registrations.

---

## Completed Activities

### User Login Functionality
- **Email-Only Login:** Changed login form from "Username or Email" to "Email" only for simplicity and GDPR compliance (reduces personal data collection).
- **Forgot Password Feature:** Added checkbox to toggle password reset form. Dummy implementation returns success message (functionality not yet implemented).
- **Registration Feature:** Added checkbox to toggle registration form. Registration is currently blocked to prevent bot registrations - returns message directing users to contact support. User accounts will be managed by administrators.
- **Backend Endpoints:** Created `POST /api/auth/forgot-password` (dummy) and `POST /api/auth/register` (blocked) endpoints. Updated `POST /api/auth/login` to accept `email` parameter instead of `username`.
- **UI Styling:** Added CSS for checkboxes and conditional sections with dark theme styling. Sections appear with subtle background and border when toggled.
- **Status:** Login page UI complete with all requested features. Standard login endpoint ready for actual authentication implementation.

### Role-Based Landing Pages
- **Admin Landing Page:** Updated with expandable sections for ticket management and user management. Links to dedicated pages for bookings and users.
- **User Landing Page:** Created `/user` page with expandable sections for "My Tickets" and "My Information". Restricted to User role with placeholders for future functionality.
- **Inspector Landing Page:** Created `/inspector` page with expandable sections for ticket inspection (view/create, no delete) and user viewing (read-only). Restricted to Inspector role.
- **Navigation:** All landing pages use expandable checkbox pattern to keep interface clean. Role-based routing via `NavigationHelper` utility.
- **Status:** All three role-based landing pages implemented with consistent styling and expandable sections. Ready for functionality implementation.

### User Management System
- **User Service:** Created `IUserService` and `UserService` with CRUD operations for ticketing system users. Password hashing with BCrypt, email uniqueness validation, Cosmos DB storage with email as partition key.
- **Users API:** Created `UsersController` with endpoints for create, read all, read by ID, update, and delete. All endpoints restricted to Admin role. Password hashes excluded from responses. Audit logging included.
- **User Management UI:** Created `/users` page for admin user management. Create user form with email, password, name, and role selection (Admin, Inspector, User). Users table with delete functionality. Styled to match existing theme.
- **Integration:** UserService registered in dependency injection. Admin landing page links to user management when "Manage Users" section is expanded.
- **Status:** Complete user management system operational. Admins can create Inspector and User accounts for testing different role-based access.

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


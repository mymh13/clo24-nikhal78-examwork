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
- **User Management UI:** Created `/users` page for admin user management. Create user form with email, password confirmation, name, and role selection (Admin, Inspector, User). Users table with delete functionality and confirmation dialog. Styled to match existing theme.
- **Email Validation:** Implemented strict email validation requiring top-level domain (TLD) to prevent invalid emails like `user@domain`. Validation applied to both user creation and registration forms with real-time error messages.
- **Password Confirmation:** Added password confirmation field with real-time validation to ensure passwords match before submission. Minimum password length validation (6 characters).
- **Delete Confirmation:** Implemented JavaScript confirmation dialog for user deletion showing user email and warning about irreversible action.
- **Cosmos DB Container:** Added auto-creation logic for `users` container in `ValidateCosmosConnection` method. Container created on app startup if missing, preventing 404 errors on first user creation.
- **UI Improvements:** Removed expandable checkboxes from admin landing page - user management and ticket management sections now always visible with direct links. Fixed focus highlight issue on page load by adding CSS and JavaScript to blur focused elements.
- **Status:** Complete user management system operational. Admins can create Inspector and User accounts with proper validation and confirmation dialogs.

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

### Lessons Learned
- **Container Management:** Cosmos DB containers should be created either via infrastructure (Bicep) or auto-created on first use. Auto-creation provides better developer experience but should be documented.
- **Email Validation:** HTML5 email input type alone is not sufficient - custom validation with regex is needed to enforce TLD requirements and prevent invalid formats.
- **Blazor Bind Events:** When using `@bind` with validation, use `@bind:event` and `@bind:after` instead of `@onchange` to avoid conflicts.
- **User Experience:** Small UI details like focus highlights and confirmation dialogs significantly impact perceived quality and professionalism of the application.

### Key Achievements
- **Complete User Management:** Full CRUD system for users with proper validation, password hashing, and role-based access control.
- **Email Validation:** Strict email format validation prevents invalid data entry across all user creation and registration forms.
- **Password Security:** Password confirmation and length validation ensure users create secure accounts without typos.
- **Improved UX:** Fixed focus issues and streamlined admin landing page for better user experience.
- **Resilient Infrastructure:** Auto-creation of Cosmos DB containers prevents deployment failures and improves system reliability.

### What Could Be Improved
- **User Edit Functionality:** The "Edit" button in user management currently shows a placeholder message. Full edit functionality should be implemented.
- **Password Strength Requirements:** Currently only validates minimum length. Could add complexity requirements (uppercase, numbers, special characters).
- **Email Uniqueness Feedback:** Could provide more immediate feedback when checking if an email already exists (e.g., on blur event).
- **Container Creation in Bicep:** Consider adding `users` container to Cosmos DB Bicep template for infrastructure-as-code approach instead of runtime creation.

---

## Ongoing Work

- **User Authentication:** Standard login endpoint (`POST /api/auth/login`) is ready but not yet implemented. Needs to verify email/password against Cosmos DB users and create authentication cookie.
- **User Edit Functionality:** Edit button in user management UI needs implementation to allow admins to update user details.

---

## Next Steps

1. **Implement Standard Login:** Complete the email/password authentication flow for regular users and inspectors.
2. **User Edit Functionality:** Implement edit form in user management to allow updating user details (name, role, password).
3. **Ticket Attributes:** Begin implementing ticket attributes (regions, zones, age, price) as outlined in brainstorming section.
4. **Ticket Activation:** Implement ticket activation timer with dual triggers (manual and QR code scan).


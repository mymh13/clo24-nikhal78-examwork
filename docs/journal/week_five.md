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
- **Bonus E:** Add unit and/or integration testing

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
- **Standard Login Implementation:** Implemented email/password authentication flow. Login form converted from traditional POST to Blazor form handling. `AuthController.Login` now verifies credentials against Cosmos DB users using BCrypt password verification. Creates authentication cookie with proper claims (NameIdentifier, Email, Name, Role) and redirects to appropriate landing page based on role.
- **Error Message Persistence:** Fixed login error messages disappearing immediately. Converted form to use Blazor `@onsubmit` handler instead of traditional form POST, allowing error messages to persist on page until cleared or successful login.
- **UI Styling:** Added CSS for checkboxes and conditional sections with dark theme styling. Sections appear with subtle background and border when toggled.
- **Status:** Complete login functionality operational. Users and inspectors can authenticate via email/password, admins via Entra ID. Error messages persist correctly.

### Role-Based Landing Pages
- **Admin Landing Page:** Updated with sections for ticket management and user management (no checkboxes, always visible). Links to dedicated pages for bookings and users. Logout button aligned to the right.
- **User Landing Page:** Created `/user` page with sections for "My Information" and "My Tickets" (no checkboxes, always visible). Added ticket creation form in "My Tickets" section. Form auto-fills with user's ID and name. Logout button aligned to the right.
- **Inspector Landing Page:** Created `/inspector` page with sections for user viewing and ticket inspection (no checkboxes, always visible). Restricted to Inspector role. Logout button aligned to the right.
- **Navigation:** All landing pages use consistent layout with always-visible sections. Role-based routing via `NavigationHelper` utility. Logout buttons consistently positioned on the right side of user info sections.
- **Status:** All three role-based landing pages implemented with consistent styling and functional ticket creation for users.

### User Management System
- **User Service:** Created `IUserService` and `UserService` with CRUD operations for ticketing system users. Password hashing with BCrypt, email uniqueness validation, Cosmos DB storage with email as partition key.
- **Users API:** Created `UsersController` with endpoints for create, read all, read by ID, update, and delete. All endpoints restricted to Admin role. Password hashes excluded from responses. Audit logging included.
- **User Management UI:** Created `/users` page for admin user management. Create user form with email, password confirmation, name, and role selection (Admin, Inspector, User). Users table with edit and delete functionality. Styled to match existing theme.
- **User Edit Functionality:** Implemented full edit functionality for user management. Edit form appears when "Edit" button is clicked, pre-populated with user data. Email field disabled (cannot be changed). Name validation (min 2 chars, letters/spaces/hyphens only). Optional password update (leave empty to keep current). Role can be changed via dropdown. Cancel button to close edit form. Success/error messages displayed after update.
- **Name Validation:** Added name validation requiring minimum 2 characters and only allowing letters, spaces, and hyphens (no special characters). Real-time validation with error messages.
- **Email Validation:** Implemented strict email validation requiring top-level domain (TLD) to prevent invalid emails like `user@domain`. Validation applied to both user creation and registration forms with real-time error messages.
- **Password Validation:** Added password confirmation field with real-time validation to ensure passwords match before submission. Minimum password length validation (6 characters, no complexity requirements for MVP).
- **Delete Confirmation:** Implemented JavaScript confirmation dialog for user deletion showing user email and warning about irreversible action.
- **Cosmos DB Container:** Added auto-creation logic for `users` container in `ValidateCosmosConnection` method. Container created on app startup if missing, preventing 404 errors on first user creation.
- **UI Improvements:** Removed expandable checkboxes from all landing pages - sections now always visible with direct links. Fixed focus highlight issue on page load by adding CSS and JavaScript to blur focused elements. Logout buttons aligned consistently to the right.
- **Status:** Complete user management system operational. Admins can create, edit, and delete user accounts with proper validation and confirmation dialogs.

### Ticket Management Enhancements
- **Ticket Viewing for Users:** Added `GET /api/bookings/my-bookings` endpoint (User role only) to retrieve user's own tickets. Updated User landing page to display tickets in a table format with booking ID, customer email, customer name, and booking date. Tickets automatically load on page initialization and refresh after creating new tickets. Shows informative message when no tickets exist.
- **Ticket Creation Security:** Fixed ticket creation to prevent users from modifying their name or email. Name field in ticket creation form is now read-only and displays user's actual name from their account. `CreateBooking` endpoint enforces that users can only create tickets for themselves, always using their account data (ID, email, name) regardless of form input. Admin/Inspector roles can still create tickets for any user.
- **Ticket Deletion for Admins:** Added `DELETE /api/bookings/{bookingId}?customerId={customerId}` endpoint restricted to Admin role. Implemented `DeleteBookingAsync` in `BookingService` for Cosmos DB deletion using partition key. Added "Actions" column to bookings table on `/bookings` page (visible only to Admins). Delete button with JavaScript confirmation dialog. Automatically refreshes bookings list after successful deletion. Enables cleanup of errant tickets with incorrect user IDs.
- **Status:** Complete ticket viewing and management functionality. Users can view their own tickets, and admins can delete tickets with proper confirmation.

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
- **Login Error Message Disappearing:** Error messages from login were disappearing immediately due to traditional form POST causing page redirect. Resolved by converting to Blazor form handling with `@onsubmit` to keep error messages visible.

### Lessons Learned
- **Container Management:** Cosmos DB containers should be created either via infrastructure (Bicep) or auto-created on first use. Auto-creation provides better developer experience but should be documented.
- **Email Validation:** HTML5 email input type alone is not sufficient - custom validation with regex is needed to enforce TLD requirements and prevent invalid formats.
- **Blazor Bind Events:** When using `@bind` with validation, use `@bind:event` and `@bind:after` instead of `@onchange` to avoid conflicts.
- **Form Handling:** Traditional HTML form POST causes page redirects which clear error messages. Blazor form handling with `@onsubmit` and `@onsubmit:preventDefault` allows error messages to persist on the page.
- **User Experience:** Small UI details like focus highlights, error message persistence, and confirmation dialogs significantly impact perceived quality and professionalism of the application.
- **Security in API Endpoints:** When allowing multiple roles to access an endpoint, add role-specific checks within the endpoint to ensure users can only perform actions on their own data (e.g., users can only create tickets for themselves).

### Key Achievements
- **Complete User Management:** Full CRUD system for users with proper validation, password hashing, and role-based access control. Edit functionality fully implemented.
- **Standard Login Implementation:** Email/password authentication working for users and inspectors. BCrypt password verification, proper cookie-based session management, role-based redirects.
- **Ticket Creation for Users:** Users can create tickets from their landing page. Form auto-fills with user information. Security check ensures users can only create tickets for themselves. Name field is read-only to prevent tampering.
- **Ticket Viewing for Users:** Users can now view all their existing tickets in a table format on their landing page. Tickets automatically refresh after creation.
- **Ticket Deletion for Admins:** Admins can delete tickets with confirmation dialogs. Enables cleanup of errant tickets and testing of deletion functionality.
- **Email Validation:** Strict email format validation prevents invalid data entry across all user creation and registration forms.
- **Password Security:** Password confirmation and length validation ensure users create secure accounts without typos.
- **Improved UX:** Fixed focus issues, login error message persistence, and streamlined all landing pages for better user experience. Consistent logout button positioning.
- **Resilient Infrastructure:** Auto-creation of Cosmos DB containers prevents deployment failures and improves system reliability.

### What Could Be Improved
- **Password Strength Requirements:** Currently only validates minimum length. Could add complexity requirements (uppercase, numbers, special characters) for production.
- **Email Uniqueness Feedback:** Could provide more immediate feedback when checking if an email already exists (e.g., on blur event).
- **Container Creation in Bicep:** Consider adding `users` container to Cosmos DB Bicep template for infrastructure-as-code approach instead of runtime creation.
- **Ticket Search Functionality:** Add search and filtering capabilities to the admin booking management page for better ticket discovery.

---

## Ongoing Work

- **Ticket Search:** Add search and filtering functionality to the admin booking management page for better ticket discovery.

---

## Next Steps

1. **Ticket Attributes:** Begin implementing ticket attributes (regions, zones, age, price) as outlined in brainstorming section.
2. **Ticket Activation:** Implement ticket activation timer with dual triggers (manual and QR code scan).
3. **QR Code Generation:** Generate QR codes for tickets to enable scanning functionality.
4. **Ticket Search Functionality:** Add search and filtering capabilities to the admin booking management page.


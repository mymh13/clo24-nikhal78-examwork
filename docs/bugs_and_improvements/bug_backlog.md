Bugs found, added to a backlog to be corrected in the future

1. ~~If we are logged in as a user or inspector, and then manually type the address to go to the ticket.mymh.dev/health page or similar, then move back to the login-screen: Then we land on the Admin Dashboard. We are not allowed to alter Users etc but we can see the entire Admin Dashboard and list all users and bookings.~~ **FIXED**
- ~~Above is a rather critical error, but nothing application breaking or sharing anything sensitive so adding this to the bug_backlog to be reviewed later.~~
- 251203: Fixed by updating `Login.razor` to use `NavigationHelper.GetLandingPageUrl()` instead of hardcoded "/admin" link. Now users are correctly redirected to their role-specific landing page (Admin → /admin, Inspector → /inspector, User → /user).

2. ~~Integration tests created test-users into the real CosmosDB.~~ **FIXED**
- ~~Medium prio error.~~
- 251203: Fixed by implementing in-memory mock services (InMemoryUserService, InMemoryBookingService, InMemoryOutboxService) that use shared singleton storage. Tests now use mocked data instead of real Cosmos DB.

3. Barely a bug but: The zones do not show up in alfabetical order, they show up in the order you ticked them on the tickets. That might be a small UX improvement to fix.
- ~~Low prio error.~~

4. Using UTC timer to have a standard across timezones, but this means the tickets display one hour offset from the local time. Tiny UX improvement but might actually impact the passenger if they read the wrong time. For the MVP this is a tiny error but in production it would be critical.
- ~~Medium prio error.~~
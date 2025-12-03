Bugs found, added to a backlog to be corrected in the future

1. If we are logged in as a user or inspector, and then manually type the address to go to the ticket.mymh.dev/health page or similar, then move back to the login-screen: Then we land on the Admin Dashboard. We are not allowed to alter Users etc but we can see the entire Admin Dashboard and list all users and bookings.
- Above is a rather critical error, but nothing application breaking or sharing anything sensitive so adding this to the bug_backlog to be reviewed later.

2. ~~Integration tests created test-users into the real CosmosDB.~~ **FIXED**
- ~~Medium prio error.~~
- Fixed by implementing in-memory mock services (InMemoryUserService, InMemoryBookingService, InMemoryOutboxService) that use shared singleton storage. Tests now use mocked data instead of real Cosmos DB.
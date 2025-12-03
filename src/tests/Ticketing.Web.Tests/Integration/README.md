# Integration Tests

Integration tests for the Ticketing.Web application that test the full API flow using in-memory mock services.

## Prerequisites

**No external dependencies required!** Tests use in-memory mock implementations of:
- `IUserService` - In-memory user storage
- `IBookingService` - In-memory booking storage  
- `IOutboxService` - In-memory outbox event storage

This ensures tests don't create real users or data in Cosmos DB.

## Running Tests

```bash
# Run all integration tests
dotnet test src/tests/Ticketing.Web.Tests/Ticketing.Web.Tests.csproj --filter "FullyQualifiedName~Integration"

# Run a specific test
dotnet test src/tests/Ticketing.Web.Tests/Ticketing.Web.Tests.csproj --filter "FullyQualifiedName~BookingLifecycleTests"

# Run with verbose output
dotnet test src/tests/Ticketing.Web.Tests/Ticketing.Web.Tests.csproj --filter "FullyQualifiedName~Integration" --logger "console;verbosity=detailed"
```

## Test Structure

### `WebApplicationFactoryFixture`
- Sets up the test web server with test-specific configuration
- Overrides authentication with test authentication handler
- Replaces Cosmos DB services with in-memory mocks (`InMemoryUserService`, `InMemoryBookingService`, `InMemoryOutboxService`)
- Disables external services (Application Insights, Service Bus) for tests

### In-Memory Mock Services
- **`InMemoryUserService`**: Stores users in memory using `ConcurrentDictionary`
- **`InMemoryBookingService`**: Stores bookings in memory using `ConcurrentDictionary`
- **`InMemoryOutboxService`**: Stores outbox events in memory using `ConcurrentDictionary`

These mocks provide the same interface as the real services but don't persist data to Cosmos DB.

### `BookingLifecycleTests`
Tests the complete booking lifecycle:
- **CreateBooking**: Creates a booking and verifies it's stored correctly
- **CreateBooking_CalculatesPriceCorrectly**: Verifies price calculation logic
- **GetBookingById**: Retrieves bookings by customer ID
- **ActivateBooking**: Activates a ticket and verifies status update
- **ActivateBooking_AlreadyActivated**: Verifies activation validation
- **DeleteBooking**: Deletes a booking and verifies removal
- **GetBookingsByCustomer**: Verifies customer-specific filtering

## Test Data

Tests use isolated test data with unique email addresses (`test-{guid}@example.com`) to avoid conflicts between test runs.

## Notes

- **No Cosmos DB required** - Tests use in-memory mocks, so no database setup is needed
- Test authentication bypasses Azure AD and uses a test authentication handler
- External services (Application Insights, Service Bus) are disabled in tests
- Test data is stored in memory and cleared between test runs (no persistence)


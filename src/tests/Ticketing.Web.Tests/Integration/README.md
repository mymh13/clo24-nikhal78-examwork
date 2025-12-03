# Integration Tests

Integration tests for the Ticketing.Web application that test the full API flow including Cosmos DB interactions.

## Prerequisites

### Option 1: Cosmos DB Emulator (Recommended for Local Development)

1. **Install Cosmos DB Emulator:**
   - Download from: https://aka.ms/cosmosdb-emulator
   - Or install via Chocolatey: `choco install azure-cosmosdb-emulator`

2. **Start the Emulator:**
   - The emulator should be running on `https://localhost:8081`
   - Default connection string: `AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==`

### Option 2: Test Cosmos DB Account (For CI/CD)

Set the `TEST_COSMOS_CONNECTION_STRING` environment variable to a test Cosmos DB account connection string:

```bash
# Windows PowerShell
$env:TEST_COSMOS_CONNECTION_STRING="AccountEndpoint=https://your-test-account.documents.azure.com:443/;AccountKey=your-key;"

# Linux/Mac
export TEST_COSMOS_CONNECTION_STRING="AccountEndpoint=https://your-test-account.documents.azure.com:443/;AccountKey=your-key;"
```

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
- Configures Cosmos DB connection (emulator or test account)
- Disables external services (Application Insights, Service Bus) for tests

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

- Tests require a running Cosmos DB instance (emulator or test account)
- Test authentication bypasses Azure AD and uses a test authentication handler
- External services (Application Insights, Service Bus) are disabled in tests
- Tests clean up after themselves, but Cosmos DB data may persist between runs (this is acceptable for integration tests)


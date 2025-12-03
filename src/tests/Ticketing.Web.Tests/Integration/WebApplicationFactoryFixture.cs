using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Ticketing.Web;

namespace Ticketing.Web.Tests.Integration;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestCosmosConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration for testing
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Use Cosmos DB Emulator connection string for local testing
                // For CI/CD, this should be set via environment variable or use a test Cosmos DB account
                { "CosmosDb:ConnectionString", GetCosmosConnectionString() },
                
                // Disable Application Insights for tests (or use a test AI resource)
                { "APPLICATIONINSIGHTS_CONNECTION_STRING", "" },
                
                // Disable Service Bus for tests (or use a test Service Bus namespace)
                { "ServiceBus:NamespaceName", "" },
                
                // Set base price for testing
                { "Pricing:BasePricePerZone", "20.0" },
                
                // Disable Azure AD for tests
                { "AzureAd:ClientId", "" },
                { "AzureAd:TenantId", "" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove any hosted services that might interfere with tests
            var hostedServices = services.Where(s => s.ServiceType.IsAssignableTo(typeof(IHostedService))).ToList();
            foreach (var service in hostedServices)
            {
                services.Remove(service);
            }

            // Override authentication with test authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        });
    }

    private static string GetCosmosConnectionString()
    {
        // Check for environment variable first (for CI/CD)
        var envConnectionString = Environment.GetEnvironmentVariable("TEST_COSMOS_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        // Default to Cosmos DB Emulator (requires emulator to be running)
        return TestCosmosConnectionString;
    }

    public async Task InitializeAsync()
    {
        // Verify Cosmos DB connection is available
        // This will fail if Cosmos DB Emulator is not running
        try
        {
            var client = Services.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var database = client.GetDatabase("ticketing");
            await database.ReadAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Cosmos DB connection failed. Ensure Cosmos DB Emulator is running or set TEST_COSMOS_CONNECTION_STRING environment variable.",
                ex);
        }
    }

    public new Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }
}

/// <summary>
/// Test authentication handler that authenticates all requests with test user claims.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create test user claims
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Email, "test-user@example.com"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}



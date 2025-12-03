using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Ticketing.Web.Services;
using Ticketing.Web.Tests.Integration.Mocks;
using InMemoryStorage = Ticketing.Web.Tests.Integration.Mocks.InMemoryStorage;

namespace Ticketing.Web.Tests.Integration;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration for testing
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Disable Cosmos DB - we'll use in-memory mocks instead
                { "CosmosDb:ConnectionString", "" },
                
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

            // Remove real Cosmos DB services if they were registered
            var cosmosClientDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(Microsoft.Azure.Cosmos.CosmosClient));
            if (cosmosClientDescriptor != null)
            {
                services.Remove(cosmosClientDescriptor);
            }

            var bookingServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IBookingService));
            if (bookingServiceDescriptor != null)
            {
                services.Remove(bookingServiceDescriptor);
            }

            var userServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IUserService));
            if (userServiceDescriptor != null)
            {
                services.Remove(userServiceDescriptor);
            }

            var outboxServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IOutboxService));
            if (outboxServiceDescriptor != null)
            {
                services.Remove(outboxServiceDescriptor);
            }

            // Register shared in-memory storage as singleton
            services.AddSingleton<InMemoryStorage>();

            // Register in-memory mock services (scoped, but they share the singleton storage)
            services.AddScoped<IBookingService, InMemoryBookingService>();
            services.AddScoped<IUserService, InMemoryUserService>();
            services.AddScoped<IOutboxService, InMemoryOutboxService>();

            // Override authentication with test authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        });
    }

    public Task InitializeAsync()
    {
        // No initialization needed - we're using in-memory mocks instead of Cosmos DB
        return Task.CompletedTask;
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
        // Create test user claims with Admin role so tests can create users automatically
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-admin-id"),
            new Claim(ClaimTypes.Email, "test-admin@example.com"),
            new Claim(ClaimTypes.Name, "Test Admin"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}



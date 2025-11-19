using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Cosmos;
using Ticketing.Web.Services;

namespace Ticketing.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Application Insights
        services.AddApplicationInsightsTelemetry();

        // Add Cosmos DB Client
        // Try both formats: Key Vault uses -- which gets converted to : in configuration
        var cosmosConnectionString = configuration["CosmosDb:ConnectionString"] 
            ?? configuration["CosmosDb--ConnectionString"];
        
        if (!string.IsNullOrEmpty(cosmosConnectionString))
        {
            services.AddSingleton<CosmosClient>(sp => new CosmosClient(cosmosConnectionString));
            // Register Booking Service
            services.AddScoped<IBookingService, BookingService>();
            Console.WriteLine("Cosmos DB connection string found - client and booking service will be registered");
        }
        else
        {
            Console.WriteLine("Cosmos DB connection string not found - skipping client registration");
            Console.WriteLine("Checked keys: CosmosDb:ConnectionString, CosmosDb--ConnectionString");
            // Register a null CosmosClient so DI can resolve CosmosClient? in controllers
            services.AddSingleton<CosmosClient?>(sp => null);
        }

        // Add Authentication (minimal - hardcoded admin, same structure as real auth)
        services.AddAuthentication("Hardcoded")
            .AddCookie("Hardcoded", options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
            });

        services.AddAuthorization();

        // Add services to the container
        services.AddHttpContextAccessor();
        services.AddScoped(sp =>
        {
            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var baseUrl = $"{httpContext?.Request.Scheme}://{httpContext?.Request.Host}";
            return new HttpClient { BaseAddress = new Uri(baseUrl) };
        });
        services.AddControllers();
        services.AddRazorPages();
        services.AddServerSideBlazor();

        return services;
    }
}


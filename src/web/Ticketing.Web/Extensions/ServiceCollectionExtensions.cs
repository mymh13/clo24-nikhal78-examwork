using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Cosmos;

namespace Ticketing.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Application Insights
        services.AddApplicationInsightsTelemetry();

        // Add Cosmos DB Client
        var cosmosConnectionString = configuration["CosmosDb:ConnectionString"];
        if (!string.IsNullOrEmpty(cosmosConnectionString))
        {
            services.AddSingleton<CosmosClient>(sp => new CosmosClient(cosmosConnectionString));
            Console.WriteLine("Cosmos DB connection string found - client will be registered");
        }
        else
        {
            Console.WriteLine("Cosmos DB connection string not found - skipping client registration");
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
        services.AddControllers();
        services.AddRazorPages();
        services.AddServerSideBlazor();

        return services;
    }
}


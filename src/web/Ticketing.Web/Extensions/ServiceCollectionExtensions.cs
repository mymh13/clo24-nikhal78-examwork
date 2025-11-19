using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Http;
using Ticketing.Web.Services;

namespace Ticketing.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Application Insights
        services.AddApplicationInsightsTelemetry();

        var cosmosConnectionString = configuration["CosmosDb:ConnectionString"] 
            ?? configuration["CosmosDb--ConnectionString"];
        
        if (!string.IsNullOrEmpty(cosmosConnectionString))
        {
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            services.AddSingleton<CosmosClient>(sp => new CosmosClient(cosmosConnectionString, cosmosClientOptions));
            services.AddScoped<IBookingService, BookingService>();
        }

        services.AddAuthentication("Hardcoded")
            .AddCookie("Hardcoded", options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddHttpClient("BlazorServer", (sp, client) =>
        {
            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var baseUrl = $"{httpContext?.Request.Scheme}://{httpContext?.Request.Host}";
            client.BaseAddress = new Uri(baseUrl);
            
            if (httpContext?.Request.Headers.ContainsKey("Cookie") == true)
            {
                var cookies = httpContext.Request.Headers["Cookie"].ToString();
                client.DefaultRequestHeaders.Add("Cookie", cookies);
            }
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = false
        });
        
        services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return httpClientFactory.CreateClient("BlazorServer");
        });
        services.AddControllers();
        services.AddRazorPages();
        services.AddServerSideBlazor();

        return services;
    }
}


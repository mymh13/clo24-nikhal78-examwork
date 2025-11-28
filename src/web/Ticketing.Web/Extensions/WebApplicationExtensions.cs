using Microsoft.Azure.Cosmos;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;

namespace Ticketing.Web.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;
                    
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = exception?.Message ?? "An error occurred",
                            path = context.Request.Path
                        });
                    }
                    else
                    {
                        context.Response.Redirect("/Error");
                    }
                });
            });
            app.UseHsts();
        }

        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        
        // Trigger Azure App Configuration refresh on each request
        // This middleware checks the sentinel key and refreshes configuration if changed
        app.Use(async (context, next) =>
        {
            var refresherProvider = app.Services.GetService<IConfigurationRefresherProvider>();
            if (refresherProvider != null)
            {
                foreach (var configurationRefresher in refresherProvider.Refreshers)
                {
                    // TryRefreshAsync respects the refresh interval (30 seconds)
                    // It will only actually refresh if the sentinel key changed and interval elapsed
                    // We await it to ensure it completes, but it returns quickly if interval hasn't elapsed
                    try
                    {
                        var refreshed = await configurationRefresher.TryRefreshAsync();
                        var logger = context.RequestServices.GetService<ILogger<Program>>();
                        if (refreshed)
                        {
                            logger?.LogInformation("App Configuration refreshed successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail the request if refresh fails
                        var logger = context.RequestServices.GetService<ILogger<Program>>();
                        logger?.LogWarning(ex, "Failed to refresh App Configuration");
                    }
                }
            }
            await next();
        });
        
        app.UseRouting();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        return app;
    }

    public static WebApplication ValidateCosmosConnection(this WebApplication app)
    {
        var cosmosClient = app.Services.GetService<CosmosClient>();
        if (cosmosClient != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500);
                    var database = cosmosClient.GetDatabase("ticketing");
                    
                    try
                    {
                        var bookingsContainer = database.GetContainer("bookings");
                        await bookingsContainer.ReadContainerAsync();
                    }
                    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        await database.CreateContainerIfNotExistsAsync(
                            new ContainerProperties("bookings", "/customerId"));
                        Console.WriteLine("Created 'bookings' container in Cosmos DB");
                    }
                    
                    try
                    {
                        var usersContainer = database.GetContainer("users");
                        await usersContainer.ReadContainerAsync();
                    }
                    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        await database.CreateContainerIfNotExistsAsync(
                            new ContainerProperties("users", "/email"));
                        Console.WriteLine("Created 'users' container in Cosmos DB");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cosmos DB connection failed: {ex.Message}");
                }
            });
        }

        return app;
    }
}


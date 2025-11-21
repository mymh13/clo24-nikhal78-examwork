using Microsoft.Azure.Cosmos;
using Microsoft.AspNetCore.HttpOverrides;

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
                    var container = database.GetContainer("bookings");
                    await container.ReadContainerAsync();
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


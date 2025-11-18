using Microsoft.Azure.Cosmos;

namespace Ticketing.Web.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
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
            Console.WriteLine("Testing Cosmos DB connection...");
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500);
                    var database = cosmosClient.GetDatabase("ticketing");
                    var container = database.GetContainer("bookings");
                    var containerProperties = await container.ReadContainerAsync();
                    Console.WriteLine($"Cosmos DB connected successfully: Database 'ticketing' Container 'bookings'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cosmos DB connection failed: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"  Inner exception: {ex.InnerException.Message}");
                    }
                }
            });
        }
        else
        {
            Console.WriteLine("Cosmos DB client not registered - skipping connection test");
        }

        return app;
    }
}


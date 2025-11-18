using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Load local development settings if they exist (appsettings.Development.local.json)
// Load regardless of environment for local testing
builder.Configuration.AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: true);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add Cosmos DB Client (minimal - connection string from configuration)
var cosmosConnectionString = builder.Configuration["CosmosDb:ConnectionString"];
if (!string.IsNullOrEmpty(cosmosConnectionString))
{
    builder.Services.AddSingleton<CosmosClient>(sp => new CosmosClient(cosmosConnectionString));
    Console.WriteLine("Cosmos DB connection string found - client will be registered");
}
else
{
    Console.WriteLine("Cosmos DB connection string not found - skipping client registration");
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Test Cosmos DB connection on startup (minimal validation)
var cosmosClient = app.Services.GetService<CosmosClient>();
if (cosmosClient != null)
{
    Console.WriteLine("Testing Cosmos DB connection...");
    _ = Task.Run(async () =>
    {
        try
        {
            await Task.Delay(500); // Small delay to ensure app is fully started
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

app.Run();
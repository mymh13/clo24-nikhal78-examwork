using Ticketing.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load local configuration (appsettings.Development.local.json)
builder.Configuration.AddLocalConfiguration(builder.Environment);

// Load Key Vault configuration (if configured)
builder.Configuration.AddKeyVaultConfiguration(builder.Environment);

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure pipeline
app.ConfigurePipeline();

// Validate Cosmos DB connection
app.ValidateCosmosConnection();

app.Run();
using Ticketing.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddLocalConfiguration(builder.Environment);
builder.Configuration.AddKeyVaultConfiguration(builder.Environment);
builder.Configuration.AddAppConfiguration(builder.Environment);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.ConfigurePipeline();
app.ValidateCosmosConnection();

app.Run();
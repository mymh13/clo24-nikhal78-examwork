using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Ticketing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly CosmosClient? _cosmosClient;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IConfiguration configuration,
        CosmosClient? cosmosClient,
        ILogger<HealthController> logger)
    {
        _configuration = configuration;
        _cosmosClient = cosmosClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<HealthStatus>> GetHealth()
    {
        var health = new HealthStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Configuration = new ConfigurationStatus
            {
                KeyVaultName = _configuration["KeyVault:Name"] ?? "Not configured",
                KeyVaultConfigured = !string.IsNullOrEmpty(_configuration["KeyVault:Name"]),
                CosmosDbClientRegistered = _cosmosClient != null,
                ApplicationInsightsConfigured = !string.IsNullOrEmpty(_configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"])
            }
        };

        // Test Cosmos DB connection if client is available
        if (_cosmosClient != null)
        {
            try
            {
                var database = _cosmosClient.GetDatabase("ticketing");
                var container = database.GetContainer("bookings");
                await container.ReadContainerAsync();
                health.Configuration.CosmosDbConnectionStatus = "Connected";
            }
            catch (Exception ex)
            {
                health.Configuration.CosmosDbConnectionStatus = $"Error: {ex.Message}";
                health.Status = "Degraded";
            }
        }
        else
        {
            health.Configuration.CosmosDbConnectionStatus = "Client not registered";
            health.Status = "Degraded";
        }

        return Ok(health);
    }
}

public class HealthStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ConfigurationStatus Configuration { get; set; } = new();
}

public class ConfigurationStatus
{
    public string KeyVaultName { get; set; } = string.Empty;
    public bool KeyVaultConfigured { get; set; }
    public bool CosmosDbClientRegistered { get; set; }
    public string CosmosDbConnectionStatus { get; set; } = "Not tested";
    public bool ApplicationInsightsConfigured { get; set; }
}


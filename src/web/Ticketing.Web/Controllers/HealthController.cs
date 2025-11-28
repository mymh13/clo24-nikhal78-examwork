using System.Linq;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using Ticketing.Web.Services;

namespace Ticketing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly CosmosClient? _cosmosClient;
    private readonly IFeatureManager? _featureManager;
    private readonly IOutboxService? _outboxService;
    private readonly ServiceBusClient? _serviceBusClient;
    private readonly IEventPublisher? _eventPublisher;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<HealthController> logger)
    {
        _configuration = configuration;
        _cosmosClient = serviceProvider.GetService<CosmosClient>();
        _featureManager = serviceProvider.GetService<IFeatureManager>();
        _outboxService = serviceProvider.GetService<IOutboxService>();
        _serviceBusClient = serviceProvider.GetService<ServiceBusClient>();
        _eventPublisher = serviceProvider.GetService<IEventPublisher>();
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<HealthStatus>> GetHealth()
    {
        try
        {
            var keyVaultName = _configuration["KeyVault:Name"] ?? _configuration["KeyVault__Name"] ?? "Not configured";
            
            var appConfigEndpoint = _configuration["AppConfiguration--Endpoint"] 
                ?? _configuration["AppConfiguration:Endpoint"]
                ?? "Not configured";
            
            var appConfigName = _configuration["AppConfiguration--Name"]
                ?? _configuration["AppConfiguration:Name"]
                ?? "Not configured";
            
            var sentinelValue = _configuration["Settings:Sentinel"] ?? "Not found";
            
            var health = new HealthStatus
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Configuration = new ConfigurationStatus
                {
                    KeyVaultName = keyVaultName,
                    KeyVaultConfigured = !string.IsNullOrEmpty(keyVaultName) && keyVaultName != "Not configured",
                    CosmosDbClientRegistered = _cosmosClient != null,
                    ApplicationInsightsConfigured = !string.IsNullOrEmpty(_configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]),
                    AppConfigurationEndpoint = appConfigEndpoint,
                    AppConfigurationName = appConfigName,
                    AppConfigurationConfigured = !string.IsNullOrEmpty(appConfigEndpoint) && appConfigEndpoint != "Not configured",
                    SentinelValue = sentinelValue,
                    FeatureManagerAvailable = _featureManager != null,
                    OutboxServiceRegistered = _outboxService != null,
                    ServiceBusClientRegistered = _serviceBusClient != null,
                    EventPublisherRegistered = _eventPublisher != null
                }
            };
            
            // Test feature flag access if available
            if (_featureManager != null)
            {
                try
                {
                    var testFlag = await _featureManager.IsEnabledAsync("BookingEvents_Enabled");
                    health.Configuration.FeatureFlagValue = testFlag;
                    health.Configuration.FeatureFlagTest = $"BookingEvents_Enabled = {testFlag}";
                    
                    // Also check raw configuration value for debugging
                    var rawConfigValue = _configuration[".appconfig.featureflag~BookingEvents_Enabled"];
                    if (!string.IsNullOrEmpty(rawConfigValue))
                    {
                        health.Configuration.FeatureFlagTest += $" (Raw: {rawConfigValue})";
                    }
                }
                catch (Exception ex)
                {
                    health.Configuration.FeatureFlagTest = $"Error: {ex.Message}";
                    _logger.LogError(ex, "Error checking feature flag");
                }
            }
            
            // Test outbox service if available
            if (_outboxService != null)
            {
                try
                {
                    var pendingEvents = await _outboxService.GetPendingEventsAsync();
                    health.Configuration.OutboxPendingEventsCount = pendingEvents.Count();
                    health.Configuration.OutboxServiceStatus = "Operational";
                }
                catch (Exception ex)
                {
                    health.Configuration.OutboxServiceStatus = $"Error: {ex.Message}";
                    health.Status = "Degraded";
                }
            }

            // Test Service Bus client if available
            if (_serviceBusClient != null)
            {
                try
                {
                    var queueName = _configuration["ServiceBus:QueueName"] 
                        ?? _configuration["ServiceBus--QueueName"] 
                        ?? "booking-events";
                    
                    await using var sender = _serviceBusClient.CreateSender(queueName);
                    health.Configuration.ServiceBusStatus = "Operational";
                    health.Configuration.ServiceBusQueueName = queueName;
                }
                catch (Exception ex)
                {
                    health.Configuration.ServiceBusStatus = $"Error: {ex.Message}";
                    health.Status = "Degraded";
                }
            }
            else
            {
                health.Configuration.ServiceBusStatus = "Client not registered";
            }

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in health check endpoint");
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
        }
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
    public string AppConfigurationEndpoint { get; set; } = string.Empty;
    public string AppConfigurationName { get; set; } = string.Empty;
    public bool AppConfigurationConfigured { get; set; }
    public string SentinelValue { get; set; } = string.Empty;
    public bool FeatureManagerAvailable { get; set; }
    public bool? FeatureFlagValue { get; set; }
    public string FeatureFlagTest { get; set; } = "Not tested";
    public bool OutboxServiceRegistered { get; set; }
    public string OutboxServiceStatus { get; set; } = "Not tested";
    public int OutboxPendingEventsCount { get; set; }
    public bool ServiceBusClientRegistered { get; set; }
    public bool EventPublisherRegistered { get; set; }
    public string ServiceBusStatus { get; set; } = "Not tested";
    public string ServiceBusQueueName { get; set; } = string.Empty;
}


using Azure.Data.AppConfiguration;
using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using System.Text.Json;

namespace Ticketing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class FeatureFlagController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IFeatureManager? _featureManager;
    private readonly ILogger<FeatureFlagController> _logger;

    public FeatureFlagController(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<FeatureFlagController> logger)
    {
        _configuration = configuration;
        _featureManager = serviceProvider.GetService<IFeatureManager>();
        _logger = logger;
    }

    [HttpGet("mini-health")]
    public async Task<ActionResult<MiniHealthStatus>> GetMiniHealth()
    {
        try
        {
            var appConfigName = _configuration["AppConfiguration--Name"]
                ?? _configuration["AppConfiguration:Name"]
                ?? "Not configured";
            
            var sentinelValue = _configuration["Settings:Sentinel"] ?? "Not found";
            
            var miniHealth = new MiniHealthStatus
            {
                AppConfigurationName = appConfigName,
                AppConfigurationConfigured = !string.IsNullOrEmpty(appConfigName) && appConfigName != "Not configured",
                SentinelValue = sentinelValue,
                FeatureManagerAvailable = _featureManager != null
            };

            if (_featureManager != null)
            {
                try
                {
                    var isEnabled = await _featureManager.IsEnabledAsync("BookingEvents_Enabled");
                    miniHealth.FeatureFlagValue = isEnabled;
                    miniHealth.FeatureFlagDisplay = $"BookingEvents_Enabled = {isEnabled}";
                }
                catch (Exception ex)
                {
                    miniHealth.FeatureFlagDisplay = $"Error: {ex.Message}";
                    _logger.LogError(ex, "Error checking feature flag");
                }
            }

            var outboxService = HttpContext.RequestServices.GetService<Services.IOutboxService>();
            if (outboxService != null)
            {
                try
                {
                    var pendingEvents = await outboxService.GetPendingEventsAsync();
                    miniHealth.OutboxPendingEventsCount = pendingEvents.Count();
                    miniHealth.OutboxServiceStatus = "Operational";
                }
                catch (Exception ex)
                {
                    miniHealth.OutboxServiceStatus = $"Error: {ex.Message}";
                    _logger.LogError(ex, "Error checking outbox service");
                }
            }

            return Ok(miniHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mini health check endpoint");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("toggle")]
    public async Task<ActionResult<ToggleResult>> ToggleFeatureFlag()
    {
        try
        {
            var appConfigEndpoint = _configuration["AppConfiguration--Endpoint"] 
                ?? _configuration["AppConfiguration:Endpoint"];
            
            if (string.IsNullOrEmpty(appConfigEndpoint) || appConfigEndpoint == "Not configured")
            {
                return BadRequest(new { error = "App Configuration endpoint not configured" });
            }

            var currentValue = _featureManager != null 
                ? await _featureManager.IsEnabledAsync("BookingEvents_Enabled")
                : false;

            var newValue = !currentValue;

            var client = new ConfigurationClient(
                new Uri(appConfigEndpoint),
                new DefaultAzureCredential());

            var featureFlagKey = ".appconfig.featureflag/BookingEvents_Enabled";
            
            var featureFlag = await client.GetConfigurationSettingAsync(featureFlagKey);
            if (featureFlag.Value == null)
            {
                return NotFound(new { error = "Feature flag not found" });
            }

            var featureFlagContent = JsonSerializer.Deserialize<FeatureFlagContent>(
                featureFlag.Value.Value);

            if (featureFlagContent == null)
            {
                return BadRequest(new { error = "Invalid feature flag format" });
            }

            featureFlagContent.enabled = newValue;

            var updatedFeatureFlag = new ConfigurationSetting(
                featureFlagKey,
                JsonSerializer.Serialize(featureFlagContent))
            {
                ContentType = "application/vnd.microsoft.appconfig.featureflag+json;charset=utf-8"
            };

            await client.SetConfigurationSettingAsync(updatedFeatureFlag);

            var sentinelValue = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            await client.SetConfigurationSettingAsync("Settings:Sentinel", sentinelValue);

            _logger.LogInformation(
                "Feature flag toggled from {OldValue} to {NewValue} by {User}",
                currentValue,
                newValue,
                User.Identity?.Name);

            return Ok(new ToggleResult
            {
                Success = true,
                PreviousValue = currentValue,
                NewValue = newValue,
                SentinelValue = sentinelValue,
                Message = $"Feature flag toggled from {currentValue} to {newValue}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling feature flag");
            return StatusCode(500, new ToggleResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }
}

public class MiniHealthStatus
{
    public string AppConfigurationName { get; set; } = string.Empty;
    public bool AppConfigurationConfigured { get; set; }
    public string SentinelValue { get; set; } = string.Empty;
    public bool FeatureManagerAvailable { get; set; }
    public bool? FeatureFlagValue { get; set; }
    public string FeatureFlagDisplay { get; set; } = "Not tested";
    public string OutboxServiceStatus { get; set; } = "Not tested";
    public int OutboxPendingEventsCount { get; set; }
}

public class ToggleResult
{
    public bool Success { get; set; }
    public bool? PreviousValue { get; set; }
    public bool? NewValue { get; set; }
    public string? SentinelValue { get; set; }
    public string Message { get; set; } = string.Empty;
}

internal class FeatureFlagContent
{
    public string id { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public bool enabled { get; set; }
    public FeatureFlagConditions? conditions { get; set; }
}

internal class FeatureFlagConditions
{
    public FeatureFlagClientFilters? client_filters { get; set; }
}

internal class FeatureFlagClientFilters
{
    public string name { get; set; } = string.Empty;
    public Dictionary<string, object>? parameters { get; set; }
}


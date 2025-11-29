using Azure;
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

            var client = new ConfigurationClient(
                new Uri(appConfigEndpoint),
                new DefaultAzureCredential());

            var featureFlagKey = ".appconfig.featureflag/BookingEvents_Enabled";
            string? sentinelValue = null;
            
            // Retry logic with exponential backoff for rate limiting and transient errors
            const int maxRetries = 3;
            var retryDelay = TimeSpan.FromMilliseconds(500);
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    if (attempt > 0)
                    {
                        await Task.Delay(retryDelay);
                        retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2); // Exponential backoff
                        _logger.LogInformation("Retrying feature flag toggle, attempt {Attempt}", attempt + 1);
                    }

                    // Get current feature flag with ETag for optimistic concurrency
                    // Read directly from App Configuration (not cached) to get the actual current value
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

                    // Get current value from App Configuration (source of truth, not cached)
                    var currentValue = featureFlagContent.enabled;
                    var newValue = !currentValue;

                    _logger.LogInformation(
                        "Toggle request: Current value from App Config = {CurrentValue}, Target value = {NewValue}",
                        currentValue,
                        newValue);

                    // Always attempt the toggle - don't check if already at target due to propagation delays
                    // The ETag will handle any conflicts if the value changed between read and write
                    featureFlagContent.enabled = newValue;

                    // Update with ETag for optimistic concurrency control
                    var updatedFeatureFlag = new ConfigurationSetting(
                        featureFlagKey,
                        JsonSerializer.Serialize(featureFlagContent))
                    {
                        ContentType = "application/vnd.microsoft.appconfig.featureflag+json;charset=utf-8"
                    };

                    // Use MatchConditions with ETag for optimistic concurrency
                    await client.SetConfigurationSettingAsync(
                        updatedFeatureFlag, 
                        onlyIfUnchanged: true);

                    // Update sentinel key to trigger hot-reload
                    sentinelValue = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    await client.SetConfigurationSettingAsync("Settings:Sentinel", sentinelValue);

                    // Verify the change was applied
                    await Task.Delay(500); // Brief delay for propagation
                    var verifyFlag = await client.GetConfigurationSettingAsync(featureFlagKey);
                    var verifyContent = JsonSerializer.Deserialize<FeatureFlagContent>(verifyFlag.Value.Value);
                    
                    if (verifyContent?.enabled != newValue)
                    {
                        _logger.LogWarning("Feature flag toggle verification failed. Expected {Expected}, got {Actual}", 
                            newValue, verifyContent?.enabled);
                        // Continue anyway - might be propagation delay
                    }

                    _logger.LogInformation(
                        "Feature flag successfully toggled from {OldValue} to {NewValue} by {User}. Sentinel updated to {SentinelValue}",
                        currentValue,
                        newValue,
                        User.Identity?.Name,
                        sentinelValue);

                    return Ok(new ToggleResult
                    {
                        Success = true,
                        PreviousValue = currentValue,
                        NewValue = newValue,
                        SentinelValue = sentinelValue,
                        Message = $"Feature flag toggled from {currentValue} to {newValue}"
                    });
                }
                catch (RequestFailedException ex) when (attempt < maxRetries - 1 && (ex.Status == 403 || ex.Status == 409 || ex.Status == 429))
                {
                    // 403: Permission/rate limit, 409: ETag conflict, 429: Too many requests
                    _logger.LogWarning(ex, 
                        "Transient error toggling feature flag (attempt {Attempt}/{MaxRetries}): {Status} - {Message}", 
                        attempt + 1, maxRetries, ex.Status, ex.Message);
                    
                    if (ex.Status == 409)
                    {
                        // ETag conflict - feature flag changed between read and write
                        // This is expected if user clicked multiple times quickly
                        // Re-read the current value and toggle from there
                        _logger.LogInformation("ETag conflict detected - feature flag was modified. Re-reading current value and retrying toggle.");
                        await Task.Delay(500); // Brief delay before retry to allow propagation
                        continue; // Will re-read in next iteration
                    }
                    
                    if (ex.Status == 429)
                    {
                        // Rate limited - wait longer before retry
                        retryDelay = TimeSpan.FromSeconds(2);
                        continue;
                    }
                    
                    // 403 might be transient (token refresh, propagation delay)
                    if (ex.Status == 403 && attempt < maxRetries - 1)
                    {
                        await Task.Delay(1000); // Wait a bit for token refresh/propagation
                        continue;
                    }
                    
                    // Last attempt or non-retryable error
                    throw;
                }
            }
            
            // Should not reach here, but just in case
            throw new InvalidOperationException("Failed to toggle feature flag after all retries");
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure service error toggling feature flag. Status: {Status}, ErrorCode: {ErrorCode}", ex.Status, ex.ErrorCode);
            
            if (ex.Status == 403)
            {
                return StatusCode(403, new ToggleResult
                {
                    Success = false,
                    Message = $"Access denied (403 Forbidden). This may be due to rate limiting or token refresh. Please wait a moment and try again."
                });
            }
            
            return StatusCode(ex.Status, new ToggleResult
            {
                Success = false,
                Message = $"Azure service error: {ex.Message} (Status: {ex.Status})"
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


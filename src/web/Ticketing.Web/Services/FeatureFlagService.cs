using Microsoft.FeatureManagement;

namespace Ticketing.Web.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<FeatureFlagService> _logger;
    
    private const string BookingEventsFeatureFlag = "BookingEvents_Enabled";

    public FeatureFlagService(IFeatureManager featureManager, ILogger<FeatureFlagService> logger)
    {
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsBookingEventsEnabledAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var isEnabled = await _featureManager.IsEnabledAsync(BookingEventsFeatureFlag, cancellationToken);
            _logger.LogDebug("Feature flag {FeatureFlag} is {Status}", BookingEventsFeatureFlag, isEnabled ? "enabled" : "disabled");
            return isEnabled;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check feature flag {FeatureFlag}, defaulting to disabled (synchronous mode)", BookingEventsFeatureFlag);
            return false;
        }
    }
}


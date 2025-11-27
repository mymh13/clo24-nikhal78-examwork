namespace Ticketing.Web.Services;

public interface IFeatureFlagService
{
    Task<bool> IsBookingEventsEnabledAsync(CancellationToken cancellationToken = default);
}


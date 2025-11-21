using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;

namespace Ticketing.Web.Authentication;

public class TicketStore : ITicketStore
{
    private readonly IDistributedCache _cache;
    private readonly IDataProtector _dataProtector;
    private const string KeyPrefix = "AuthTicket:";

    public TicketStore(IDistributedCache cache, IDataProtectionProvider dataProtectionProvider)
    {
        _cache = cache;
        _dataProtector = dataProtectionProvider.CreateProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            CookieAuthenticationDefaults.AuthenticationScheme,
            "v2");
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = $"{KeyPrefix}{Guid.NewGuid()}";
        var expiresUtc = ticket.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddMinutes(30);
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiresUtc
        };

        var ticketData = ProtectTicket(ticket);
        await _cache.SetAsync(key, ticketData, options);

        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var expiresUtc = ticket.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddMinutes(30);
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiresUtc
        };

        var ticketData = ProtectTicket(ticket);
        await _cache.SetAsync(key, ticketData, options);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var ticketData = await _cache.GetAsync(key);
        if (ticketData == null || ticketData.Length == 0)
        {
            return null;
        }

        return UnprotectTicket(ticketData);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    private byte[] ProtectTicket(AuthenticationTicket ticket)
    {
        var ticketSerializer = new TicketSerializer();
        var ticketBytes = ticketSerializer.Serialize(ticket);
        return _dataProtector.Protect(ticketBytes);
    }

    private AuthenticationTicket? UnprotectTicket(byte[] protectedData)
    {
        try
        {
            var ticketBytes = _dataProtector.Unprotect(protectedData);
            var ticketSerializer = new TicketSerializer();
            return ticketSerializer.Deserialize(ticketBytes);
        }
        catch
        {
            return null;
        }
    }
}


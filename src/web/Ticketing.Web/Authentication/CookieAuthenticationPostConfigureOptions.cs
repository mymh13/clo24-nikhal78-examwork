using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Ticketing.Web.Authentication;

public class CookieAuthenticationPostConfigureOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly ITicketStore _ticketStore;

    public CookieAuthenticationPostConfigureOptions(ITicketStore ticketStore)
    {
        _ticketStore = ticketStore;
    }

    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        if (name == CookieAuthenticationDefaults.AuthenticationScheme)
        {
            options.SessionStore = _ticketStore;
        }
    }
}


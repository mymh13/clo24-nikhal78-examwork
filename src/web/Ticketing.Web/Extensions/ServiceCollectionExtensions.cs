using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Ticketing.Web.Services;

namespace Ticketing.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Application Insights
        services.AddApplicationInsightsTelemetry();

        var cosmosConnectionString = configuration["CosmosDb:ConnectionString"] 
            ?? configuration["CosmosDb--ConnectionString"];
        
        if (!string.IsNullOrEmpty(cosmosConnectionString))
        {
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            services.AddSingleton<CosmosClient>(sp => new CosmosClient(cosmosConnectionString, cosmosClientOptions));
            services.AddScoped<IBookingService, BookingService>();
        }

        var azureAdClientId = configuration["AzureAd:ClientId"];
        var azureAdTenantId = configuration["AzureAd:TenantId"];
        var azureAdInstance = configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
        var azureAdCallbackPath = configuration["AzureAd:CallbackPath"] ?? "/signin-oidc";

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                if (!string.IsNullOrEmpty(azureAdClientId) && !string.IsNullOrEmpty(azureAdTenantId))
                {
                    options.Authority = $"{azureAdInstance}{azureAdTenantId}/v2.0";
                    options.ClientId = azureAdClientId;
                    options.CallbackPath = azureAdCallbackPath;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.ResponseMode = OpenIdConnectResponseMode.Query;
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"{azureAdInstance}{azureAdTenantId}/v2.0",
                        ValidateAudience = true,
                        ValidAudience = azureAdClientId,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    options.SignedOutCallbackPath = "/signout-callback-oidc";
                    options.SignedOutRedirectUri = "/login?logout=success";

                    options.Events.OnTokenValidated = context =>
                    {
                        var claimsIdentity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                        if (claimsIdentity != null)
                        {
                            var email = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                            var name = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value 
                                ?? context.Principal?.FindFirst("name")?.Value;
                            
                            if (!string.IsNullOrEmpty(email))
                            {
                                claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email));
                            }
                            if (!string.IsNullOrEmpty(name))
                            {
                                claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, name));
                            }

                            var roles = context.Principal?.FindAll("roles")?.Select(c => c.Value) ?? Enumerable.Empty<string>();
                            foreach (var role in roles)
                            {
                                claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
                            }

                            if (!roles.Any())
                            {
                                claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"));
                            }
                        }
                        return Task.CompletedTask;
                    };
                }
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddHttpClient("BlazorServer", (sp, client) =>
        {
            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var baseUrl = $"{httpContext?.Request.Scheme}://{httpContext?.Request.Host}";
            client.BaseAddress = new Uri(baseUrl);
            
            if (httpContext?.Request.Headers.ContainsKey("Cookie") == true)
            {
                var cookies = httpContext.Request.Headers["Cookie"].ToString();
                client.DefaultRequestHeaders.Add("Cookie", cookies);
            }
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = false
        });
        
        services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return httpClientFactory.CreateClient("BlazorServer");
        });
        services.AddControllers();
        services.AddRazorPages();
        services.AddServerSideBlazor();

        return services;
    }
}


namespace Ticketing.Web.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddLocalConfiguration(this IConfigurationBuilder builder, IWebHostEnvironment environment)
    {
        // Load local development settings if they exist (appsettings.Development.local.json)
        // Load regardless of environment for local testing
        builder.AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: true);
        
        return builder;
    }
}


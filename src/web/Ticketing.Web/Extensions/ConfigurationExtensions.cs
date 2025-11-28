using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;

namespace Ticketing.Web.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddLocalConfiguration(this IConfigurationBuilder builder, IWebHostEnvironment environment)
    {
        builder.AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: true);
        
        return builder;
    }

    public static IConfigurationBuilder AddKeyVaultConfiguration(this IConfigurationBuilder builder, IWebHostEnvironment environment)
    {
        var tempConfig = builder.Build();
        var keyVaultName = tempConfig["KeyVault:Name"] ?? tempConfig["KeyVault__Name"];
        
        if (!string.IsNullOrEmpty(keyVaultName))
        {
            var keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";
            
            try
            {
                builder.AddAzureKeyVault(
                    new Uri(keyVaultUri),
                    new DefaultAzureCredential(),
                    new KeyVaultSecretManager());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load Key Vault configuration: {ex.Message}");
            }
        }
        
        return builder;
    }

    // Static variable to store the refresher for middleware access
    private static IConfigurationRefresher? _configurationRefresher;
    
    public static IConfigurationRefresher? GetConfigurationRefresher() => _configurationRefresher;

    public static IConfigurationBuilder AddAppConfiguration(this IConfigurationBuilder builder, IWebHostEnvironment environment)
    {
        // Build temporary config to get App Configuration endpoint
        var tempConfig = builder.Build();
        
        // Try to get App Configuration endpoint from Key Vault (deployed) or appsettings.json
        var appConfigEndpoint = tempConfig["AppConfiguration--Endpoint"] 
            ?? tempConfig["AppConfiguration:Endpoint"]
            ?? tempConfig["AppConfiguration__Endpoint"];
        
        if (string.IsNullOrEmpty(appConfigEndpoint))
        {
            // Fallback: try to construct from App Configuration name
            var appConfigName = tempConfig["AppConfiguration--Name"]
                ?? tempConfig["AppConfiguration:Name"]
                ?? tempConfig["AppConfiguration__Name"];
            
            if (!string.IsNullOrEmpty(appConfigName))
            {
                appConfigEndpoint = $"https://{appConfigName}.azconfig.io";
            }
        }
        
        if (!string.IsNullOrEmpty(appConfigEndpoint))
        {
            try
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
                        .Select(KeyFilter.Any, LabelFilter.Null)
                        .Select(KeyFilter.Any, environment.EnvironmentName)
                        .UseFeatureFlags()
                        .ConfigureRefresh(refresh =>
                        {
                            // Sentinel key pattern for hot-reload
                            refresh.Register("Settings:Sentinel", refreshAll: true)
                                .SetRefreshInterval(TimeSpan.FromSeconds(30)); // Reduced to 30 seconds for faster testing
                        });
                    
                    // Get the refresher from the options and store it
                    _configurationRefresher = options.GetRefresher();
                }, optional: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load App Configuration: {ex.Message}");
                Console.WriteLine("App Configuration will fall back to appsettings.json values.");
            }
        }
        else
        {
            Console.WriteLine("Info: App Configuration endpoint not found. Using appsettings.json for feature flags.");
        }
        
        return builder;
    }
}


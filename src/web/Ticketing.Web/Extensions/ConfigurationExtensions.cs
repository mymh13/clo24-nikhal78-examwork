using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

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

    public static IConfigurationBuilder AddKeyVaultConfiguration(this IConfigurationBuilder builder, IWebHostEnvironment environment)
    {
        var keyVaultName = builder.Build()["KeyVault:Name"];
        
        if (!string.IsNullOrEmpty(keyVaultName))
        {
            var keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";
            
            try
            {
                // Use DefaultAzureCredential which supports:
                // - Managed Identity (when running on Azure)
                // - Azure CLI (when running locally with 'az login')
                // - Visual Studio / VS Code authentication
                builder.AddAzureKeyVault(
                    new Uri(keyVaultUri),
                    new DefaultAzureCredential(),
                    new KeyVaultSecretManager());
                
                Console.WriteLine($"Key Vault configuration loaded from: {keyVaultUri}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load Key Vault configuration: {ex.Message}");
                Console.WriteLine("Falling back to local configuration only.");
            }
        }
        else
        {
            Console.WriteLine("Key Vault name not configured - skipping Key Vault configuration");
        }
        
        return builder;
    }
}


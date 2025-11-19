using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

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
        // Try both KeyVault:Name and KeyVault__Name (Azure app settings use __)
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
                
                Console.WriteLine($"Key Vault configuration loaded from: {keyVaultUri}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load Key Vault configuration: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine("Falling back to local configuration only.");
            }
        }
        else
        {
            Console.WriteLine("Key Vault name not configured - skipping Key Vault configuration");
            Console.WriteLine("Checked keys: KeyVault:Name, KeyVault__Name");
        }
        
        return builder;
    }
}


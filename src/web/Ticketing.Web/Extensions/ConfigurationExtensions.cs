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
}


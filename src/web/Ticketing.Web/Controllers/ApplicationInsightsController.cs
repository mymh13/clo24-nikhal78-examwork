using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ticketing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ApplicationInsightsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApplicationInsightsController> _logger;
    private readonly HttpClient _httpClient;

    public ApplicationInsightsController(
        IConfiguration configuration,
        ILogger<ApplicationInsightsController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpPost("query")]
    public async Task<ActionResult<QueryResult>> ExecuteQuery([FromBody] QueryRequest request)
    {
        try
        {
            // Get subscription ID from environment or configuration
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID")
                ?? _configuration["AZURE_SUBSCRIPTION_ID"] 
                ?? _configuration["Azure:SubscriptionId"];
            
            // If still not found, try to get from Azure metadata service (works in Azure)
            if (string.IsNullOrEmpty(subscriptionId))
            {
                try
                {
                    var metadataClient = new HttpClient();
                    metadataClient.DefaultRequestHeaders.Add("Metadata", "true");
                    var metadataResponse = await metadataClient.GetStringAsync(
                        "http://169.254.169.254/metadata/instance/compute/subscriptionId?api-version=2021-02-01&format=text");
                    subscriptionId = metadataResponse.Trim();
                }
                catch
                {
                    // Metadata service not available (local dev)
                }
            }

            var resourceGroup = Environment.GetEnvironmentVariable("AZURE_RESOURCE_GROUP")
                ?? _configuration["AZURE_RESOURCE_GROUP"] 
                ?? _configuration["Azure:ResourceGroup"] 
                ?? "rg-examwork-dev";
            var appInsightsName = _configuration["APPLICATIONINSIGHTS_NAME"] 
                ?? _configuration["ApplicationInsights:Name"] 
                ?? "examwork-insights-dev";

            if (string.IsNullOrEmpty(subscriptionId))
            {
                return BadRequest(new QueryResult 
                { 
                    Success = false, 
                    Error = "Azure subscription ID not configured. Set AZURE_SUBSCRIPTION_ID environment variable or configuration." 
                });
            }

            var credential = new DefaultAzureCredential();
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { "https://management.azure.com/.default" }));

            var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Insights/components/{appInsightsName}/api/query?api-version=2021-05-01";

            var queryRequest = new
            {
                query = request.Query,
                timespan = request.Timespan ?? "PT1H"
            };

            var json = JsonSerializer.Serialize(queryRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
            // Note: Content-Type is automatically set by StringContent, don't add it to request headers

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<QueryResult>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null)
                {
                    result.Success = true;
                    return Ok(result);
                }
            }

            _logger.LogError("Application Insights query failed: {StatusCode} - {Content}", 
                response.StatusCode, responseContent);

            return StatusCode((int)response.StatusCode, new QueryResult
            {
                Success = false,
                Error = $"Query failed: {response.StatusCode} - {responseContent}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Application Insights query");
            return StatusCode(500, new QueryResult
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    public class QueryRequest
    {
        public string Query { get; set; } = string.Empty;
        public string? Timespan { get; set; }
    }

    public class QueryResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public List<QueryTable>? Tables { get; set; }
    }

    public class QueryTable
    {
        public string Name { get; set; } = string.Empty;
        public List<QueryColumn> Columns { get; set; } = new();
        public List<List<object?>> Rows { get; set; } = new();
    }

    public class QueryColumn
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}


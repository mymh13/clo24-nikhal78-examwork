using System.Net.Http.Json;
using System.Text.Json;

namespace Ticketing.Web.Helpers;

public static class HttpErrorHelper
{
    public static async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
    {
        if (response == null)
        {
            return "An unexpected error occurred. Please try again.";
        }

        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (errorResponse.TryGetProperty("error", out var errorProperty))
            {
                return errorProperty.GetString() ?? GetDefaultErrorMessage(response.StatusCode);
            }
        }
        catch
        {
        }

        return GetDefaultErrorMessage(response.StatusCode);
    }

    private static string GetDefaultErrorMessage(System.Net.HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "Invalid request. Please check your input and try again.",
            System.Net.HttpStatusCode.Unauthorized => "You are not authorized to perform this action. Please log in.",
            System.Net.HttpStatusCode.Forbidden => "You do not have permission to perform this action.",
            System.Net.HttpStatusCode.NotFound => "The requested resource was not found.",
            System.Net.HttpStatusCode.Conflict => "A conflict occurred. The resource may already exist.",
            System.Net.HttpStatusCode.InternalServerError => "An error occurred on the server. Please try again later.",
            System.Net.HttpStatusCode.ServiceUnavailable => "The service is temporarily unavailable. Please try again later.",
            _ => "An error occurred. Please try again."
        };
    }

    public static string HandleException(Exception ex, string operation)
    {
        return ex switch
        {
            HttpRequestException => $"Unable to connect to the server. Please check your connection and try again.",
            TaskCanceledException => $"The request timed out. Please try again.",
            JsonException => $"Invalid response from server. Please try again.",
            _ => $"An error occurred while {operation}. Please try again."
        };
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ticketing.Web.Helpers;

public static class ErrorHandlerHelper
{
    public static ActionResult HandleException(Exception ex, ILogger logger, string operation, object? context = null)
    {
        logger.LogError(ex, "{Operation} failed. Context: {@Context}", operation, context);
        return new BadRequestObjectResult(new { error = GetUserFriendlyMessage(ex) });
    }

    public static ActionResult HandleValidationError(string errorMessage, ILogger? logger = null, string? operation = null)
    {
        if (logger != null && !string.IsNullOrEmpty(operation))
        {
            logger.LogWarning("Validation failed for {Operation}: {ErrorMessage}", operation, errorMessage);
        }

        return new BadRequestObjectResult(new { error = errorMessage });
    }

    public static ActionResult HandleNotFound(string resourceName, string identifier, ILogger? logger = null)
    {
        var errorMessage = $"{resourceName} not found.";
        
        if (logger != null)
        {
            logger.LogWarning("{ResourceName} not found: {Identifier}", resourceName, identifier);
        }

        return new NotFoundObjectResult(new { error = errorMessage });
    }

    public static ActionResult HandleUnauthorized(string errorMessage, ILogger? logger = null, string? userId = null)
    {
        if (logger != null)
        {
            logger.LogWarning("Unauthorized access attempt. User: {UserId}, Message: {ErrorMessage}", userId, errorMessage);
        }

        return new ObjectResult(new { error = errorMessage })
        {
            StatusCode = 403
        };
    }

    public static ActionResult HandleInternalError(Exception ex, ILogger logger, string operation, object? context = null)
    {
        logger.LogError(ex, "Internal server error during {Operation}. Context: {@Context}", operation, context);
        return new ObjectResult(new { error = "An unexpected error occurred. Please try again later." })
        {
            StatusCode = 500
        };
    }

    private static string GetUserFriendlyMessage(Exception ex)
    {
        return ex switch
        {
            ArgumentNullException argNull => $"Required parameter is missing: {argNull.ParamName}",
            ArgumentException argEx => argEx.Message,
            InvalidOperationException invalidOp => invalidOp.Message,
            KeyNotFoundException => "The requested resource was not found.",
            UnauthorizedAccessException => "You do not have permission to perform this operation.",
            TimeoutException => "The operation timed out. Please try again.",
            _ => "An error occurred while processing your request. Please try again."
        };
    }

    public static ActionResult CreateErrorResponse(string errorMessage, int statusCode = 400)
    {
        return new ObjectResult(new { error = errorMessage })
        {
            StatusCode = statusCode
        };
    }
}


using System.Net;
using System.Text.Json;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    /// <summary>
    /// Dataspace error response details
    /// </summary>
    private class ErrorResponseDetails
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Request processing failure!");
            var response = context.Response;
            response.ContentType = "application/json";

            ErrorResponseDetails errorResponseDetails = new()
            {
                Type = "InternalServerError",
                Message = error.Message
            };

            switch (error)
            {
                case NotAuthorizedException:
                    // custom application error
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponseDetails.Type = "Unauthorized";
                    break;
                case NotFoundException:
                    // not found error
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponseDetails.Type = "NotFound";
                    break;
                case BadRequestException e:
                    // bad request error
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponseDetails.Type = "BadRequest";
                    break;
                default:
                    // unhandled error
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponseDetails,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
            await response.WriteAsync(result);
        }
    }
}
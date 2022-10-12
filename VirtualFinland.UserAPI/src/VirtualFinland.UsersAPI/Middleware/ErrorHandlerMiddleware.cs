using System.Net;
using System.Text.Json;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    /// <summary>
    /// RFC7807 Problem Details
    /// </summary>
    private class ErrorResponseDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        
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

            switch(error)
            {
                case NotAuthorizedExpception e:
                    // custom application error
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case NotFoundException e:
                    // not found error
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                default:
                    // unhandled error
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var result = JsonSerializer.Serialize(new ErrorResponseDetails() { Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", Title = error?.Message, Detail = error?.Message, Status = response.StatusCode, Instance = response.HttpContext.Request.Path}, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            await response.WriteAsync(result);
        }
    }
}
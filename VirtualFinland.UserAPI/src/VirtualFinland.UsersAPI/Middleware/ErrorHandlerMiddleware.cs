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
            Dictionary<string, List<string>> validationErrorDetails = new Dictionary<string, List<string>>();

            switch(error)
            {
                case NotAuthorizedException:
                    // custom application error
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case NotFoundException:
                    // not found error
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case BadRequestException e:
                    // bad request error
                    response.StatusCode = (int)HttpStatusCode.BadRequest;

                    e.ValidationErrors?.ForEach( o => validationErrorDetails.Add(o.Field, new List<string>() { o.Message }));
                    break;
                default:
                    // unhandled error
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            ErrorResponseDetails errorResponseDetails = validationErrorDetails?.Count == 0 ? new ErrorResponseDetails()
            {
                Type = "https://tools.ietf.org/html/rfc7231",
                Title = error.Message,
                Detail = error.Message,
                Status = response.StatusCode,
                Instance = response.HttpContext.Request.Path
            } : new ErrorResponseDetails()
            {
                Type = "https://tools.ietf.org/html/rfc7231",
                Title = error.Message,
                Detail = error.Message,
                Status = response.StatusCode,
                Instance = response.HttpContext.Request.Path,
                Extensions = { new KeyValuePair<string, object?>( "errors", validationErrorDetails)  }
            };

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
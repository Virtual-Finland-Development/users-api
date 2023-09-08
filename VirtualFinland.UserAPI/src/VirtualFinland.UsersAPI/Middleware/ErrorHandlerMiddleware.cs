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
    private class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dataspace validation error response details
    /// </summary>
    private class ValidationErrorResponse
    {
        public List<ValidationErrorDetail> Detail { get; set; } = new List<ValidationErrorDetail>();
    }

    private class ValidationErrorDetail
    {
        public List<object> Loc { get; set; } = default!;
        public string Msg { get; set; } = default!;
        public string Type { get; set; } = default!;
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
            switch (error)
            {
                case NotAuthorizedException:
                    await WriteErrorResponse(context, new ErrorResponse()
                    {
                        Type = "Unauthorized",
                        Message = error.Message ?? "Not authorized"
                    }, HttpStatusCode.Unauthorized);
                    break;
                case NotFoundException:
                    await WriteErrorResponse(context, new ErrorResponse()
                    {
                        Type = "NotFound",
                        Message = error.Message ?? "Not found"
                    }, HttpStatusCode.NotFound);
                    break;
                case BadRequestException:
                    _logger.LogError(error, "Request processing failure!");
                    await WriteErrorResponse(context, new ErrorResponse()
                    {
                        Type = "BadRequest",
                        Message = error.Message ?? "Bad request"
                    }, HttpStatusCode.BadRequest);
                    break;
                case ValidationException:
                    var validationError = error as ValidationException;
                    if (validationError?.Errors is not null)
                    {

                        var errorDetail = new List<ValidationErrorDetail>();
                        foreach (var validationErrorDetail in validationError.Errors)
                        {
                            errorDetail.Add(new ValidationErrorDetail()
                            {
                                Loc = new List<object>() { validationErrorDetail.Key },
                                Msg = string.Join(" ", validationErrorDetail.Value),
                                Type = "ValidationError"
                            });
                        }

                        await WriteErrorResponse(context, new ValidationErrorResponse()
                        {
                            Detail = errorDetail,
                        }, HttpStatusCode.UnprocessableEntity);
                    }
                    else
                    {
                        await WriteErrorResponse(context, new ErrorResponse()
                        {
                            Type = "UnprocessableEntity",
                            Message = error.Message ?? "Validation error"
                        }, HttpStatusCode.UnprocessableEntity);
                    }
                    break;
                default:
                    _logger.LogError(error, "Request processing failure!");
                    await WriteErrorResponse(context, new ErrorResponse()
                    {
                        Type = "InternalServerError",
                        Message = error.Message ?? "Internal server error"
                    }, HttpStatusCode.InternalServerError);
                    break;
            }
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, object errorResponseDetails, HttpStatusCode statusCode)
    {

        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(errorResponseDetails,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }
        );
        await response.WriteAsync(result);
    }
}
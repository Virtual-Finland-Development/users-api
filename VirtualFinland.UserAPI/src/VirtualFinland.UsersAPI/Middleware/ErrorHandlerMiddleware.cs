using System.Net;
using System.Text.Json;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

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
            // Debug mode logging
            _logger.LogDebug(error, "Debug");

            // Attach exception to the request context for request logger middleware
            context.Items.Add("Exception", error);

            // Write the error response
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
                case FluentValidation.ValidationException:
                    var fluentValidationError = error as FluentValidation.ValidationException;
                    if (fluentValidationError?.Errors is not null)
                    {
                        var errorDetail = new List<ValidationErrorDetail>();
                        foreach (var validationErrorDetail in fluentValidationError.Errors)
                        {
                            errorDetail.Add(new ValidationErrorDetail()
                            {
                                Loc = new List<object>() { validationErrorDetail.PropertyName },
                                Msg = validationErrorDetail.ErrorMessage,
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
                case TimeoutException:
                    await WriteErrorResponse(context, new ErrorResponse()
                    {
                        Type = "RequestTimeout",
                        Message = error.Message ?? "Request timeout on external service"
                    }, HttpStatusCode.RequestTimeout);
                    break;
                default:
                    await WriteErrorResponse(context, new ErrorResponse()
                    {
                        Type = "InternalServerError",
                        Message = error.Message ?? "Internal server error"
                    }, HttpStatusCode.InternalServerError);
                    break;
            }
        }
    }

    /// <summary>
    /// Write the error response    
    /// </summary>
    /// <param name="context"></param>
    /// <param name="errorResponse"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    private static async Task WriteErrorResponse(HttpContext context, object errorResponse, HttpStatusCode statusCode)
    {

        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(errorResponse,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }
        );
        await response.WriteAsync(result);
    }

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
}
using System.Net;
using System.Text.Json;
using VirtualFinland.UserAPI.Exceptions;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// RFC7807 Problem Details
    /// </summary>
    private class ErrorResponseDetails
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }
    }

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
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

            var result = JsonSerializer.Serialize(new ErrorResponseDetails() { Title = error?.Message, Detail = error?.Message, Status = response.StatusCode.ToString(), Instance = response.HttpContext.Request.Path}, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            await response.WriteAsync(result);
        }
    }
}
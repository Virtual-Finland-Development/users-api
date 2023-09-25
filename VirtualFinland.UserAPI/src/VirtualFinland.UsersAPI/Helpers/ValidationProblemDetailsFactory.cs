using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Helpers;

public class ValidationProblemDetailsFactory : ProblemDetailsFactory
{
    public override ProblemDetails CreateProblemDetails(HttpContext httpContext, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ModelStateDictionary modelStateDictionary, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null)
    {
        var problemDetails = new ValidationProblemDetails(modelStateDictionary)
        {
            Type = type,
            Title = title,
            Status = 422,
            Detail = detail,
            Instance = instance
        };
        throw new ValidationException(problemDetails); // Pass the problem to the exception handler
    }
}
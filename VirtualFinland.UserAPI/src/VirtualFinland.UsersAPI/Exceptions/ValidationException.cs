using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace VirtualFinland.UserAPI.Exceptions;

public class ValidationException : Exception
{
    public ValidationException() : base() { }

    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, params object[] args)
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }

    public ValidationException(ValidationProblemDetails validationProblemDetails) : base()
    {
        Errors = validationProblemDetails.Errors;
    }

    public IDictionary<string, string[]>? Errors { get; }
}
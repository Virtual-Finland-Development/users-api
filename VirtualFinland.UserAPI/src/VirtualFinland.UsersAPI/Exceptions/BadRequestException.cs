using System.Globalization;

namespace VirtualFinland.UserAPI.Exceptions;

public class BadRequestException : Exception
{

    public List<ValidationErrorDetail>? ValidationErrors { get; }
    public BadRequestException() : base() {}

    public BadRequestException(string message) : base(message) { }

    public BadRequestException(string message, List<ValidationErrorDetail> validationErrors) : base(message)
    {
        this.ValidationErrors = validationErrors;
    }

    public BadRequestException(string message, params object[] args) 
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}
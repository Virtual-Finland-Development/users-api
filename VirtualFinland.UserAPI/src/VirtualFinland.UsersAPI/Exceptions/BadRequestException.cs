using System.Globalization;

namespace VirtualFinland.UserAPI.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException() : base() {}

    public BadRequestException(string message) : base(message) { }

    public BadRequestException(string message, params object[] args) 
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}
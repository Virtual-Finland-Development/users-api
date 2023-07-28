using System.Globalization;

namespace VirtualFinland.UserAPI.Exceptions;

public class NotFoundException : HandledException
{
    public NotFoundException() : base() { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, params object[] args)
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}
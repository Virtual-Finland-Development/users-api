using System.Globalization;

namespace VirtualFinland.UserAPI.Exceptions;

public class NotAuthorizedException : HandledException
{
    public NotAuthorizedException() : base() { }

    public NotAuthorizedException(string message) : base(message) { }

    public NotAuthorizedException(string message, params object[] args)
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}
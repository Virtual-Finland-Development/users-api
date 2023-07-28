using System.Globalization;

namespace VirtualFinland.UserAPI.Exceptions;

public class HandledException : Exception
{
    public HandledException() : base() { }

    public HandledException(string message) : base(message) { }

    public HandledException(string message, params object[] args)
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}
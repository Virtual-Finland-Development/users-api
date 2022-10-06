using System.Globalization;

namespace VirtualFinland.UserAPI.Exceptions;

public class NotAuthorizedExpception : Exception
{
    public NotAuthorizedExpception() : base() {}

    public NotAuthorizedExpception(string message) : base(message) { }

    public NotAuthorizedExpception(string message, params object[] args) 
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}
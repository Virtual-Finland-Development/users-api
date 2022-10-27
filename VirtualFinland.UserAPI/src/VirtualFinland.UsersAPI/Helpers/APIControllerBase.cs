using Microsoft.AspNetCore.Mvc;
using VirtualFinland.UserAPI.Middleware;

namespace VirtualFinland.UserAPI.Helpers;

public class ApiControllerBase : ControllerBase
{
    protected Guid? UserDdId
    {
        get
        {
            return (Guid?)this.HttpContext.Items[IdentityProviderAuthMiddleware.ContextItemUserDbIdName];
        }
    } 
}
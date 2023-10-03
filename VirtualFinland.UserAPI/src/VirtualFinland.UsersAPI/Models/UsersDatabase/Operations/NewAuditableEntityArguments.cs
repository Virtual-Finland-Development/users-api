namespace VirtualFinland.UserAPI.Models.UsersDatabase.Operations;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UserAPI.Security.Models;

public class NewAuditableEntityArguments : Auditable
{
    public NewAuditableEntityArguments()
    {
        Created = DateTime.Now;
        Modified = DateTime.Now;
    }

    public NewAuditableEntityArguments(RequestAuthenticatedUser user)
    {
        Created = DateTime.Now;
        Modified = DateTime.Now;
        Metadata = new AuditableMetadata(user);
    }
}
namespace VirtualFinland.UserAPI.Security.Models;

public enum AuditLogEvent
{
    Create,
    Read,
    Update,
    Delete
}

public static class AuditLogEventExtensions
{
    public static string ToString(this AuditLogEvent auditLogEvent)
    {
        return auditLogEvent.ToString().ToUpper();
    }
}
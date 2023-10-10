
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Extensions;

public static class AuditLoggerExtensions
{
    public static void LogAuditLogEvent(this ILogger logger, AuditLogEvent auditEvent, RequestAuthenticatedUser requestAuthenticatedUser)
    {
        logger.LogInformation("AuditLog: {auditEvent} on {user}", auditEvent.ToString().ToUpper(), requestAuthenticatedUser);
    }
}
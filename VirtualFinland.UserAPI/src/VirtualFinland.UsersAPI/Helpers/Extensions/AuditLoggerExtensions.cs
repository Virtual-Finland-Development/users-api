
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Extensions;

public static class AuditLoggerExtensions
{
    public static void LogAuditLogEvent(this ILogger logger, AuditLogEvent auditEvent, string eventContextInfo, RequestAuthenticatedUser requestAuthenticatedUser)
    {
        logger.LogInformation("AuditLog: {auditEvent} ({eventContextInfo}) on {user}", auditEvent.ToString().ToUpper(), eventContextInfo, requestAuthenticatedUser);
    }
}
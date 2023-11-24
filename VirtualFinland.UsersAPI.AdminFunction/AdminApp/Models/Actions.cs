namespace VirtualFinland.AdminFunction.AdminApp.Models;

public enum Actions
{
    Migrate,
    InitializeDatabaseUser,
    InitializeDatabaseAuditLogTriggers,
    UpdateTermsOfService,
    UpdateAnalytics,
    InvalidateCaches,
}
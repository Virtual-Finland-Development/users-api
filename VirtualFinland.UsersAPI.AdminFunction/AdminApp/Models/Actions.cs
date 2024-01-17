namespace VirtualFinland.AdminFunction.AdminApp.Models;

public enum Actions
{
    InitializeDatabase,
    Migrate,
    InitializeDatabaseUser,
    InitializeDatabaseAuditLogTriggers,
    UpdateTermsOfService,
    UpdateAnalytics,
    InvalidateCaches,
    UpdatePerson,
    RunCleanups,
}
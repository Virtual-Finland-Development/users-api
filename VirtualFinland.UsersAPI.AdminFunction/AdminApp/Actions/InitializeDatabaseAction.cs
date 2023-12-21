namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Runs the initial database setup actions: migrate, setup database user and audit log triggers
/// </summary>
public class InitializeDatabaseAction : IAdminAppAction
{
    private readonly MigrateAction _migrateAction;
    private readonly InitializeDatabaseUserAction _initializeDatabaseUserAction;
    private readonly InitializeDatabaseAuditLogTriggersAction _initializeDatabaseAuditLogTriggersAction;

    public InitializeDatabaseAction(MigrateAction databaseMigrationAction, InitializeDatabaseUserAction initializeDatabaseUserAction, InitializeDatabaseAuditLogTriggersAction databaseAuditLogTriggersInitializationAction)
    {
        _migrateAction = databaseMigrationAction;
        _initializeDatabaseUserAction = initializeDatabaseUserAction;
        _initializeDatabaseAuditLogTriggersAction = databaseAuditLogTriggersInitializationAction;
    }

    public async Task Execute(string? payload)
    {
        await _migrateAction.Execute(payload);
        await _initializeDatabaseUserAction.Execute(payload);
        await _initializeDatabaseAuditLogTriggersAction.Execute(payload);
    }
}
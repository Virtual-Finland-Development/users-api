namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Runs the initial database setup actions: migrate, setup database user and audit log triggers
/// </summary>
public class InitializeDatabaseAction : IAdminAppAction
{
    private readonly MigrateAction _databaseMigrationAction;
    private readonly DatabaseUserInitializationAction _databaseUserInitializationAction;
    private readonly InitializeDatabaseAuditLogTriggersAction _databaseAuditLogTriggersInitializationAction;

    public InitializeDatabaseAction(MigrateAction databaseMigrationAction, DatabaseUserInitializationAction databaseUserInitializationAction, InitializeDatabaseAuditLogTriggersAction databaseAuditLogTriggersInitializationAction)
    {
        _databaseMigrationAction = databaseMigrationAction;
        _databaseUserInitializationAction = databaseUserInitializationAction;
        _databaseAuditLogTriggersInitializationAction = databaseAuditLogTriggersInitializationAction;
    }

    public async Task Execute(string? payload)
    {
        await _databaseMigrationAction.Execute(payload);
        await _databaseUserInitializationAction.Execute(payload);
        await _databaseAuditLogTriggersInitializationAction.Execute(payload);
    }
}
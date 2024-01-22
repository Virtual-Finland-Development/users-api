using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using static VirtualFinland.UserAPI.Helpers.Services.NotificationService;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Flag for deletion the persons that have not been active in years
/// A month after the delete-flagging, delete them
/// </summary>
public class RunCleanupsAction : IAdminAppAction
{
    private readonly UsersDbContext _dataContext;
    private readonly ActionDispatcherService _actionDispatcherService;
    private readonly CleanupConfig _config;
    private readonly ILogger<RunCleanupsAction> _logger;

    public RunCleanupsAction(UsersDbContext dataContext, ActionDispatcherService actionDispatcherService, CleanupConfig config, ILogger<RunCleanupsAction> logger)
    {
        _dataContext = dataContext;
        _actionDispatcherService = actionDispatcherService;
        _config = config;
        _logger = logger;
    }

    public async Task Execute(string? _)
    {
        if (_config.AbandonedAccounts.IsEnabled)
        {
            await FlagInactiveAccountsAsAbandoned();
            await RunAbandonedAccountsCleanup();
        }
    }

    public async Task FlagInactiveAccountsAsAbandoned()
    {
        var flagAsAbandonedAt = DateTime.UtcNow.AddDays(-_config.AbandonedAccounts.FlagAsAbandonedInDays);

        // Find persons that have not been active in years
        var abandonedPersons = await _dataContext.Persons
            .AsQueryable()
            .Where(p => p.ToBeDeletedFromInactivity != true && p.LastActive != null && p.LastActive < flagAsAbandonedAt)
            .Take(_config.AbandonedAccounts.MaxPersonsToFlagPerDay)
            .ToListAsync();

        _logger.LogInformation("Flagging {abandonedPersonsCount} accounts that have not been active since {flagAsAbandonedAt}", abandonedPersons.Count, flagAsAbandonedAt);

        // Mark for deletion
        foreach (var abandonedPerson in abandonedPersons)
        {
            _logger.LogInformation("Marking person {PersonId} for deletion", abandonedPerson.Id);
            await _actionDispatcherService.UpdatePersonToBeDeletedFlag(abandonedPerson);
        }
    }

    public async Task RunAbandonedAccountsCleanup()
    {
        var deletionAt = DateTime.UtcNow.AddDays(-_config.AbandonedAccounts.DeleteFlaggedAfterDays);

        // Find persons that have not been active in years
        var abandonedPersons = await _dataContext.Persons
            .AsQueryable()
            .Where(p => p.ToBeDeletedFromInactivity == true)
            .Take(_config.AbandonedAccounts.MaxPersonsToFlagPerDay)
            .ToListAsync();

        var pendingDeletions = abandonedPersons.Where(p => p.Modified > deletionAt).ToList();
        var deletes = abandonedPersons.Where(p => p.Modified <= deletionAt).ToList();

        _logger.LogInformation("Found {abandonedPersonsCount} accounts that have been marked for deletion of which {pendingDeletionsCount} are still pending and {deletesCount} are ready to be deleted", abandonedPersons.Count, pendingDeletions.Count, deletes.Count);

        foreach (var pendingDeletion in pendingDeletions)
        {
            _logger.LogInformation("Person {PersonId} is still pending deletion", pendingDeletion.Id);
        }

        foreach (var abandonedPerson in deletes)
        {
            _logger.LogInformation("Deleting person {PersonId} because it has been marked for deletion for over a month", abandonedPerson.Id);
            await _actionDispatcherService.DeleteAbandonedPerson(abandonedPerson);
        }
    }
}
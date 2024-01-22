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
        Console.WriteLine($"Flagging accounts that have not been active since {flagAsAbandonedAt}");

        // Find persons that have not been active in years
        var abandonedPersons = await _dataContext.Persons
            .AsQueryable()
            .Where(p => p.ToBeDeletedFromInactivity != true && (p.LastActive == null || p.LastActive < flagAsAbandonedAt))
            .Take(_config.AbandonedAccounts.MaxPersonsToFlagPerDay)
            .ToListAsync();

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

        foreach (var abandonedPerson in abandonedPersons)
        {
            if (abandonedPerson.Modified > deletionAt)
            {
                _logger.LogInformation("Person {PersonId} has been marked for deletion for less than a month", abandonedPerson.Id);
            }
            else
            {
                _logger.LogInformation("Deleting person {PersonId} because it has been marked for deletion for over a month", abandonedPerson.Id);
                await _actionDispatcherService.DeleteAbandonedPerson(abandonedPerson);
            }
        }
    }
}
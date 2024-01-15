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
    private readonly NotificationService _notificationService;
    private readonly CleanupConfig _config;
    private readonly ILogger<RunCleanupsAction> _logger;

    public RunCleanupsAction(UsersDbContext dataContext, NotificationService notificationService, CleanupConfig config, ILogger<RunCleanupsAction> logger)
    {
        _dataContext = dataContext;
        _notificationService = notificationService;
        _config = config;
        _logger = logger;
    }

    public async Task Execute(string? _)
    {
        if (_config.AbandonedAccounts.IsEnabled)
        {
            await RunAbandoedAccountsCleanup();
        }
    }

    public async Task RunAbandoedAccountsCleanup()
    {
        var flagAsAbandonedAt = DateTime.UtcNow.AddDays(-_config.AbandonedAccounts.FlagAsAbandonedInDays);
        var deletionAt = DateTime.UtcNow.AddDays(-_config.AbandonedAccounts.DeleteFlaggedAfterDays);

        // Find persons that have not been active in years
        var abandonedPersons = await _dataContext.Persons.Where(p => p.LastActive != null && p.LastActive < flagAsAbandonedAt).ToListAsync();

        // Mark them for deletion
        foreach (var abandonedPerson in abandonedPersons)
        {
            if (abandonedPerson.ToBeDeletedFromInactivity)
            {
                if (abandonedPerson.Modified > deletionAt)
                {
                    _logger.LogInformation("Person {PersonId} has been marked for deletion for less than a month", abandonedPerson.Id);
                }
                else
                {
                    _logger.LogInformation("Deleting person {PersonId} because it has been marked for deletion for over a month", abandonedPerson.Id);
                    _dataContext.Persons.Remove(abandonedPerson);
                    await _notificationService.SendPersonNotification(abandonedPerson, NotificationTemplate.AccountDeletedFromInactivity);
                }
            }
            else
            {
                _logger.LogInformation("Marking person {PersonId} for deletion", abandonedPerson.Id);
                abandonedPerson.ToBeDeletedFromInactivity = true; // Updates Modified attr too
                await _notificationService.SendPersonNotification(abandonedPerson, NotificationTemplate.AccountToBeDeletedFromInactivity);
            }
        }

        await _dataContext.SaveChangesAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Flag for deletion the persons that have not been active in years
/// A month after the delete-flagging, delete them
/// </summary>
public class AbandonedAccountsCleanupAction : IAdminAppAction
{
    private readonly UsersDbContext _dataContext;
    private readonly ILogger<AbandonedAccountsCleanupAction> _logger;

    public AbandonedAccountsCleanupAction(UsersDbContext dataContext, ILogger<AbandonedAccountsCleanupAction> logger)
    {
        _dataContext = dataContext;
        _logger = logger;
    }

    public async Task Execute(string? _)
    {
        var dateYearsAgo = DateTime.UtcNow.AddYears(-3);
        var dateMonthAgo = DateTime.UtcNow.AddMonths(-1);

        // Find persons that have not been active in years
        var abandonedPersons = await _dataContext.Persons.Where(p => p.LastActive != null && p.LastActive < dateYearsAgo).ToListAsync();

        // Mark them for deletion
        foreach (var abandonedPerson in abandonedPersons)
        {
            if (abandonedPerson.ToBeDeletedFromInactivity)
            {
                if (abandonedPerson.Modified > dateMonthAgo)
                {
                    _logger.LogInformation("Person {PersonId} has been marked for deletion for less than a month", abandonedPerson.Id);
                }
                else
                {
                    _logger.LogInformation("Deleting person {PersonId} because it has been marked for deletion for over a month", abandonedPerson.Id);
                    _dataContext.Persons.Remove(abandonedPerson);

                }
            }
            else
            {
                _logger.LogInformation("Marking person {PersonId} for deletion", abandonedPerson.Id);
                abandonedPerson.ToBeDeletedFromInactivity = true; // Updates Modified attr too
            }
        }

        await _dataContext.SaveChangesAsync();
    }
}
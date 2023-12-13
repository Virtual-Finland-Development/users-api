using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Configurations;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

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

        // Find persons that have not been active in years
        var abandonedPersons = await _dataContext.Persons.AllAsync(p => p.LastActive != null && p.LastActive < dateYearsAgo);




    }
}
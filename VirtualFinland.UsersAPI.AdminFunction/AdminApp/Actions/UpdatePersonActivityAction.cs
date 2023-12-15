using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

public class UpdatePersonActivityAction : IAdminAppAction
{
    private readonly UsersDbContext _dataContext;
    private readonly ILogger<UpdatePersonActivityAction> _logger;

    public UpdatePersonActivityAction(UsersDbContext dataContext, ILogger<UpdatePersonActivityAction> logger)
    {
        _dataContext = dataContext;
        _logger = logger;
    }

    public async Task Execute(string? input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        var actionInput = JsonSerializer.Deserialize<UpdatePersonActivityActionInput>(input) ??
            throw new ArgumentException("Invalid input", nameof(input));

        // Find the person
        var person = await _dataContext.Persons.FirstOrDefaultAsync(p => p.Id == actionInput.PersonId);
        if (person is null)
        {
            _logger.LogWarning("Person {PersonId} not found", actionInput.PersonId);
            return;
        }

        // Update the last active date
        person.LastActive = DateTime.UtcNow;
        person.ToBeDeletedFromInactivity = false;
        await _dataContext.SaveChangesAsync();

        _logger.LogInformation("Person {PersonId} last active date updated", actionInput.PersonId);
    }

    public record UpdatePersonActivityActionInput
    {
        public Guid PersonId { get; init; }
    }
}
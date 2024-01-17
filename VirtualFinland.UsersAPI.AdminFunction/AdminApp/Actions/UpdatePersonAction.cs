using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using static VirtualFinland.UserAPI.Helpers.Services.NotificationService;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

public class UpdatePersonAction : IAdminAppAction
{
    private readonly UsersDbContext _dataContext;
    private readonly ILogger<UpdatePersonAction> _logger;
    private readonly NotificationService _notificationService;


    public UpdatePersonAction(UsersDbContext dataContext, ILogger<UpdatePersonAction> logger, NotificationService notificationService)
    {
        _dataContext = dataContext;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Execute(string? input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        var actionInput = JsonSerializer.Deserialize<UpdatePersonActionInput>(input) ??
            throw new ArgumentException("Invalid input", nameof(input));

        // Find the person
        var person = await _dataContext.Persons.FirstOrDefaultAsync(p => p.Id == actionInput.PersonId);
        if (person is null)
        {
            _logger.LogWarning("Person {PersonId} not found", actionInput.PersonId);
            return;
        }

        // Update the person metadata
        switch (actionInput.Type)
        {
            case UpdatePersonActionType.UpdateLastActiveDate:
                await UpdateLastActiveDate(person);
                break;
            case UpdatePersonActionType.UpdateToBeDeletedFlag:
                await UpdateToBeDeletedFlag(person);
                break;
            case UpdatePersonActionType.DeleteAbandonedPerson:
                await DeleteAbandonedPerson(person);
                break;
            default:
                throw new ArgumentException("Invalid action type", nameof(actionInput.Type));
        }
    }

    private async Task UpdateLastActiveDate(Person person)
    {
        _logger.LogInformation("Updating Person {PersonId} last active date", person.Id);
        person.LastActive = DateTime.UtcNow;  // Update the last active date
        person.ToBeDeletedFromInactivity = false; // Reset the to be deleted flag
        await _dataContext.SaveChangesAsync();
    }

    private async Task UpdateToBeDeletedFlag(Person person)
    {
        _logger.LogInformation("Marking person {PersonId} for deletion", person.Id);
        person.ToBeDeletedFromInactivity = true; // Updates Modified attr too
        await _dataContext.SaveChangesAsync();
        await _notificationService.SendPersonNotification(person, NotificationTemplate.AccountToBeDeletedFromInactivity);
    }

    private async Task DeleteAbandonedPerson(Person person)
    {
        if (!person.ToBeDeletedFromInactivity)
        {
            _logger.LogWarning("Person {PersonId} is not marked for deletion", person.Id);
            return;
        }

        _logger.LogInformation("Deleting person {PersonId} because it has been marked for deletion for over a month", person.Id);
        _dataContext.Persons.Remove(person);
        await _dataContext.SaveChangesAsync();
        await _notificationService.SendPersonNotification(person, NotificationTemplate.AccountDeletedFromInactivity);
    }

    public record UpdatePersonActionInput
    {
        public Guid PersonId { get; init; }
        public UpdatePersonActionType Type { get; init; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UpdatePersonActionType
    {
        UpdateLastActiveDate,
        UpdateToBeDeletedFlag,
        DeleteAbandonedPerson
    }
}
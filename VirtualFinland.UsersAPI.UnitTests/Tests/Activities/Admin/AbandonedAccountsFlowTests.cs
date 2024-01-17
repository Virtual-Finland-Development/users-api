using Moq;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.AdminFunction.AdminApp.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Admin;

public class AbandonedAccountsFlowTests : APITestBase
{

    [Fact]
    /// <summary>
    /// 
    /// </summary>
    public async Task PersonShouldBeDeletedAfterLongInactivity()
    {
        // Arrange
        var notificationsConfig = new NotificationsConfig(new NotificationsConfig.EmailConfigValues
        {
            IsEnabled = false
        });
        var notificationService = new NotificationService(
            notificationsConfig,
            new EmailTemplates(notificationsConfig),
            new Mock<ILogger<NotificationService>>().Object
        );
        var logger = new Mock<ILogger<UpdatePersonAction>>();
        var updatePersonAction = new UpdatePersonAction(_dbContext, logger.Object, notificationService);


        var person = _dbContext.Persons.Add(new PersonBuilder().Build()).Entity;
        person.LastActive = DateTime.UtcNow.AddYears(-3);
        await _dbContext.SaveChangesAsync();

        // Act
        await updatePersonAction.UpdateToBeDeletedFlag(person);
        await updatePersonAction.DeleteAbandonedPerson(person);

        // Assert
        var deletedPerson = await _dbContext.Persons.Where(p => p.Id == person.Id).FirstOrDefaultAsync();
        Assert.Null(deletedPerson);
    }
}

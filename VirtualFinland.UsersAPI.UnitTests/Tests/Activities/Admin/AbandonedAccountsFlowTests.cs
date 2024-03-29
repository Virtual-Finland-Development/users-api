using Moq;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.AdminFunction.AdminApp.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UserAPI.Data.Repositories;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Admin;

public class AbandonedAccountsFlowTests : APITestBase
{

    [Fact]
    /// <summary>
    /// Test that a person is flagged for deletion after long inactivity and then deleted
    /// </summary>
    public async Task PersonShouldBeDeletedAfterLongInactivity()
    {
        // Arrange
        var person = _dbContext.Persons.Add(new PersonBuilder().Build()).Entity;
        person.LastActive = DateTime.UtcNow.AddYears(-3);
        await _dbContext.SaveChangesAsync();

        var notificationsConfig = new NotificationsConfig(new NotificationsConfig.EmailConfigValues
        {
            IsEnabled = false
        });
        var notificationService = new NotificationService(
            notificationsConfig,
            new EmailTemplates(notificationsConfig),
            new Mock<ActionDispatcherService>().Object,
            new Mock<ILogger<NotificationService>>().Object
        );
        var logger = new Mock<ILogger<UpdatePersonAction>>();
        var analyticsLoggerFactoryMock = GetMockedAnalyticsLoggerFactory();
        var servicesProvider = GetMockedServiceProvider();
        var personRepository = new PersonRepository(servicesProvider.Object);
        var updatePersonAction = new UpdatePersonAction(_dbContext, personRepository, analyticsLoggerFactoryMock, notificationService);

        var actionDispatcherService = new Mock<ActionDispatcherService>();
        actionDispatcherService.Setup(x =>
            x.DeleteAbandonedPerson(It.IsAny<Person>()))
            .Returns(async () =>
            {
                await updatePersonAction.DeleteAbandonedPerson(person);
                return Task.CompletedTask;
            });
        actionDispatcherService.Setup(x =>
            x.UpdatePersonToBeDeletedFlag(It.IsAny<Person>()))
            .Returns(async () =>
            {
                await updatePersonAction.UpdateToBeDeletedFlag(person);
                return Task.CompletedTask;
            });

        var runCleanupsAction = new RunCleanupsAction(_dbContext, actionDispatcherService.Object, new CleanupConfig(
            new CleanupConfig.AbandonedAccountsConfig
            {
                IsEnabled = true,
                FlagAsAbandonedInDays = 1,
                DeleteFlaggedAfterDays = 1,
                MaxPersonsToFlagPerDay = 1
            }
        ), new Mock<ILogger<RunCleanupsAction>>().Object);

        // Act 1
        await runCleanupsAction.FlagInactiveAccountsAsAbandoned();
        // Assert 1
        var abandonedPerson = await _dbContext.Persons.Where(p => p.Id == person.Id).FirstOrDefaultAsync();
        Assert.NotNull(abandonedPerson);
        Assert.True(abandonedPerson.ToBeDeletedFromInactivity);

        // Act 2
        await runCleanupsAction.RunAbandonedAccountsCleanup();
        // Assert 2
        var notYetDeletedPerson = await _dbContext.Persons.Where(p => p.Id == person.Id).FirstOrDefaultAsync();
        Assert.NotNull(notYetDeletedPerson);
        Assert.True(notYetDeletedPerson.ToBeDeletedFromInactivity);

        // Interlude: update the modified date to be old enough
        notYetDeletedPerson.Modified = DateTime.UtcNow.AddDays(-2);
        await _dbContext.SaveChangesAsync();

        // Act 3
        await runCleanupsAction.RunAbandonedAccountsCleanup();
        // Assert 3
        var deletedPerson = await _dbContext.Persons.Where(p => p.Id == person.Id).FirstOrDefaultAsync();
        Assert.Null(deletedPerson);
    }
}

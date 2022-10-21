using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Models;
using VirtualFinland.UsersAPI.UnitTests.Helpers;


namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User;

public class UserTests : APITestBase
{
    [Fact]
    public async void Should_GetUserAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<GetUser.Handler>>();
        var query = new GetUser.Query(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);
        var handler = new GetUser.Handler(_dbContext, mockLogger.Object);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should()
            .Match<GetUser.User>(o =>
                o.Id == dbEntities.user.Id &&
                o.Address == dbEntities.user.Address &&
                o.FirstName == dbEntities.user.FirstName &&
                o.LastName == dbEntities.user.LastName &&
                o.ImmigrationDataConsent == dbEntities.user.ImmigrationDataConsent &&
                o.JobsDataConsent == dbEntities.user.JobsDataConsent);
        
    }
    
    [Fact]
    public async void Should_UpdateUserAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var command = new UpdateUser.Command("New FirstName", "New LastName", string.Empty, true, false, "en", "en", "5001","en",new List<string>(), new List<string>());
        command.SetAuth(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);
        var handler = new UpdateUser.Handler(_dbContext, mockLogger.Object);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should()
            .Match<UpdateUser.User>(o =>
                o.Id == dbEntities.user.Id &&
                o.FirstName == command.FirstName &&
                o.LastName == command.LastName &&
                o.ImmigrationDataConsent == command.ImmigrationDataConsent &&
                o.JobsDataConsent == command.JobsDataConsent &&
                o.NationalityCode == command.NationalityCode &&
                o.NativeLanguageCode == command.NativeLanguageCode &&
                o.OccupationCode == command.OccupationCode &&
                o.CountryOfBirthCode == command.CountryOfBirthCode);
        
    }
}
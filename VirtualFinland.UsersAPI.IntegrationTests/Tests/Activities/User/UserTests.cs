using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Models;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.UsersAPI.UnitTests.Mocks;
using UpdateUser = VirtualFinland.UserAPI.Activities.User.Operations.UpdateUser;


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
                o.JobsDataConsent == dbEntities.user.JobsDataConsent &&
                o.NationalityCode == dbEntities.user.NationalityISOCode &&
                o.OccupationCode == dbEntities.user.OccupationISCOCode &&
                o.NativeLanguageCode == dbEntities.user.NationalityISOCode &&
                o.CountryOfBirthCode == dbEntities.user.CountryOfBirthISOCode &&
                o.Gender == dbEntities.user.Gender &&
                DateOnly.FromDateTime(o.DateOfBirth.Value) == dbEntities.user.DateOfBirth);
        
    }
    
    [Fact]
    public async void Should_UpdateUserAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockContriesRepository();
        var languageRepository = new LanguageRepository();
        
        var command = new UpdateUser.Command("New FirstName", "New LastName", string.Empty, true, false, "fi", "fi-FI", "01","fi",new List<string>(), new List<string>(), "1", DateTime.Now);
        command.SetAuth(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);
        var handler = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
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
                o.CountryOfBirthCode == command.CountryOfBirthCode &&
                o.Gender == command.Gender);
        
    }
    
    [Fact]
    public async void Should_FailUpdateUserNationalityCheckAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockContriesRepository();
        var languageRepository = new LanguageRepository();
        
        var command = new UpdateUser.Command(null, null, null, null, null, null, null, null,"not a code",null, null, null, null);
        command.SetAuth(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);
        var handler = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*NationalityCode*");
    }
    
    [Fact]
    public async void Should_FailUpdateUserCountryOfBirthAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockContriesRepository();
        var languageRepository = new LanguageRepository();
        
        var command = new UpdateUser.Command(null, null, null, null, null, "not a code", null, null,null,null, null, null, null);
        command.SetAuth(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);
        var handler = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*CountryOfBirthCode*");
    }
    
    [Fact]
    public async void Should_FailUpdateUserNativeLanguageCodeAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockContriesRepository();
        var languageRepository = new LanguageRepository();
        
        var command = new UpdateUser.Command(null, null, null, null, null, null, "not a code", null,null,null, null, null, null);
        command.SetAuth(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);
        var handler = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*NativeLanguageCode*");
    }
    
    [Fact]
    public async void Should_FailUpdateUserOccupationCodeAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockContriesRepository();
        var languageRepository = new LanguageRepository();
        
        var command = new UpdateUser.Command(null, null, null, null, null, null, null, "not a code",null,null, null, null, null);
        command.SetAuth(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);
        var handler = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*OccupationCode*");
    }
    
    [Fact]
    public async void Should_FailUpdateUserAuthenticationCheckAsync()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockContriesRepository();
        var languageRepository = new LanguageRepository();
        
        var command = new UpdateUser.Command(null, null, null, null, null, null, null, "not a code",null,null, null, null, null);
        command.SetAuth("false user id", "false issuer");
        var handler = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>().WithMessage("*User could not be identified as a valid user*");
    }
    
    [Fact]
    public async void Should_FailGetUserAuthenticationCheckAsync()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GetUser.Handler>>();
        var query = new GetUser.Query("false user id", "false issuer");
        var handler = new GetUser.Handler(_dbContext, mockLogger.Object);
        
        // Act
        var act = () => handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>().WithMessage("*User could not be identified as a valid user*");;
    }
}
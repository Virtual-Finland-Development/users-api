using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.UsersAPI.UnitTests.Mocks;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User;

public class UserTests : APITestBase
{
    [Fact]
    public async Task Should_GetUserAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<GetUser.Handler>>();
        var query = new GetUser.Query(dbEntities.user.Id);
        var handler = new GetUser.Handler(_dbContext, mockLogger.Object);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should()
            .Match<GetUser.User>(o =>
                o.DateOfBirth != null &&
                o.Id == dbEntities.user.Id &&
                o.Address == dbEntities.user.Address &&
                o.FirstName == dbEntities.user.FirstName &&
                o.LastName == dbEntities.user.LastName &&
                o.ImmigrationDataConsent == dbEntities.user.ImmigrationDataConsent &&
                o.JobsDataConsent == dbEntities.user.JobsDataConsent &&
                o.CitizenshipCode == dbEntities.user.CitizenshipCode &&
                o.OccupationCode == dbEntities.user.OccupationCode &&
                o.NativeLanguageCode == dbEntities.user.CitizenshipCode &&
                o.CountryOfBirthCode == dbEntities.user.CountryOfBirthCode &&
                o.Gender == dbEntities.user.Gender &&
                DateOnly.FromDateTime(o.DateOfBirth.Value) == dbEntities.user.DateOfBirth);
        
    }
    
    [Fact]
    public async Task Should_UpdateUserAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new LanguageRepository();
        var command = new UpdateUserCommandBuilder().Build();
        command.SetAuth(dbEntities.user.Id);
        var sut = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should()
            .Match<UpdateUser.User>(o =>
                o.Id == dbEntities.user.Id &&
                o.FirstName == command.FirstName &&
                o.LastName == command.LastName &&
                o.ImmigrationDataConsent == command.ImmigrationDataConsent &&
                o.JobsDataConsent == command.JobsDataConsent &&
                o.CitizenshipCode == command.CitizenshipCode &&
                o.NativeLanguageCode == command.NativeLanguageCode &&
                o.OccupationCode == command.OccupationCode &&
                o.CountryOfBirthCode == command.CountryOfBirthCode &&
                o.Gender == command.Gender);
        
    }
    
    [Fact]
    public async Task Should_FailUpdateUserNationalityCheckAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new LanguageRepository();
        var command = new UpdateUserCommandBuilder().WithCitizenshipCode("not a code").Build();
        command.SetAuth(dbEntities.user.Id);
        var sut = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
    
    [Fact]
    public async Task Should_FailUpdateUserCountryOfBirthAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new LanguageRepository();
        var command = new UpdateUserCommandBuilder().WithCountryOfBirthCode("not a code").Build();
        command.SetAuth(dbEntities.user.Id);
        var sut = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
    
    [Fact]
    public async Task Should_FailUpdateUserNativeLanguageCodeAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new LanguageRepository();
        var command = new UpdateUserCommandBuilder().WithNativeLanguageCode("not a code").Build();
        command.SetAuth(dbEntities.user.Id);
        var sut = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
    
    [Fact]
    public async Task Should_FailUpdateUserOccupationCodeAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new LanguageRepository();
        var command = new UpdateUserCommandBuilder().WithOccupationCode("not a code").Build();
        command.SetAuth(dbEntities.user.Id);
        var sut = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
    
    [Fact]
    public async Task Should_FailUserCheckWithNonExistingUserIdAsync()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new LanguageRepository();
        var command = new UpdateUserCommandBuilder().Build();
        command.SetAuth(Guid.Empty);
        var sut = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);
        
        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_FailUserUpdateWithMaxLengths_FluentValidation()
    {
        // Arrange
        var validator = new UpdateUser.CommandValidator();
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var command = new UpdateUserCommandBuilder()
            .WithFirstName(new string('*', 256))
            .WithLastName(new string('*', 256))
            .WithAddress(new string('*', 256))
            .WithJobsDataConsent(null)
            .WithImmigrationDataConsent(null)
            .WithCountryOfBirthCode("12345678910")
            .WithNativeLanguageCode("12345678910")
            .WithOccupationCode("12345678910")
            .WithCitizenshipCode("12345678910")
            .WithJobTitles(null)
            .WithRegions(null)
            .WithGender((Gender)3)
            .WithDateOfBirth(null)
            .Build();
        command.SetAuth(dbEntities.user.Id);

        // Assert
        var result = validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(user => user.FirstName);
        result.ShouldHaveValidationErrorFor(user => user.LastName);
        result.ShouldHaveValidationErrorFor(user => user.Address);
        result.ShouldHaveValidationErrorFor(user => user.Gender);
        result.ShouldHaveValidationErrorFor(user => user.CountryOfBirthCode);
        result.ShouldHaveValidationErrorFor(user => user.CitizenshipCode);
        result.ShouldHaveValidationErrorFor(user => user.OccupationCode);
        result.ShouldHaveValidationErrorFor(user => user.NativeLanguageCode);
    }
}

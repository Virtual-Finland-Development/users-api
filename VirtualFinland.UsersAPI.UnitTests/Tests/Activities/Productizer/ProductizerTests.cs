using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.UsersAPI.UnitTests.Mocks;
using UpdateUser = VirtualFinland.UserAPI.Activities.Productizer.Operations.UpdateUser;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer;

public class ProductizerTests : APITestBase
{
    [Fact]
    public async void Should_GetUserAsync()
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
    public async void Should_UpdateUserAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockContriesRepository();
        var languageRepository = new LanguageRepository();
        
        var command = new UpdateUser.Command("New FirstName", "New LastName", string.Empty, true, false, "fi", "fi-FI", "01","fi",new List<string>(), new List<string>(), "1", DateTime.Now);
        command.SetAuth(dbEntities.user.Id);
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
                o.CitizenshipCode == command.CitizenshipCode &&
                o.NativeLanguageCode == command.NativeLanguageCode &&
                o.OccupationCode == command.OccupationCode &&
                o.CountryOfBirthCode == command.CountryOfBirthCode &&
                o.Gender == command.Gender);
        
    }
    
    [Fact]
    public async void Should_FailUserUpdateWithMaxLengths_FluentValidation()
    {
        // Arrange
        var validator = new UpdateUser.CommandValidator();
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var command = new UpdateUser.Command(new string('*', 256), new string('*', 256), new string('*', 520),null, null, "12345678910", "12345678910", "12345678910", "12345678910", null, null, "superghumangender123", null);
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
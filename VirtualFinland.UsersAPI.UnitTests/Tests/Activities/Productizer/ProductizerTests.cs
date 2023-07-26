using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.UsersAPI.UnitTests.Mocks;
using VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User.Builder;
using UpdateUserCommandBuilder = VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer.Builder.UpdateUserCommandBuilder;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer;

public class ProductizerTests : APITestBase
{
    [Fact]
    public async Task Should_GetUserAsync()
    {
        // Arrange
        var (user, externalIdentity) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<GetUser.Handler>>();
        var query = new GetUser.Query(user.Id);
        var handler = new GetUser.Handler(_dbContext, mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should()
            .Match<GetUser.User>(o =>
                o.DateOfBirth != null &&
                o.Id == user.Id &&
                o.Address!.StreetAddress == user.AdditionalInformation!.Address!.StreetAddress &&
                o.FirstName == user.GivenName &&
                o.LastName == user.LastName &&
                o.CitizenshipCode == user.AdditionalInformation.CitizenshipCode &&
                o.OccupationCode == user.AdditionalInformation.OccupationCode &&
                o.NativeLanguageCode == user.AdditionalInformation.CitizenshipCode &&
                o.CountryOfBirthCode == user.AdditionalInformation.CountryOfBirthCode &&
                o.Gender == user.AdditionalInformation.Gender &&
                o.DateOfBirth.ToString() == (user.AdditionalInformation.DateOfBirth != null ? DateOnly.Parse(user.AdditionalInformation.DateOfBirth).ToString() : null));
    }

    [Fact]
    public async Task Should_UpdateUserAsync()
    {
        // Arrange
        var (user, externalIdentity) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UpdateUser.Handler>>();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new MockLanguageRepository();
        var command = new UpdateUserCommandBuilder().Build();
        command.SetAuth(user.Id);
        var sut = new UpdateUser.Handler(_dbContext, mockLogger.Object, languageRepository, countryRepository, occupationRepository);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should()
            .Match<UpdateUser.User>(o =>
                o.Id == user.Id &&
                o.FirstName == command.FirstName &&
                o.LastName == command.LastName &&
                o.CitizenshipCode == command.CitizenshipCode &&
                o.NativeLanguageCode == command.NativeLanguageCode &&
                o.OccupationCode == command.OccupationCode &&
                o.CountryOfBirthCode == command.CountryOfBirthCode &&
                o.Gender == command.Gender);

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
            .WithAddress(new AddressBuilder().WithStreetAddress(new string('*', 256)).Build())
            .WithCountryOfBirthCode("12345678910")
            .WithNativeLanguageCode("12345678910")
            .WithOccupationCode("12345678910")
            .WithCitizenshipCode("12345678910")
            .WithJobTitles(null)
            .WithRegions(null)
            .WithGender("Alien")
            .WithDateOfBirth(null)
            .Build();
        command.SetAuth(dbEntities.user.Id);

        // Assert
        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(user => user.FirstName);
        result.ShouldHaveValidationErrorFor(user => user.LastName);
        result.ShouldHaveValidationErrorFor(user => user.Address!.StreetAddress);
        result.ShouldHaveValidationErrorFor(user => user.Gender);
        result.ShouldHaveValidationErrorFor(user => user.CountryOfBirthCode);
        result.ShouldHaveValidationErrorFor(user => user.CitizenshipCode);
        result.ShouldHaveValidationErrorFor(user => user.OccupationCode);
        result.ShouldHaveValidationErrorFor(user => user.NativeLanguageCode);
    }
}

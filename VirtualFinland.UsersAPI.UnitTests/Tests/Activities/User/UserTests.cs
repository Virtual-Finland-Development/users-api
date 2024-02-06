using FluentAssertions;
using FluentValidation.TestHelper;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Models;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.UsersAPI.UnitTests.Mocks;
using VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User.Builder;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User;

public class UserTests : APITestBase
{
    [Fact]
    public async Task Should_GetUserAsync()
    {
        // Arrange
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLoggerFactory = GetMockedAnalyticsLoggerFactory();
        var query = new GetUser.Query(requestAuthenticatedUser);
        var handler = new GetUser.Handler(_dbContext, mockLoggerFactory);

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
                DateOnly.FromDateTime(o.DateOfBirth.Value) == user.AdditionalInformation.DateOfBirth);
    }

    [Fact]
    public async Task Should_UpdateUserAsync()
    {
        // Arrange
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLoggerFactory = GetMockedAnalyticsLoggerFactory();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new MockLanguageRepository();
        var command = new UpdateUserCommandBuilder().Build();
        command.SetAuth(requestAuthenticatedUser);
        var sut = new UpdateUser.Handler(_dbContext, mockLoggerFactory, languageRepository, countryRepository,
            occupationRepository);

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
    public async Task Should_DeleteUserAsync()
    {
        // Arrange
        var (_, _, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var command = new DeleteUser.Command();
        command.SetAuth(requestAuthenticatedUser);

        var mockLoggerFactory = GetMockedAnalyticsLoggerFactory();
        var servicesProvider = GetMockedServiceProvider();
        var personRepository = new PersonRepository(servicesProvider.Object);
        var sut = new DeleteUser.Handler(personRepository, mockLoggerFactory);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType(typeof(MediatR.Unit));
    }

    [Fact]
    public async Task Should_FailUpdateUserNationalityCheckAsync()
    {
        // Arrange
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLoggerFactory = GetMockedAnalyticsLoggerFactory();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new MockLanguageRepository();
        var command = new UpdateUserCommandBuilder().WithCitizenshipCode("not a code").Build();
        command.SetAuth(requestAuthenticatedUser);
        var sut = new UpdateUser.Handler(_dbContext, mockLoggerFactory, languageRepository, countryRepository,
            occupationRepository);

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Should_FailUpdateUserCountryOfBirthAsync()
    {
        // Arrange
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLoggerFactory = GetMockedAnalyticsLoggerFactory();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new MockLanguageRepository();
        var command = new UpdateUserCommandBuilder().WithCountryOfBirthCode("not a code").Build();
        command.SetAuth(requestAuthenticatedUser);
        var sut = new UpdateUser.Handler(_dbContext, mockLoggerFactory, languageRepository, countryRepository,
            occupationRepository);

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Should_FailUpdateUserNativeLanguageCodeAsync()
    {
        // Arrange
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLoggerFactory = GetMockedAnalyticsLoggerFactory();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new MockLanguageRepository();
        var command = new UpdateUserCommandBuilder().WithNativeLanguageCode("not a code").Build();
        command.SetAuth(requestAuthenticatedUser);
        var sut = new UpdateUser.Handler(_dbContext, mockLoggerFactory, languageRepository, countryRepository,
            occupationRepository);

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Should_FailUpdateUserOccupationCodeAsync()
    {
        // Arrange
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLoggerFactory = GetMockedAnalyticsLoggerFactory();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new MockLanguageRepository();
        var command = new UpdateUserCommandBuilder().WithOccupationCode("not a code").Build();
        command.SetAuth(requestAuthenticatedUser);
        var sut = new UpdateUser.Handler(_dbContext, mockLoggerFactory, languageRepository, countryRepository,
            occupationRepository);

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Should_FailUserCheckWithNonExistingUserIdAsync()
    {
        // Arrange
        var mockLoggerFactory = GetMockedAnalyticsLoggerFactory();
        var occupationRepository = new MockOccupationsRepository();
        var countryRepository = new MockCountriesRepository();
        var languageRepository = new MockLanguageRepository();
        var command = new UpdateUserCommandBuilder().Build();
        command.SetAuth(new RequestAuthenticatedUser(Guid.NewGuid()));
        var sut = new UpdateUser.Handler(_dbContext, mockLoggerFactory, languageRepository, countryRepository,
            occupationRepository);

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
        command.SetAuth(dbEntities.requestAuthenticatedUser);

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

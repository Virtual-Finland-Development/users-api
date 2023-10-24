using FluentAssertions;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UsersAPI.UnitTests.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Security;

public class TermsOfServiceAggreementVerifyTests : APITestBase
{
    [Fact]
    public async Task Should_InAuthVerification_ShouldThrowError()
    {
        // Arrange
        await SetupTermsOfServices();
        var (person, _, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var (_, authenticationService, mockHttpContext) = GetGoodLoginRequestSituation(requestAuthenticatedUser, true);

        // Act
        var act = async () => await authenticationService.Authenticate(mockHttpContext.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }

    [Fact]
    public async Task Should_SuccessInAuthVerification()
    {
        // Arrange
        var termsOfService = await SetupTermsOfServices();
        var (person, _, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var (_, authenticationService, mockHttpContext) = GetGoodLoginRequestSituation(requestAuthenticatedUser, true);
        var termsOfServiceRepository = new TermsOfServiceRepository(GetMockedServiceProvider().Object);
        await termsOfServiceRepository.AddNewTermsOfServiceAgreement(termsOfService, person.Id);

        // Act
        var authResult = await authenticationService.Authenticate(mockHttpContext.Object);

        // Assert
        authResult.PersonId.Should().Be(person.Id);
    }
}
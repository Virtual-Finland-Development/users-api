using FluentAssertions;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer.Builder;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer;

// ReSharper disable once InconsistentNaming
public class PersonBasicInformation_UnitTests : APITestBase
{
    [Fact]
    public async Task GetPersonBasicInformation_WithExistingUserId_ReturnsData()
    {
        var (user, _, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var query = new GetPersonBasicInformation.Query(requestAuthenticatedUser);
        var sut = new GetPersonBasicInformation.Handler(_dbContext);

        var actual = await sut.Handle(query, CancellationToken.None);

        actual.Should().NotBeNull();
        actual.GivenName.Should().Match(user.GivenName);
        actual.LastName.Should().Match(user.LastName);
        actual.Email.Should().Match(user.Email);
        actual.PhoneNumber.Should().Match(user.PhoneNumber);
        actual.Residency.Should().Contain(user.ResidencyCode);
    }

    [Fact]
    public async Task UpdateBasicInformation_WithValidData_ReturnsUpdatedData()
    {
        var (_, _, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var command = new UpdatePersonBasicInformationCommandBuilder().Build();
        command.SetAuth(requestAuthenticatedUser);
        var sut = new UpdatePersonBasicInformation.Handler(_dbContext);

        var actual = await sut.Handle(command, CancellationToken.None);

        actual.Should().NotBeNull();

        actual.GivenName.Should().Match(command.GivenName);
        actual.LastName.Should().Match(command.LastName);
        actual.Email.Should().Match(command.Email);
        actual.PhoneNumber.Should().Match(command.PhoneNumber);
        actual.Residency.Should().Contain(command.Residency);
    }
}

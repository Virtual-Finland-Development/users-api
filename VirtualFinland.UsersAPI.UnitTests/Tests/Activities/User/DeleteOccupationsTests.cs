using FluentAssertions;
using VirtualFinland.UserAPI.Activities.User.Occupations.Operations;
using VirtualFinland.UsersAPI.UnitTests.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User;

public class DeleteOccupationsTests : APITestBase
{
    [Fact]
    public async Task TryingToDeleteUserOccupations_WithEmptyIdList_ShouldThrowError()
    {
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var command = new DeleteOccupations.Command(new List<Guid>());
        command.SetAuth(requestAuthenticatedUser);
        var sut = new DeleteOccupations.Handler(_dbContext);

        var act = () => sut.Handle(command, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task TryingToDeleteUserOccupations_WithCorrectId_ShouldNotThrowError()
    {
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext,
            new Guid("c03ed8cb-5aa5-41fe-89ed-f1cfad44e2f6"));
        var command = new DeleteOccupations.Command(new List<Guid> { new("c03ed8cb-5aa5-41fe-89ed-f1cfad44e2f6") });
        command.SetAuth(requestAuthenticatedUser);
        var sut = new DeleteOccupations.Handler(_dbContext);

        var act = () => sut.Handle(command, default);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task TryingToDeleteAllOccupations_ShouldNotThrowError()
    {
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var command = new DeleteOccupations.Command();
        command.SetAuth(requestAuthenticatedUser);
        var sut = new DeleteOccupations.Handler(_dbContext);

        var act = () => sut.Handle(command, default);

        await act.Should().NotThrowAsync();
    }
}

using FluentAssertions;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer.Builder;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer;

// ReSharper disable once InconsistentNaming
public class PersonJobApplicantProfile_UnitTests : APITestBase
{
    [Fact]
    public async Task GetJobApplicantProfile_WithExistingUser_ReturnsData()
    {
        var entities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var query = new GetJobApplicantProfile.Query(entities.user.Id);
        var sut = new GetJobApplicantProfile.Handler(_dbContext);

        var actual = await sut.Handle(query, CancellationToken.None);

        actual.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateJobApplicantProfile_WithValidData_ReturnsUpdatedData()
    {
        var entities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var command = new UpdateJobApplicantProfileCommandBuilder().Build();
        command.SetAuth(entities.user.Id);
        var sut = new UpdateJobApplicantProfile.Handler(_dbContext);

        var actual = await sut.Handle(command, CancellationToken.None);

        actual.Occupations.Should().BeEquivalentTo(command.Occupations);
        actual.Educations.Should().BeEquivalentTo(command.Educations);
        actual.LanguageSkills.Should().BeEquivalentTo(command.LanguageSkills);
        actual.OtherSkills.Should().BeEquivalentTo(command.OtherSkills);
        actual.Certifications.Should().BeEquivalentTo(command.Certifications);
        actual.Permits.Should().BeEquivalentTo(command.Permits);
        actual.WorkPreferences.Should().BeEquivalentTo(command.WorkPreferences);
    }

    [Fact]
    public async Task UpdateJobApplicantProfile_WithInvalidRegionCode_ThrowsError()
    {
        var entities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var command = new UpdateJobApplicantProfileCommandBuilder()
            .WithWorkPreferences(
                new UpdateJobApplicantProfileCommandWorkPreferencesBuilder()
                    .WithRegions(new List<string> { "05" })
                    .Build()
            ).Build();
        command.SetAuth(entities.user.Id);
        var sut = new UpdateJobApplicantProfile.Handler(_dbContext);

        var act = () => sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}

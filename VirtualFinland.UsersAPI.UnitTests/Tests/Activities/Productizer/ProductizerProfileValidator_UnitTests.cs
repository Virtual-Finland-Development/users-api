using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using VirtualFinland.UserAPI.Activities.Productizer;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;
using VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer.Builder;
using Xunit.Abstractions;
using PersonBasicInformationResponse =
    VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation.GetPersonBasicInformation.GetPersonBasicInformationResponse;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer;

// ReSharper disable once InconsistentNaming
public class ProductizerProfileValidator_UnitTests
{
    private readonly ITestOutputHelper _helper;

    public ProductizerProfileValidator_UnitTests(ITestOutputHelper helper)
    {
        _helper = helper;
    }
    
    [Fact]
    public void IsPersonBasicInformationCreated_WithEmailOnly_ShouldReturnTrue()
    {
        var response = new PersonBasicInformationResponse(null, null, "email@email.email", null, null);

        var actual = ProductizerProfileValidator.IsPersonBasicInformationCreated(response);

        actual.Should().BeTrue();
    }

    [Fact]
    public void IsPersonBasicInformationCreated_WithEverythingButEmail_ShouldReturnFalse()
    {
        var response = new PersonBasicInformationResponse(
            Arg.Any<string>(),
            Arg.Any<string>(),
            null,
            Arg.Any<string>(),
            Arg.Any<string>()
        );

        var actual = ProductizerProfileValidator.IsPersonBasicInformationCreated(response);

        actual.Should().BeFalse();
    }

    [Fact]
    public void IsPersonBasicInformationCreated_WithNoData_ShouldReturnFalse()
    {
        var response = new PersonBasicInformationResponse(null, null, null, null, null);

        var actual = ProductizerProfileValidator.IsPersonBasicInformationCreated(response);

        actual.Should().BeFalse();
    }

    [Fact]
    public void IsPersonBasicInformationCreated_WithEmptyEmail_ShouldReturnFalse()
    {
        var response = new PersonBasicInformationResponse(
            Arg.Any<string>(),
            Arg.Any<string>(),
            "",
            Arg.Any<string>(),
            Arg.Any<string>()
        );

        var actual = ProductizerProfileValidator.IsPersonBasicInformationCreated(response);

        actual.Should().BeFalse();
    }

    [Fact]
    public void IsPersonBasicInformationCreated_WithEmptyNameButValidEmail_ShouldReturnTrue()
    {
        var response = new PersonBasicInformationResponse(
            "",
            Arg.Any<string>(),
            "email@email.email",
            Arg.Any<string>(),
            Arg.Any<string>()
        );

        var actual = ProductizerProfileValidator.IsPersonBasicInformationCreated(response);

        actual.Should().BeTrue();
    }

    [Fact]
    public void IsJobApplicantProfileCreated_WithValidValues_ShouldReturnTrue()
    {
        var response = new PersonJobApplicantProfileResponseBuilder().Build();

        var actual = ProductizerProfileValidator.IsJobApplicantProfileCreated(response);

        actual.Should().BeTrue();
        _helper.WriteLine(JsonSerializer.Serialize(response));
    }

    [Fact]
    public void IsJobApplicantProfileCreated_OnlyRequiredData_ShouldReturnTrue()
    {
        var response = new PersonJobApplicantProfileResponseBuilder()
            .WithCertifications(new List<PersonJobApplicantProfileResponse.Certification>
                { new(null, new List<string> { "xyz" }, null) })
            .WithWorkPreferences(
                new PersonJobApplicantProfileResponseWorkPreferencesBuilder()
                    .WithNaceCode(null)
                    .WithEmploymentType(null)
                    .WithWorkingTime(null)
                    .Build())
            .Build();
        
        var actual = ProductizerProfileValidator.IsJobApplicantProfileCreated(response);

        actual.Should().BeTrue();
        _helper.WriteLine(JsonSerializer.Serialize(response));
    }

    [Fact]
    public void IsJobApplicantProfileCreated_WithNoActualData_ShouldReturnFalse()
    {
        var response = new PersonJobApplicantProfileResponseBuilder()
            .WithWorkPreferences(new PersonJobApplicantProfileResponseWorkPreferencesBuilder()
                .WithNaceCode(null)
                .WithPreferredMunicipalities(new List<string>())
                .WithPreferredRegions(new List<string>())
                .WithWorkingLanguage(new List<string>())
                .WithWorkingTime(null)
                .WithEmploymentType(null)
                .Build())
            .Build();
        
        var actual = ProductizerProfileValidator.IsJobApplicantProfileCreated(response);

        actual.Should().BeFalse();
        _helper.WriteLine(JsonSerializer.Serialize(response));
    }
}

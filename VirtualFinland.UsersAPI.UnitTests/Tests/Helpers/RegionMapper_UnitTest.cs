using FluentAssertions;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Helpers;

// ReSharper disable once InconsistentNaming
public class RegionMapper_UnitTest
{
    [Fact]
    public void TryingToMapRegion_WithIsoCode_ShouldReturnCodeSetValue()
    {
        var actual = RegionMapper.FromIso_3166_2_ToCodeSet("FI-01");

        actual.Should<string>().Be("21");
    }

    [Theory]
    [InlineData("FI-99")]
    [InlineData("")]
    [InlineData(null)]
    public void TryingToMapRegion_WithInvalidIsoCode_ShouldThrowError(string value)
    {
        var act = () => RegionMapper.FromIso_3166_2_ToCodeSet(value);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TryingToMapRegion_WithCodeSetValue_ShouldReturnIsoCode()
    {
        var actual = RegionMapper.FromCodeSetToIso_3166_2("21");

        actual.Should<string>().Be("FI-01");
    }

    [Theory]
    [InlineData("99")]
    [InlineData("")]
    [InlineData(null)]
    public void TryingToMapRegion_WithInvalidCodeSetValue_ShouldThrowError(string value)
    {
        var act = () => RegionMapper.FromCodeSetToIso_3166_2(value);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

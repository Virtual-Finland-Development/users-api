using VirtualFinland.UserAPI.Models.Shared;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User.Builder;

internal class AddressBuilder
{
    private string? _streetAddress = "Mannerheimintie 42";
    private readonly string? _zipCode = "42322";
    private readonly string? _city = "Helsinki"; 
    private readonly string? _country = "Finland";

    public Address Build()
    {
        return new Address(_streetAddress, _zipCode, _city, _country);
    }

    public AddressBuilder WithStreetAddress(string value)
    {
        _streetAddress = value;
        return this;
    }
}

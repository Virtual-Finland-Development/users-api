using VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer.Builder;

public class UpdatePersonBasicInformationCommandBuilder
{
    private readonly string _email = "john.doe@example.com";
    private readonly string? _givenName = "John";
    private readonly string? _lastName = "Doe";
    private readonly string? _phoneNumber = "+35844123456789";
    private string _residency = "FIN";

    public UpdatePersonBasicInformation.Command Build()
    {
        return new UpdatePersonBasicInformation.Command(
            _givenName,
            _lastName,
            _email,
            _phoneNumber,
            _residency
        );
    }

    public UpdatePersonBasicInformationCommandBuilder WithResidencyCode(string value)
    {
        _residency = value;
        return this;
    }
}

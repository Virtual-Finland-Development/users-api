using FluentAssertions;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Security;

public class CryptoTests
{
    [Fact]
    public void Should_SucceedInEncryption()
    {
        // Arrange
        var secrets = new DatabaseEncryptionSecrets("12345678901234567890123456789012", "1234567890123456");
        var cryptoUtility = new CryptoUtility(secrets);

        // Act
        var plaintext = "test";
        var encrypted = cryptoUtility.Encrypt(plaintext, "password");
        var decrypted = cryptoUtility.Decrypt(encrypted, "password");
        var decryptedBadAct = () => cryptoUtility.Decrypt(encrypted, "password2");

        // Assert
        decrypted.Should().Be(plaintext);
        decryptedBadAct.Should().Throw<NotAuthorizedException>();
    }
}

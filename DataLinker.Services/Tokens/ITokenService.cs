
namespace DataLinker.Services.Tokens
{
    public interface ITokenService
    {
        TokenInfo GenerateEmailConfirmationToken(int receiverId);

        TokenInfo ParseEmailConfirmationToken(string token);

        TokenInfo GenerateLicenseVerificationToken(int receiverId, int licenseId);

        TokenInfo ParseLicenseVerificationToken(string token);

        TokenInfo GenerateConsumerProviderRegistrationToken(int receiverId, int licenseId, int consumerProviderRegistrationId);

        TokenInfo ParseConsumerProviderRegistrationToken(string token);
    }
}

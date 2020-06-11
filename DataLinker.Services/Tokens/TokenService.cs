using DataLinker.Services.Configuration;
using DataLinker.Services.Exceptions;
using System;
using System.Linq;

namespace DataLinker.Services.Tokens
{
    internal class TokenService: ITokenService
    {
        private readonly int _emailExpireAfter;
        private readonly int _approvalLinkExpiresIn;
        private DateTime GetDate => DateTime.UtcNow;

        public TokenService(IConfigurationService config)
        {
            _emailExpireAfter = int.Parse(config.ConfirmNewEmailExpires);
            _approvalLinkExpiresIn = int.Parse(config.ApprovalLinkExpiresIn);
        }

        public TokenInfo GenerateEmailConfirmationToken(int receiverId)
        {
            // Setup expiration date
            var expirationDate = DateTime.Now.AddHours(_emailExpireAfter);

            // Remove ticks
            expirationDate = expirationDate.AddTicks(-(expirationDate.Ticks % TimeSpan.TicksPerSecond));
            var id = BitConverter.GetBytes(receiverId);
            var time = BitConverter.GetBytes(expirationDate.ToBinary());
            var guid = Guid.NewGuid();

            // Concatinate generated data for activation link
            var idWithTime = id.Concat(time);
            var byteArray = idWithTime.Concat(guid.ToByteArray()).ToArray();

            // Encode token
            var encodedToken = ToHexString(byteArray);

            // Setup result
            var result = new TokenInfo
            {
                Token = guid.ToString(),
                ReceiverId = receiverId,
                TokenExpire = expirationDate,
                TokenInfoEncoded = encodedToken
            };

            return result;
        }

        public TokenInfo ParseEmailConfirmationToken(string token)
        {
            // Decode token
            var byteArray = ToByteArray(token);

            // Get concatenated details
            var id = BitConverter.ToInt32(byteArray, 0);
            var time = BitConverter.ToInt64(byteArray, 4);
            var guid = new Guid(byteArray.Skip(12).ToArray()).ToString();

            // Setup result
            var result = new TokenInfo
            {
                Token = guid,
                ReceiverId = id,
                TokenExpire = DateTime.FromBinary(time),
                TokenInfoEncoded = token
            };

            return result;
        }

        public TokenInfo GenerateLicenseVerificationToken(int receiverId, int licenseId)
        {
            var expirationDate = GetDate.AddHours(_approvalLinkExpiresIn);

            // Remove ticks
            expirationDate = expirationDate.AddTicks(-(expirationDate.Ticks % TimeSpan.TicksPerSecond));

            // Generate data
            var licensid = BitConverter.GetBytes(licenseId);
            var userId = BitConverter.GetBytes(receiverId);
            var time = BitConverter.GetBytes(expirationDate.ToBinary());
            var guid = Guid.NewGuid();

            // Concatinate generated data
            var licenseWithUserId = licensid.Concat(userId);
            var idWithTime = licenseWithUserId.Concat(time);
            var concatinatedByteArray = idWithTime.Concat(guid.ToByteArray()).ToArray();

            // Encode token
            var encodedToken = ToHexString(concatinatedByteArray);

            // Setup result
            var result = new TokenInfo
            {
                Token = guid.ToString(),
                ReceiverId = receiverId,
                TokenExpire = expirationDate,
                TokenInfoEncoded = encodedToken,
                ID = licenseId
            };

            return result;
        }

        public TokenInfo ParseLicenseVerificationToken(string token)
        {
            // Decode token
            var byteArray = ToByteArray(token);

            // Get concatenated details
            var licenseId = BitConverter.ToInt32(byteArray, 0);
            var userId = BitConverter.ToInt32(byteArray, 4);
            var expire = DateTime.FromBinary(BitConverter.ToInt64(byteArray, 8));
            var guid = new Guid(byteArray.Skip(16).ToArray()).ToString();

            // Setup result
            var result = new TokenInfo
            {
                Token = guid,
                ReceiverId = userId,
                TokenExpire = expire,
                TokenInfoEncoded = token,
                ID = licenseId
            };

            return result;
        }
        
        private static string ToHexString(byte[] bytes, bool useLowerCase = false)
        {
            var hex = string.Concat(bytes.Select(b => b.ToString(useLowerCase ? "x2" : "X2")));

            return hex;
        }

        public static byte[] ToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public TokenInfo GenerateConsumerProviderRegistrationToken(int receiverId, int licenseId, int consumerProviderRegistrationId)
        {
            var expirationDate = GetDate.AddHours(_approvalLinkExpiresIn);

            // Remove ticks
            expirationDate = expirationDate.AddTicks(-(expirationDate.Ticks % TimeSpan.TicksPerSecond));

            // Generate data
            var licensid = BitConverter.GetBytes(licenseId);
            var registrationId = BitConverter.GetBytes(consumerProviderRegistrationId);
            var userId = BitConverter.GetBytes(receiverId);
            var time = BitConverter.GetBytes(expirationDate.ToBinary());
            var guid = Guid.NewGuid();

            // Concatinate generated data

            var licenseWithUserId = licensid.Concat(registrationId).Concat(userId);
            var idWithTime = licenseWithUserId.Concat(time);
            var concatinatedByteArray = idWithTime.Concat(guid.ToByteArray()).ToArray();

            // Encode token
            var encodedToken = ToHexString(concatinatedByteArray);

            // Setup result
            var result = new TokenInfo
            {
                Token = guid.ToString(),
                ReceiverId = receiverId,
                TokenExpire = expirationDate,
                TokenInfoEncoded = encodedToken,
                ID = licenseId,
                ConsumerProviderRegistrationId = consumerProviderRegistrationId
            };

            return result;
        }

        public TokenInfo ParseConsumerProviderRegistrationToken(string token)
        {
            // Decode token
            var byteArray = ToByteArray(token);

            // Get concatenated details
            var licenseId = BitConverter.ToInt32(byteArray, 0);
            var consumerProviderRegistrationId = BitConverter.ToInt32(byteArray, 4);
            var userId = BitConverter.ToInt32(byteArray, 8);
            var expire = DateTime.FromBinary(BitConverter.ToInt64(byteArray, 12));
            var guid = new Guid(byteArray.Skip(20).ToArray()).ToString();

            // Setup result
            var result = new TokenInfo
            {
                Token = guid,
                ReceiverId = userId,
                TokenExpire = expire,
                TokenInfoEncoded = token,
                ID = licenseId,
                ConsumerProviderRegistrationId = consumerProviderRegistrationId
            };

            return result;
        }
    }
}

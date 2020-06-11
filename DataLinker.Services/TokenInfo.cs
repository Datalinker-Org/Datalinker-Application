using System;

namespace DataLinker.Services
{
    public class TokenInfo
    {
        public string Token { get; set; }

        public DateTime? TokenExpire { get; set; }

        public string TokenInfoEncoded { get; set; }

        public int? ID { get; set; }

        public int? ReceiverId { get; set; }

        public int? ConsumerProviderRegistrationId { get; set; }
    }
}
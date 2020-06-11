using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("LicenseApprovalRequests")]
    public class LicenseApprovalRequest : IObject<int>
    {
        public int OrganizationLicenseID { get; set; }

        public DateTime SentAt { get; set; }

        public int SentTo { get; set; }

        public int SentBy { get; set; }

        public DateTime ExpiresAt { get; set; }

        public string accessToken { get; set; }
        // TODO difference between tokens - rename
        public string Token { get; set; }

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }

        [Ignore, NotMapped]
        public string KeyName => "ID";
    }
}
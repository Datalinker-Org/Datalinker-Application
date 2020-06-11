using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("LicenseAgreements")]
    public class LicenseAgreement : IObject<int>
    {
        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? ExpiresAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int ConsumerProviderRegistrationId { get; set; }

        [Ignore(IgnoreType.Update)]
        public string SoftwareStatement { get; set; }

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }

        [Ignore, NotMapped]
        public string KeyName => "ID";
    }
}
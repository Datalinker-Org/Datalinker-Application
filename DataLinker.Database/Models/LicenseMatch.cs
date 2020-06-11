using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("LicenseMatches")]
    public class LicenseMatch: IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }

        public int ConsumerLicenseID { get; set; }

        public int ProviderLicenseID { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }
    }
}

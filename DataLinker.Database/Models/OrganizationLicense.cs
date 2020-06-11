using System;
using System.ComponentModel.DataAnnotations.Schema;
using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Database.Models
{
    [TableName("OrganizationLicenses")]
    public class OrganizationLicense: IObject<int>
    {
        public int? LicenseTemplateID { get; set; }

        public int ProviderEndpointID { get; set; }

        public int ApplicationID { get; set; }

        public int? CustomLicenseID { get; set; }

        public int Status { get; set; }

        public int DataSchemaID { get; set; }
        
        [Ignore(IgnoreType.Insert)]
        public DateTime? ApprovedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? ApprovedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? PublishedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? PublishedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? UpdatedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? UpdatedBy { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }

        [Ignore(IgnoreType.Insert | IgnoreType.Update)]
        public int ID { get; set; }

        [Ignore, NotMapped]
        public string KeyName => "ID";
    }
}

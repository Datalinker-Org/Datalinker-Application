using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("ConsumerProviderRegistrations")]
    public class ConsumerProviderRegistration : IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore(IgnoreType.Insert | IgnoreType.Update)]
        public int ID { get; set; }

        public int ConsumerApplicationID { get; set; }

        public int OrganizationLicenseID { get; set; }

        public int Status { get; set; }

        public string Remarks { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? ApprovedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? ApprovedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? ProviderApprovedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? ProviderApprovedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? DeclinedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? DeclinedBy { get; set; }
    }
}

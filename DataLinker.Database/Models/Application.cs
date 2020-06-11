using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("Applications")]
    public class Application : IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }

        public int OrganizationID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid PublicID { get; set; }

        public bool IsProvider { get; set; }

        public bool IsIntroducedAsIndustryGood { get; set; }

        public bool IsVerifiedAsIndustryGood { get; set; }

        public bool IsActive { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? UpdatedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? UpdatedBy { get; set; }
    }
}

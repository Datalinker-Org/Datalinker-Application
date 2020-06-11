using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("ProviderEndpoints")]
    public class ProviderEndpoint : IObject<int>
    {
        public bool IsActive { get; set; }

        public string ServiceUri { get; set; }

        public bool IsIndustryGood { get; set; }

        public string Description { get; set; }

        public  string Name { get; set; }

        [Ignore(IgnoreType.Update)]
        public int ApplicationId { get; set; }

        [Ignore(IgnoreType.Update)]
        public int DataSchemaID { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? UpdatedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? UpdatedBy { get; set; }

        [Ignore(IgnoreType.Insert | IgnoreType.Update)]
        public int ID { get; set; }

        [Ignore, NotMapped]
        public string KeyName => "ID";
    }
}
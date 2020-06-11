using System;
using System.ComponentModel.DataAnnotations.Schema;
using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Database.Models
{
    [TableName("DataSchemas")]
    public class DataSchema : IObject<int>
    {
        public string PublicID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int? CreatedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? UpdatedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? UpdatedBy { get; set; }

        public int Version { get; set; }

        public bool IsIndustryGood { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? PublishedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? PublishedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? RetractedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? RetractedBy { get; set; }
        
        public int Status { get; set; }
        public bool IsAggregate { get; set; }

        [Ignore(IgnoreType.Insert | IgnoreType.Update)]
        public int ID { get; set; }

        [Ignore, NotMapped]
        public string KeyName => "ID";

        public string GetFormattedName()
        {
            if (IsAggregate && !Name.Contains("aggregate") && !Name.Contains("Aggregate"))
            {
                return Name + " (aggregate)";
            }

            return Name;
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Database.Models
{
    [TableName("LicenseClauseTemplates")]
    public class LicenseClauseTemplate : IObject<int>
    {
        public int LicenseClauseID { get; set; }

        public string ShortText { get; set; }

        public string Description { get; set; }

        public string LegalText { get; set; }
        
        public int Version { get; set; }

        public int Status { get; set; }

        public int ClauseType { get; set; }
        
        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? UpdatedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? UpdatedBy { get; set; }

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }

        [Ignore, NotMapped]
        public string KeyName => "ID";
    }
}
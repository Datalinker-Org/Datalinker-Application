using System;
using System.ComponentModel.DataAnnotations.Schema;
using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Database.Models
{
    [TableName("LicenseTemplates")]
    public class LicenseTemplate : IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }
        public int LicenseID { get; set; }
        public int Version { get; set; }
        
        public string Description { get; set; }

        public string LicenseText { get; set; }

        public string Name { get; set; }

        public int Status { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? UpdatedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? UpdatedBy { get; set; }
        
        //[Ignore]
        //public bool IsDraft => Status == (int)TemplateStatus.Draft;

        //[Ignore]
        //public bool IsActive => Status == (int)TemplateStatus.Active;

        //[Ignore]
        //public bool IsRetracted => Status == (int)TemplateStatus.Retracted;
    }
}

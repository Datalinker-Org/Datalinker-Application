using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("SoftwareStatements")]
    public class SoftwareStatement : IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore(IgnoreType.Insert | IgnoreType.Update)]
        public int ID { get; set; }

        public int ApplicationID { get; set; }

        public string Content { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }

        [Ignore(IgnoreType.Insert)]
        public DateTime? ExpiredAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? ExpiredBy { get; set; }
    }
}

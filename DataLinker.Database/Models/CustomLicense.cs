using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("CustomLicenses")]
    public class CustomLicense: IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore(IgnoreType.Insert | IgnoreType.Update)]
        public int ID { get; set; }

        public string FileName { get; set; }

        public byte[] Content { get; set; }

        public string MimeType { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }
    }
}

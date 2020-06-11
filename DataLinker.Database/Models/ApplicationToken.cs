using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("ApplicationTokens")]
    public class ApplicationToken : IObject<int>
    {
        public int ApplicationID { get; set; }

        public string OriginHost { get; set; }

        public string Token { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? ExpiredAt { get; set; }

        public int? ExpiredBy { get; set; }

        [Ignore]
        public bool IsExpired => ExpiredAt < DateTime.Now;

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }

        [Ignore, NotMapped]
        public string KeyName => "ID";
    }
}
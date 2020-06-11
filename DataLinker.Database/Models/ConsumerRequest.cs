using System;
using System.ComponentModel.DataAnnotations.Schema;
using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Database.Models
{
    [TableName("ConsumerRequests")]
    public class ConsumerRequest : IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }

        public int ProviderID { get; set; }

        public int ConsumerID { get; set; }

        public int DataSchemaID { get; set; }
        
        public int Status { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }
        
        [Ignore(IgnoreType.Insert)]
        public DateTime? ProcessedAt { get; set; }

        [Ignore(IgnoreType.Insert)]
        public int? ProcessedBy { get; set; }
    }
}

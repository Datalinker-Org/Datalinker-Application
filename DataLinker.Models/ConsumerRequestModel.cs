using System;

namespace DataLinker.Models
{
    public class ConsumerRequestModel
    {
        public int Id { get; set; }

        public string SchemaName { get; set; }

        public string ConsumerName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
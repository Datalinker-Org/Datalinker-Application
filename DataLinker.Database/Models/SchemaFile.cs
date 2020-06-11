using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Database.Models
{
    [TableName("SchemaFiles")]
    public class SchemaFile : IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore(IgnoreType.Insert | IgnoreType.Update)]
        public int ID { get; set; }
        
        public int DataSchemaID { get; set; }
        public string SchemaText { get; set; }
        public string FileFormat { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        
        public static string BuildFileName(DataSchema metaData) {
            return metaData.Name + "_v" + metaData.Version;
        }
    }
}

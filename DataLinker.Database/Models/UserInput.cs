using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("UserInputs")]
    public class UserInput : IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore(IgnoreType.Insert | IgnoreType.Update)]
        public int ID { get; set; }

        public int UserID { get; set; }

        public string LicenseSelection { get; set; }

        public int DataSchemaId { get; set; }
    }
}

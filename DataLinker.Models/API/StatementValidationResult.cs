using System.Collections.Generic;

namespace DataLinker.Models
{
    public class StatementValidationResult
    {
        public List<SchemaValidationResult> schemas { get; set; }

        public bool isSuccessfull { get; set; }

        public string error_description { get; set; }
    }

    public class SchemaValidationResult
    {
        public string publicId { get; set; }

        public bool isValid { get; set; }

        public string error_description { get; set; }
    }
}
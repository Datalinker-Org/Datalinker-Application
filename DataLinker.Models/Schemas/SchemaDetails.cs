namespace DataLinker.Models
{
    /// <summary>
    ///     Schema details that describes it.
    /// </summary>
    public class SchemaDetails
    {
        public SchemaDetails() { }
        
        public string public_id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public bool is_aggregate { get; set; }
    }

    public class SoftwareStatementSchema : SchemaDetails
    {
        public int licenseId { get; set; }
    }
}
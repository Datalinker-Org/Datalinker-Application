namespace DataLinker.Models
{
    public class LicenseDetails
    {
        public string software_statement { get; set; }

        /// <summary>
        /// Space separated publicIds of accepted schemas
        /// </summary>
        public string accepted_schemas { get; set; }
    }
}
namespace DataLinker.Services.Emails.Models
{
    public class LegalOfficerApprovedLicenseEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string OrganizationName { get; set; }

        public string SchemaName { get; set; }

        public bool IsProvider { get; set; }
    }
}
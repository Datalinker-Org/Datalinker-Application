namespace DataLinker.Services.Emails.Models
{
    public class LegalOfficerVerificationLicenseEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string OrganizationName { get; set; }

        public string SchemaName { get; set; }

        public bool IsProvider { get; set; }

        public string LinkToConfirmationScreen { get; set; }
    }
}
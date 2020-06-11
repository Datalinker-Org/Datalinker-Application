namespace DataLinker.Services.Emails.Models
{
    public class NewProviderLicenseEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string OrgName { get; set; }

        public string SchemaName { get; set; }

        public string LinkToLicense { get; set; }
    }
}
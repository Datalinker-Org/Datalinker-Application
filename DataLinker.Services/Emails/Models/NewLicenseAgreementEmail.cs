namespace DataLinker.Services.Emails.Models
{
    public class NewLicenseAgreementEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string OrgNameConsumer { get; set; }

        public string OrgNameProvider { get; set; }

        public string SchemaName { get; set; }

        public string LinkToLicense { get; set; }
    }
}
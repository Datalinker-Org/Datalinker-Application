namespace DataLinker.Services.Emails.Models.ConsumerProviderRegistrations
{
    public class ConsumerLegalApprovalEmail : CommonEmailProperties
    {
        public string Name { get; set; }
        public string ConsumerOrganizationName { get; set; }
        public string ProviderOrganizationName { get; set; }
        public string SchemaName { get; set; }
        public string LinkToConfirmationScreen { get; set; }
    }
}
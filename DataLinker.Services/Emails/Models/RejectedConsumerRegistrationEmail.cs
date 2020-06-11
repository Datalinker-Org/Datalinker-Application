namespace DataLinker.Services.Emails.Models
{
    public class RejectedConsumerRegistrationEmail : CommonEmailProperties
    {
        public string Name { get; set; }
        public string ProviderName { get; set; }
        public string SchemaName { get; set; }
    }
}
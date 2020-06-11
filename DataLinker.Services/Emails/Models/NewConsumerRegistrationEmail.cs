namespace DataLinker.Services.Emails.Models
{
    public class NewConsumerRegistrationEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string UrlToConsumerRequests { get; set; }
    }
}
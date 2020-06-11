namespace DataLinker.Services.Emails.Models
{
    public class SoftwareStatementUpdatedEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string OrganizationName { get; set; }
        
        public string UrlToApplicationDetails { get; set; }
    }
}
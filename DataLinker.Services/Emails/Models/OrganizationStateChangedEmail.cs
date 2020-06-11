namespace DataLinker.Services.Emails.Models
{
    public class OrganizationStateChangedEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string OrganizationName { get; set; }
        
        public string State { get; set; }
    }
}
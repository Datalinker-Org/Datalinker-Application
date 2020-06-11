namespace DataLinker.Services.Emails.Models
{
    public class NewLegalOfficerEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string OrganizationName { get; set; }
    }
}
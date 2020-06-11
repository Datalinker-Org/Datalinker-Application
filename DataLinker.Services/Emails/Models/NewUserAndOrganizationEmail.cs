namespace DataLinker.Services.Emails.Models
{
    public class NewUserAndOrganizationEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string UserName { get; set; }

        public string OrgName { get; set; }

        public string UserDetailsLink { get; set; }

        public string OrgDetailsLink { get; set; }
    }
}
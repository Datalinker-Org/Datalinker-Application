namespace DataLinker.Services.Emails.Models
{
    public class UserVerificationEmail : CommonEmailProperties
    {
        public string Name { get; set; }
        public string VerificationLink { get; set; }
        public string OrgName { get; set; }
    }
}
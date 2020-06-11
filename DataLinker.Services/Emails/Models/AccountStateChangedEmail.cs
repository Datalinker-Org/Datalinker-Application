namespace DataLinker.Services.Emails.Models
{
    public class AccountStateChangedEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string State { get; set; }
    }
}
namespace DataLinker.Services.Emails.Models
{
    public class IndustryGoodApplicationEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string OrgName { get; set; }

        public string DetailsLink { get; set; }
    }
}
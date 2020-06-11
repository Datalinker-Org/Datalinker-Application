namespace DataLinker.Services.Emails.Models
{
    public class ClauseTemplatePublishedEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string ClauseDescription { get; set; }
    }
}
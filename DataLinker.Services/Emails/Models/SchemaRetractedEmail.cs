namespace DataLinker.Services.Emails.Models
{
    public class SchemaRetractedEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string SchemaName { get; set; }
    }
}
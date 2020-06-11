using Postal;

namespace DataLinker.Services.Emails.Models
{
    public class CommonEmailProperties : Email
    {
        public string To { get; set; }
        public string CC { get; set; }
        public string Bcc { get; set; }
        public string From { get; set; }
        public string UrlToImageInEmail { get; set; }
        public string UrlToStylesFile { get; set; }
        public string DataLinkerHost { get; set; }
        public string DataLinkerEmail { get; set; }
    }
}
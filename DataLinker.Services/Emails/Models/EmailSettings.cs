namespace DataLinker.Services.Emails.Models
{
    public class EmailSettings : IEmailSettings
    {
        public string SmtpServer { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPort { get; set; }
        public bool SmtpUseDefault { get; set; }
        public bool SmtpUseSsl { get; set; }
        public string SmtpPassword { get; set; }
        public string SmtpFromName { get; set; }
        public string SmtpFromEmail { get; set; }
        public bool SendEmail { get; set; }
        public string From
        {
            get
            {
                return string.IsNullOrWhiteSpace(SmtpFromName) ? SmtpFromEmail : $"{SmtpFromName} <{SmtpFromEmail}>";
            }
        }

        public string PathToMail { get; set; }
    }
}
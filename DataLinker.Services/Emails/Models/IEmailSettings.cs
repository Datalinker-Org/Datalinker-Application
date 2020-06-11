namespace DataLinker.Services.Emails.Models
{
    public interface IEmailSettings
    {
        string SmtpServer { get; set; }
        string SmtpUser { get; set; }
        string SmtpPort { get; set; }
        bool SmtpUseDefault { get; set; }
        bool SmtpUseSsl { get; set; }
        string SmtpPassword { get; set; }
        string SmtpFromName { get; set; }
        string SmtpFromEmail { get; set; }
        bool SendEmail { get; set; }
        string From { get; }
        string PathToMail { get; set; }
    }
}

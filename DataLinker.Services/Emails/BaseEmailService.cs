using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using DataLinker.Services.Emails.Models;
using Postal;

namespace DataLinker.Services.Emails
{
    public abstract class BaseEmailService
    {
        protected readonly IEmailSettings _emailSettings;
        private readonly IEmailService _emailService;
        private readonly string _emailDumpPath;
        protected const string MailFileFormat = ".pdf";
        private string DataLinkerHost { get; }
        private string UrlToCssFile { get; }
        private string UrlForImageInEmail { get; }
        private string DataLinkerContactEmail { get; }

        public BaseEmailService(IEmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
            var viewEngines = new ViewEngineCollection();
            var viewsPath = Path.Combine(ConfigurationManager.AppSettings["PathToEmailViews"]);
            viewEngines.Add(new FileRazorViewEngine(viewsPath));
            _emailService = new EmailService(viewEngines, BuildSmtpClient);
            _emailDumpPath = $"{AppDomain.CurrentDomain.BaseDirectory}{emailSettings.PathToMail}";
            Directory.CreateDirectory(_emailDumpPath);
            // todo : move all configuration to a ninject
            // todo: move to config
            DataLinkerHost = ConfigurationManager.AppSettings["DataLinkerHost"];
            UrlToCssFile = ConfigurationManager.AppSettings["UrlToCSSEmail"];
            UrlForImageInEmail = ConfigurationManager.AppSettings["UrlToImageInEmail"];
            DataLinkerContactEmail = ConfigurationManager.AppSettings["DataLinkerContactEmail"];
        }

        public bool Send(CommonEmailProperties email)
        {
            email.DataLinkerEmail = DataLinkerContactEmail;
            email.UrlToImageInEmail = UrlForImageInEmail;
            email.DataLinkerHost = DataLinkerHost;
            email.UrlToStylesFile = UrlToCssFile;

            if (_emailSettings.SendEmail)
            {
                _emailService.Send(email);
            }
            else
            {
                var emailMessage = _emailService.CreateMailMessage(email);
                var smtp = new SmtpClient();
                smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                smtp.PickupDirectoryLocation = _emailDumpPath;
                smtp.Send(emailMessage);
            }
            return true;
        }

        private SmtpClient BuildSmtpClient()
        {
            int portNumber = 0;
            if (!Int32.TryParse(_emailSettings.SmtpPort, out portNumber))
            {
                //use default smtp port
                portNumber = 25;
            }

            //setup the SMTP client
            SmtpClient smtpClient = new SmtpClient(_emailSettings.SmtpServer);

            if (_emailSettings.SmtpUseDefault)
            {
                smtpClient.UseDefaultCredentials = true;
            }
            else
            {
                smtpClient.UseDefaultCredentials = false;
                var sendingCredential = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
                smtpClient.Credentials = sendingCredential;
            }

            
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Port = portNumber;
            smtpClient.EnableSsl = _emailSettings.SmtpUseSsl;

            return smtpClient;
        }
    }
}
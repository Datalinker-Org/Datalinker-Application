using DataLinker.Services.Emails.Models;
using DataLinker.Services.Emails.Roles.Admin;
using DataLinker.Services.Emails.Roles.LegalOfficer;
using DataLinker.Services.Emails.Roles.User;
using DataLinker.Services.Emails.Services;

namespace DataLinker.Services.Emails
{
    public class EmailNotificationService : BaseEmailService, INotificationService
    {
        public EmailNotificationService(IEmailSettings emailSettings,
            IAdminNotificationService adminNotificationService,
            IUserNotificationService userNotificationService,
            ILegalOfficerNotificationService legalOfficerNotificationService,
            IConsumerProviderRegistrationNotificatonService consumerProviderRegistrationNotificationService)
            : base(emailSettings)
        {
            Admin = adminNotificationService;
            User = userNotificationService;
            LegalOfficer = legalOfficerNotificationService;
            ConsumerProviderRegistration = consumerProviderRegistrationNotificationService;
        }

        public IUserNotificationService User { get; set; }

        public ILegalOfficerNotificationService LegalOfficer { get; set; }

        public IAdminNotificationService Admin { get; set; }

        public IConsumerProviderRegistrationNotificatonService ConsumerProviderRegistration { get; set; }
        
    }
}
using DataLinker.Services.Emails.Roles.Admin;
using DataLinker.Services.Emails.Roles.LegalOfficer;
using DataLinker.Services.Emails.Roles.User;
using DataLinker.Services.Emails.Services;

namespace DataLinker.Services.Emails
{
    public interface INotificationService
    {
        IUserNotificationService User { get; set; }
        ILegalOfficerNotificationService LegalOfficer { get; set; }
        IAdminNotificationService Admin { get; set; }
        IConsumerProviderRegistrationNotificatonService ConsumerProviderRegistration { get; set; }
    }
}

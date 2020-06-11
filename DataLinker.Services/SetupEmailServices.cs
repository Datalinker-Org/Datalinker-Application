using DataLinker.Services.Emails;
using DataLinker.Services.Emails.Roles.Admin;
using DataLinker.Services.Emails.Roles.LegalOfficer;
using DataLinker.Services.Emails.Roles.User;
using DataLinker.Services.Emails.Services;
using Ninject;

namespace DataLinker.Services
{
    public static class SetupEmailServices
    {
        public static void RegisterNInjectBindings(IKernel kernel)
        {
            kernel.Bind<IAdminNotificationService>().To<AdminNotificationService>();
            kernel.Bind<IUserNotificationService>().To<UserNotificationService>();
            kernel.Bind<ILegalOfficerNotificationService>().To<LegalOfficerNotificationService>();
            kernel.Bind<IConsumerProviderRegistrationNotificatonService>().To<ConsumerProviderRegistrationNotificationService>();
            kernel.Bind<INotificationService>().To<EmailNotificationService>();
        }
    }
}
using DataLinker.Services.Emails.Models;
using DataLinker.Models.Enums;
using Hangfire;
using System.Linq;
using Rezare.CommandBuilder.Services;
using DataLinker.Database.Models;

namespace DataLinker.Services.Emails.Roles.Admin
{
    public class AdminNotificationService : BaseEmailService, IAdminNotificationService
    {
        private readonly IService<Database.Models.User> _userService;
        private readonly IService<Organization> _organizationService;

        public AdminNotificationService(IEmailSettings emailSettings,
            IService<Database.Models.User> userService,
            IService<Organization> organizationService) : base(emailSettings)
        {
            _userService = userService;
            _organizationService = organizationService;
        }

        public void UpdatedStatusForLicense(int userId, string url, int newStatus)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);
            if (user != null)
            {
                var email = new AdminVerificationLicenseEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    DetailsLink = url,
                    DecisionMade = (PublishStatus) newStatus
                };

                Send(email);
            }
        }

        public void UpdatedStatusForLicenseInBackground(int userId, string url, int newStatus)
        {
            BackgroundJob.Enqueue<IAdminNotificationService>(
                s => s.UpdatedStatusForLicense(userId, url, newStatus));
        }

        public void NewIndustryGoodApplication(string url, int orgId)
        {
            var admins = _userService.Where(i => i.IsSysAdmin == true).Where(i => i.IsActive == true);
            var organization = _organizationService.FirstOrDefault(i => i.ID == orgId);
            foreach (var admin in admins)
            {
                var email = new IndustryGoodApplicationEmail
                {
                    To = admin.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = admin.Name,
                    OrgName = organization.Name,
                    DetailsLink = url
                };

                Send(email);
            }
        }

        public void NewIndustryGoodApplicationJob(string url, string orgName, string toName, string toEmail)
        {
            var email = new IndustryGoodApplicationEmail
            {
                To = toEmail,
                From = _emailSettings.SmtpFromEmail,
                Name = toName,
                OrgName = orgName,
                DetailsLink = url
            };

            Send(email);
        }

        public void NewIndustryGoodApplicationInBackground(string url, int orgId)
        {
            var admins = _userService.Where(i => i.IsSysAdmin == true).Where(i => i.IsActive == true);
            var organization = _organizationService.FirstOrDefault(i => i.ID == orgId);
            foreach (var admin in admins)
            {
                BackgroundJob.Enqueue<IAdminNotificationService>(
                    s => s.NewIndustryGoodApplicationJob(url, organization.Name, admin.Name, admin.Email));
            }
        }

        public void NewOrganization(string userName, string orgName, string userDetailsUrl, string orgDetailsUrl)
        {
            var admins = _userService.Where(i => i.IsSysAdmin == true).Where(i => i.IsActive == true);
            foreach (var admin in admins)
            {
                var email = new NewUserAndOrganizationEmail
                {
                    To = admin.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = admin.Name,
                    OrgName = orgName,
                    OrgDetailsLink = orgDetailsUrl,
                    UserName = userName,
                    UserDetailsLink = userDetailsUrl
                };

                Send(email);
            }
        }

        public void NewOrganizationJob(string userName, string orgName, string userDetailsUrl, string orgDetailsUrl,
            string toName, string toEmail)
        {
            var email = new NewUserAndOrganizationEmail
            {
                To = toEmail,
                From = _emailSettings.SmtpFromEmail,
                Name = toName,
                OrgName = orgName,
                OrgDetailsLink = orgDetailsUrl,
                UserName = userName,
                UserDetailsLink = userDetailsUrl
            };

            Send(email);
        }

        public void NewOrganizationInBackground(string userName, string orgName, string userDetailsUrl,
            string orgDetailsUrl)
        {
            var admins = _userService.Where(i => i.IsSysAdmin == true).Where(i => i.IsActive == true);
            foreach (var admin in admins)
            {
                BackgroundJob.Enqueue<IAdminNotificationService>(
                    s => s.NewOrganizationJob(userName, orgName, userDetailsUrl, orgDetailsUrl, admin.Name, admin.Email));
            }
        }

        public void NewLegalOfficer(string url, string organizationName)
        {
            var admins = _userService.Where(i => i.IsSysAdmin == true).Where(i => i.IsActive == true);
            foreach (var admin in admins)
            {
                var email = new NewLegalOfficerEmail
                {
                    To = admin.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = admin.Name,
                    OrganizationName = organizationName
                };

                Send(email);
            }
        }

        public void NewLegalOfficerJob(string url, string organizationName, string toEmail, string toName)
        {
            var email = new NewLegalOfficerEmail
            {
                To = toEmail,
                From = _emailSettings.SmtpFromEmail,
                Name = toName,
                OrganizationName = organizationName
            };

            Send(email);
        }

        public void NewLegalOfficerInBackground(string url, string organizationName)
        {
            var admins = _userService.Where(i => i.IsSysAdmin == true).Where(i => i.IsActive == true);
            foreach (var admin in admins)
            {
                BackgroundJob.Enqueue<IAdminNotificationService>(
                    s => s.NewLegalOfficerJob(url, organizationName, admin.Email, admin.Name));
            }
        }
    }
}

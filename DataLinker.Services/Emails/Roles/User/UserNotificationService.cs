using DataLinker.Services.Emails.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Helpers;
using Hangfire;
using System.Linq;
using Rezare.CommandBuilder.Services;
using DataLinker.Database.Models;

namespace DataLinker.Services.Emails.Roles.User
{
    public class UserNotificationService: BaseEmailService, IUserNotificationService
    {
        private readonly IService<LicenseClauseTemplate> _clausesTemplateService;
        private readonly IService<Database.Models.User> _userService;
        private readonly IService<OrganizationLicense> _licenseService;
        private readonly IService<Application> _appService;
        private readonly IService<Organization> _organizationService;
        private readonly IService<DataSchema> _dataSchemaService;

        public UserNotificationService(IService<Database.Models.User> userService,
            IService<LicenseClauseTemplate> clausesTemplateService,
            IService<OrganizationLicense> licenseService,
            IService<Application> appService,
            IService<Organization> organizationService,
            IService<DataSchema> dataSchemaService,
            IEmailSettings emailSettings) : base(emailSettings)
        {
            _userService = userService;
            _clausesTemplateService = clausesTemplateService;
            _licenseService = licenseService;
            _appService = appService;
            _organizationService = organizationService;
            _dataSchemaService = dataSchemaService;
        }

        public void EmailVerification(int userId, string url)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);
            var org = _organizationService.FirstOrDefault(o => o.ID == user.OrganizationID);

            if (user != null)
            {
                var email = new UserVerificationEmail
                {
                    To = user.NewEmail,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    VerificationLink = url,
                    OrgName = org.Name
                };

                Send(email);
            }
        }

        public void EmailInvitation(int userId, string url, int inviterID)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);
            var inviter = _userService.FirstOrDefault(i => i.ID == inviterID);
            var org = _organizationService.FirstOrDefault(o => o.ID == user.OrganizationID);

            if (user != null)
            {
                var email = new UserInvitationEmail()
                {
                    To = user.NewEmail,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    VerificationLink = url,
                    OrgName = org.Name,
                    InviterName = inviter.Name
                };

                Send(email);
            }
        }

        public void NewClause(int clauseTemplateId)
        {
            var clause = _clausesTemplateService.FirstOrDefault(i => i.ID ==clauseTemplateId);
            var users = _userService.Where(i=>i.IsActive == true);
            foreach (var user in users)
            {
                var email = new ClauseTemplatePublishedEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    ClauseDescription = clause.Description
                };

                Send(email);
            }
        }

        public void NewClauseJob(string toEmail, string toName, string clauseDescr)
        {
            var email = new ClauseTemplatePublishedEmail
            {
                To = toEmail,
                From = _emailSettings.SmtpFromEmail,
                Name = toName,
                ClauseDescription = clauseDescr
            };

            Send(email);
        }

        public void OrganizationIsActive(int orgId, ActivationState state)
        {
            var org = _organizationService.FirstOrDefault(i => i.ID == orgId);
            var users = _userService.Where(i => i.OrganizationID == orgId).Where(i => i.IsActive == true);
            foreach (var user in users)
            {
                var email = new OrganizationStateChangedEmail()
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    OrganizationName = org.Name,
                    State = state.GetDescription()
                };

                Send(email);
            }
        }

        public void OrganizationIsActiveJob(string toEmail, string toName, string orgName, ActivationState state)
        {
                var email = new OrganizationStateChangedEmail
                {
                    To = toEmail,
                    From = _emailSettings.SmtpFromEmail,
                    Name = toName,
                    OrganizationName = orgName,
                    State = state.GetDescription()
                };

                Send(email);
        }

        public void SoftwareStatementUpdated(int orgId, string urlToApplicationDetails)
        {
            var org = _organizationService.FirstOrDefault(i => i.ID == orgId);
            var users = _userService.Where(i => i.OrganizationID == orgId).Where(i => i.IsActive == true);
            foreach (var user in users)
            {
                var email = new SoftwareStatementUpdatedEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    OrganizationName = org.Name,
                    UrlToApplicationDetails = urlToApplicationDetails
                };

                Send(email);
            }
        }
        public void SoftwareStatementUpdatedJob(string toEmail, string toName,string orgName, string urlToApplicationDetails)
        {
            var email = new SoftwareStatementUpdatedEmail
            {
                To = toEmail,
                From = _emailSettings.SmtpFromEmail,
                Name = toName,
                OrganizationName = orgName,
                UrlToApplicationDetails = urlToApplicationDetails
            };

            Send(email);
        }

        public void SoftwareStatementUpdatedInBackground(int orgId, string urlToApplicationDetails)
        {
            var org = _organizationService.FirstOrDefault(i => i.ID == orgId);
            var users = _userService.Where(i => i.OrganizationID == orgId).Where(i => i.IsActive == true);
            foreach (var user in users)
            {
                BackgroundJob.Enqueue<IUserNotificationService>(
                    s => s.SoftwareStatementUpdatedJob(user.Email, user.Name, org.Name, urlToApplicationDetails));
            }
        }

        public void SchemaRetracted(int userId, string schemaName)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);
            if (user != null)
            {
                var email = new SchemaRetractedEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    SchemaName = schemaName
                };

                Send(email);
            }
        }

        public void UpdatedAccountState(int userId, ActivationState state)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);
            var email = new AccountStateChangedEmail
            {
                To = user.Email,
                From = _emailSettings.SmtpFromEmail,
                Name = user.Name,
                State = state.GetDescription()
            };

            Send(email);
        }

        public void StatusForLicenseUpdated(int userId, string url, string schemaName, string organizationName, bool isProvider, int newStatus)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);
            if (user != null)
            {
                var email = new UserVerificationLicenseEmail()
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    IsProvider = isProvider,
                    SchemaName = schemaName,
                    OrganizationName = organizationName,
                    DecisionMade = (PublishStatus)newStatus,
                    DetailsLink = url
                };

                Send(email);
            }
        }

        public void UpdatedAccountStateInBackground(int userId, ActivationState state)
        {
            BackgroundJob.Enqueue<IUserNotificationService>(s => s.UpdatedAccountState(userId, state));
        }

        public void SchemaRetractedInBackground(int userId, string schemaName)
        {
            BackgroundJob.Enqueue<IUserNotificationService>(s => s.SchemaRetracted(userId, schemaName));
        }

        public void NewClauseInBackground(int clauseId)
        {
            var clause = _clausesTemplateService.FirstOrDefault(i => i.ID == clauseId);
            var users = _userService.Where(i => i.IsActive == true);
            foreach (var user in users)
            {
                BackgroundJob.Enqueue<IUserNotificationService>(s => s.NewClauseJob(user.Email,user.Name,clause.Description));
            }
        }

        public void NewProviderLicense(int licenseId, int applicationId, string linkToLicense)
        {
            var license = _licenseService.FirstOrDefault(i => i.ID == licenseId);
            var application = _appService.FirstOrDefault(i => i.ID == applicationId);
            var organization = _organizationService.FirstOrDefault(i => i.ID == application.OrganizationID);
            var dataSchema = _dataSchemaService.FirstOrDefault(i => i.ID == license.DataSchemaID);
            var users = _userService.Where(i => i.IsActive == true);

            foreach (var user in users)
            {
                var email = new NewProviderLicenseEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    OrgName = organization.Name,
                    LinkToLicense = linkToLicense,
                    SchemaName = dataSchema.Name
                };

                Send(email);
            }
        }

        public void NewProviderLicenseJob(string toEmail,string toName, string orgName,string schemaName, string linkToLicense)
        {
            var email = new NewProviderLicenseEmail
            {
                To = toEmail,
                From = _emailSettings.SmtpFromEmail,
                Name = toName,
                OrgName = orgName,
                LinkToLicense = linkToLicense,
                SchemaName = schemaName
            };

            Send(email);
        }

        public void NewProviderLicenseInBackground(int licenseId, int applicationId, string linkToLicense)
        {
            var license = _licenseService.FirstOrDefault(i => i.ID == licenseId);
            var application = _appService.FirstOrDefault(i => i.ID == applicationId);
            var organization = _organizationService.FirstOrDefault(i => i.ID == application.OrganizationID);
            var dataSchema = _dataSchemaService.FirstOrDefault(i => i.ID == license.DataSchemaID);
            var users = _userService.Where(i => i.IsActive == true);

            foreach (var user in users)
            {
                BackgroundJob.Enqueue<IUserNotificationService>(
                    s =>
                        s.NewProviderLicenseJob(user.Email, user.Name, organization.Name, dataSchema.Name, linkToLicense));
            }
        }

        public void EmailVerificationInBackground(int userId, string url)
        {
            BackgroundJob.Enqueue<IUserNotificationService>(s => s.EmailVerification(userId, url));
        }

        public void EmailInvitationInBackground(int userId, string url, int inviterID)
        {
            BackgroundJob.Enqueue<IUserNotificationService>(s => s.EmailInvitation(userId, url, inviterID));
        }

        public void OrganizationIsActiveInBackground(int orgId, ActivationState state)
        {
            var org = _organizationService.FirstOrDefault(i => i.ID == orgId);
            var users = _userService.Where(i => i.OrganizationID == orgId).Where(i => i.IsActive == true);
            foreach (var user in users)
            {
                BackgroundJob.Enqueue<IUserNotificationService>(
                    s => s.OrganizationIsActiveJob(user.Email, user.Name, org.Name, state));
            }
        }

        public void StatusForLicenseUpdatedInBackground(int userId, string url, string schemaName, string organizationName, bool isProvider, int newStatus)
        {
            BackgroundJob.Enqueue<IUserNotificationService>(s => s.StatusForLicenseUpdated(userId, url, schemaName, organizationName, isProvider, newStatus));
        }
        
        public void LegalOfficerRegisteredJob(int userId)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);

            if (user != null)
            {
                var email = new LegalOfficerRegisteredEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                };

                Send(email);
            }
        }

        public void LegalOfficerRegisteredInBackground(int userId)
        {
            BackgroundJob.Enqueue<IUserNotificationService>(s => s.LegalOfficerRegisteredJob(userId));
        }
    }
}

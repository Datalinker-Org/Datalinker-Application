using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using DataLinker.Database.Models;
using DataLinker.Services.Emails.Models;
using DataLinker.Services.Emails.Models.ConsumerProviderRegistrations;
using DataLinker.Services.FileProviders;
using DataLinker.Services.LicenseContent;
using Hangfire;
using NReco.PdfGenerator;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Services.Emails.Roles.LegalOfficer
{
    public class LegalOfficerNotificationService : BaseEmailService, ILegalOfficerNotificationService
    {
        private readonly IService<OrganizationLicense> _licenseService;
        private readonly IService<Application> _applicationService;
        private readonly IService<Organization> _organizationService;
        private readonly IService<LicenseTemplate> _licenseTemplateService;
        private readonly ILicenseContentBuilder _licenseContentBuilder;
        private readonly IService<Database.Models.User> _userService;
        private readonly IService<LicenseAgreement> _agreementService;
        private readonly IService<DataSchema> _dataSchemaService;
        private readonly IService<ConsumerProviderRegistration, int> _consumerProviderRegistrations;
        private readonly ILicenseFileProvider _licenseFileProvider;

        public LegalOfficerNotificationService(IEmailSettings emailSettings,
            IService<OrganizationLicense> licenseService,
            IService<Application> applicationService,
            IService<Organization> organizationService,
            IService<LicenseTemplate> licenseTemplateService,
            ILicenseContentBuilder licenseContentBuilder,
            IService<Database.Models.User> userService,
            IService<LicenseAgreement> agreementService,
            IService<ConsumerProviderRegistration, int> consumerProviderRegistration,
            IService<DataSchema> dataSchemaService,
            ILicenseFileProvider licenseFileProvider) : base(emailSettings)
        {
            _licenseService = licenseService;
            _applicationService = applicationService;
            _organizationService = organizationService;
            _licenseTemplateService = licenseTemplateService;
            _licenseContentBuilder = licenseContentBuilder;
            _userService = userService;
            _agreementService = agreementService;
            _consumerProviderRegistrations = consumerProviderRegistration;
            _dataSchemaService = dataSchemaService;
            _licenseFileProvider = licenseFileProvider;
        }

        public void LicenseIsPendingApproval(int userId,
            string linkToConfirmScreen,
            string linkToSchema,
            string linkToDataLinker,
            string schemaName,
            int licenseId)
        {
            var license = _licenseService.FirstOrDefault(i=>i.ID == licenseId);
            var application = _applicationService.FirstOrDefault(i => i.ID == license.ApplicationID);
            var organization = _organizationService.FirstOrDefault(i => i.ID == application.OrganizationID);
            var template = _licenseTemplateService.FirstOrDefault(i => i.ID == license.LicenseTemplateID.Value);
            var licenseDocument = _licenseContentBuilder.GetLicenseContent(license.ID);
            _licenseContentBuilder.InsertLicenseDetails(licenseDocument, linkToSchema, linkToDataLinker, organization.ID, application.IsProvider);
            var pdfDocument = new HtmlToPdfConverter { PageFooterHtml = _licenseContentBuilder.GetFooterText(license.Status, linkToDataLinker) };
            var pdfBytes = pdfDocument.GeneratePdf(licenseDocument.OuterXml);
            var stream = new MemoryStream(pdfBytes);
            var attachment = new List<Attachment>
            {
                new Attachment(stream, $"{template.Name}{MailFileFormat}",
                    MediaTypeNames.Application.Pdf)
            };

            var user = _userService.FirstOrDefault(i => i.ID == userId);
            if (user != null)
            {
                var email = new LegalOfficerVerificationLicenseEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    LinkToConfirmationScreen = linkToConfirmScreen,
                    OrganizationName = organization.Name,
                    SchemaName = schemaName,
                    IsProvider = application.IsProvider,
                    Attachments = attachment
                };

                Send(email);
            }
        }

        public void LicenseVerificationRequired(int userId,
            string linkToConfirmScreen,
            string schemaName,
            int licenseId)
        {
            var license = _licenseService.FirstOrDefault(i => i.ID == licenseId);
            var application = _applicationService.FirstOrDefault(i => i.ID == license.ApplicationID);
            var organization = _organizationService.FirstOrDefault(i => i.ID == application.OrganizationID);

            var user = _userService.FirstOrDefault(i => i.ID == userId);
            if (user != null)
            {
                var email = new CustomLicenseVerificationRequiredEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    LinkToConfirmationScreen = linkToConfirmScreen,
                    OrganizationName = organization.Name,
                    SchemaName = schemaName,
                    IsProvider = application.IsProvider,
                };

                Send(email);
            }
        }

        public void LicenseVerificationRequiredInBackground(int userId,
            string linkToConfirmScreen,
            string schemaName,
            int licenseId)
        {
            BackgroundJob.Enqueue<ILegalOfficerNotificationService>(
                s =>
                    s.LicenseVerificationRequired(userId, linkToConfirmScreen,
                        schemaName, licenseId));
        }

        public void LicenseApprovedSuccessfully(int userId,
            string linkToSchema,
            string linkToDataLinker,
            string schemaName,
            int licenseId,
            bool isProvider)
        {
            var license = _licenseService.FirstOrDefault(i => i.ID == licenseId);
            var application = _applicationService.FirstOrDefault(i => i.ID == license.ApplicationID);
            var organization = _organizationService.FirstOrDefault(i => i.ID == application.OrganizationID);
            //var template = _licenseTemplateService.FirstOrDefault(i => i.ID == license.LicenseTemplateID.Value);
            //var licenseDocument = _licenseContentBuilder.GetLicenseContent(license.ID);
            //_licenseContentBuilder.InsertLicenseDetails(licenseDocument, linkToSchema, linkToDataLinker, organization.ID, isProvider);
            //var pdfDocument = new HtmlToPdfConverter { PageFooterHtml = _licenseContentBuilder.GetFooterText(license.Status, linkToDataLinker) };
            //var pdfBytes = pdfDocument.GeneratePdf(licenseDocument.OuterXml);
            //var stream = new MemoryStream(pdfBytes);
            //var attachment = new List<Attachment>
            //{
            //    new Attachment(stream, $"{template.Name}{MailFileFormat}",
            //        MediaTypeNames.Application.Pdf)
            //};

            var attachments = new List<Attachment>();
            var attachment = GetLicense(license.ID);
            attachments.Add(attachment);

            var user = _userService.FirstOrDefault(i => i.ID == userId);
            if (user != null)
            {
                var email = new LegalOfficerApprovedLicenseEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    OrganizationName = organization.Name,
                    SchemaName = schemaName,
                    IsProvider = isProvider,
                    Attachments = attachments
                };

                Send(email);
            }
        }

        public void LicenseApprovedSuccessfullyInBackground(int userId,
            string linkToSchema,
            string linkToDataLinker,
            string schemaName,
            int licenseId,
            bool isProvider)
        {
            BackgroundJob.Enqueue<ILegalOfficerNotificationService>(
                s =>
                    s.LicenseApprovedSuccessfully(userId, linkToSchema, linkToDataLinker,
                        schemaName, licenseId, isProvider));
        }

        public void LicenseAgreementCreated(int licenseId,
            string linkToLicense,
            string linkToSchema,
            string urlToDataLinker)
        {
            var agreement = _agreementService.FirstOrDefault(i => i.ID == licenseId);
            var consumerRegistration = _consumerProviderRegistrations.GetById(agreement.ConsumerProviderRegistrationId);
            var providerLicense = _licenseService.FirstOrDefault(i => i.ID == consumerRegistration.OrganizationLicenseID);
            var consumerApp = _applicationService.FirstOrDefault(i => i.ID == consumerRegistration.ConsumerApplicationID);
            var consumerOrganization = _organizationService.FirstOrDefault(i => i.ID == consumerApp.OrganizationID);
            var providerApp = _applicationService.FirstOrDefault(i => i.ID == providerLicense.ApplicationID);
            var providerOrganization = _organizationService.FirstOrDefault(i => i.ID == providerApp.OrganizationID);
            var dataSchema = _dataSchemaService.FirstOrDefault(i => i.ID == providerLicense.DataSchemaID);
            var users = _userService.Where(i => i.OrganizationID == consumerOrganization.ID).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true).ToList();
            var providerLegals = _userService.Where(i => i.OrganizationID == providerOrganization.ID).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true);
            users.AddRange(providerLegals);
            var template = _licenseTemplateService.FirstOrDefault(i => i.ID == providerLicense.LicenseTemplateID.Value);
            var licenseDocument = _licenseContentBuilder.GetLicenseContent(organizationLicenseId: providerLicense.ID);
            _licenseContentBuilder.InsertAgreementDetails(licenseDocument, agreement.ID, linkToSchema, urlToDataLinker);
            var pdfBytes = new HtmlToPdfConverter().GeneratePdf(licenseDocument.OuterXml);

            foreach (var user in users)
            {
                var stream = new MemoryStream(pdfBytes);
                var attachment = new List<Attachment>
                {
                    new Attachment(stream, $"{template.Name}{MailFileFormat}",
                        MediaTypeNames.Application.Pdf)
                };

                var email = new NewLicenseAgreementEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    OrgNameConsumer = consumerOrganization.Name,
                    OrgNameProvider = providerOrganization.Name,
                    LinkToLicense = linkToLicense,
                    Attachments = attachment,
                    SchemaName = dataSchema.Name
                };

                Send(email);
            }
        }

        public void LicenseAgreementCreatedJob(int providderLicenseId,
            int agreementId,
            string linkToLicense,
            string linkToSchema,
            string urlToDataLinker,
            string toEmail,
            string toName,
            string consumerOrg,
            string providerOrg,
            string schemaName)
        {
            var providerLicense = _licenseService.FirstOrDefault(i => i.ID == providderLicenseId);
            var template = _licenseTemplateService.FirstOrDefault(i => i.ID == providerLicense.LicenseTemplateID.Value);
            var licenseDocument = _licenseContentBuilder.GetLicenseContent(organizationLicenseId: providerLicense.ID);
            _licenseContentBuilder.InsertAgreementDetails(licenseDocument, agreementId, linkToSchema, urlToDataLinker);
            var pdfBytes = new HtmlToPdfConverter().GeneratePdf(licenseDocument.OuterXml);

            var stream = new MemoryStream(pdfBytes);
            var attachment = new List<Attachment>
            {
                new Attachment(stream, $"{template.Name}{MailFileFormat}",
                    MediaTypeNames.Application.Pdf)
            };

            var email = new NewLicenseAgreementEmail
            {
                To = toEmail,
                From = _emailSettings.SmtpFromEmail,
                Name = toName,
                OrgNameConsumer = consumerOrg,
                OrgNameProvider = providerOrg,
                LinkToLicense = linkToLicense,
                Attachments = attachment,
                SchemaName = schemaName
            };

            Send(email);
        }

        public void LicenseAgreementCreatedInBackground(int licenseId,
            string linkToLicense,
            string linkToSchema,
            string urlToDataLinker)
        {
            var agreement = _agreementService.FirstOrDefault(i => i.ID == licenseId);
            var consumerRegistration = _consumerProviderRegistrations.GetById(agreement.ConsumerProviderRegistrationId);
            var providerLicense = _licenseService.FirstOrDefault(i => i.ID == consumerRegistration.OrganizationLicenseID);
            var consumerApp = _applicationService.FirstOrDefault(i => i.ID == consumerRegistration.ConsumerApplicationID);
            var consumerOrganization = _organizationService.FirstOrDefault(i => i.ID == consumerApp.OrganizationID);
            var providerApp = _applicationService.FirstOrDefault(i => i.ID == providerLicense.ApplicationID);
            var providerOrganization = _organizationService.FirstOrDefault(i => i.ID == providerApp.ID);
            var dataSchema = _dataSchemaService.FirstOrDefault(i => i.ID == providerLicense.DataSchemaID);
            var users = _userService.Where(i => i.OrganizationID == consumerOrganization.ID).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true).ToList();
            var providerLegals = _userService.Where(i => i.OrganizationID == providerOrganization.ID).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true);
            users.AddRange(providerLegals);
            foreach (var user in users)
            {
                BackgroundJob.Enqueue<ILegalOfficerNotificationService>(
                    s =>
                        s.LicenseAgreementCreatedJob(providerLicense.ID, agreement.ID, linkToLicense,
                            linkToSchema, urlToDataLinker, user.Email, user.Name, consumerOrganization.Name,
                            providerOrganization.Name, dataSchema.Name));
            }
        }

        public void LicenseIsPendingApprovalInBackground(int userId,
            string linkToConfirmScreen,
            string linkToSchema,
            string linkToDataLinker,
            string schemaName,
            int licenseId)
        {
            BackgroundJob.Enqueue<ILegalOfficerNotificationService>(
                s =>
                    s.LicenseIsPendingApproval(userId, linkToConfirmScreen, linkToSchema, linkToDataLinker,
                        schemaName, licenseId));
        }

        public void NewConsumerRequest(string linkToConsumerRequests,
            int userId)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);
            var email = new NewConsumerRegistrationEmail
            {
                To = user.Email,
                From = _emailSettings.SmtpFromEmail,
                Name = user.Name,
                UrlToConsumerRequests = linkToConsumerRequests,
            };

            Send(email);
        }

        public void NewConsumerRequestInBackground(int applicationId,
            string linkToConsumerRequests)
        {
            var application = _applicationService.FirstOrDefault(i => i.ID == applicationId);
            var legalOfficers = _userService.Where(i => i.OrganizationID == application.OrganizationID).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true);

            foreach (var user in legalOfficers)
            {
                BackgroundJob.Enqueue<ILegalOfficerNotificationService>(
                    s =>
                        s.NewConsumerRequest(linkToConsumerRequests, user.ID));
            }
        }


        public void RejectedConsumerRequest(string schemaName,
            string providerName,
            int userId)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);
            var email = new RejectedConsumerRegistrationEmail
            {
                To = user.Email,
                From = _emailSettings.SmtpFromEmail,
                Name = user.Name,
                ProviderName = providerName,
                SchemaName = schemaName,
            };

            Send(email);
        }

        public void RejectedConsumerRequestInBackground(int applicationId,
            int schemaId)
        {
            var application = _applicationService.FirstOrDefault(i => i.ID == applicationId);
            var organisation = _organizationService.FirstOrDefault(i => i.ID == application.OrganizationID);
            var legalOfficers = _userService.Where(i => i.OrganizationID == application.OrganizationID).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true);
            var schema = _dataSchemaService.FirstOrDefault(i => i.ID == schemaId);
            foreach (var user in legalOfficers)
            {
                BackgroundJob.Enqueue<ILegalOfficerNotificationService>(
                    s =>
                        s.RejectedConsumerRequest(schema.Name, organisation.Name, user.ID));
            }
        }

        public void ApprovedConsumerRequest(string schemaName,
            string providerName,
            int userId)
        {
            var user = _userService.FirstOrDefault(i => i.ID == userId);
            var email = new ApprovedConsumerRegistrationEmail
            {
                To = user.Email,
                From = _emailSettings.SmtpFromEmail,
                Name = user.Name,
                ProviderName = providerName,
                SchemaName = schemaName,
            };

            Send(email);
        }

        public void ApprovedConsumerRequestInBackground(int applicationId,
            int schemaId)
        {
            var application = _applicationService.FirstOrDefault(i => i.ID == applicationId);
            var organisation = _organizationService.FirstOrDefault(i => i.ID == application.OrganizationID);
            var legalOfficers = _userService.Where(i => i.OrganizationID == application.OrganizationID).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true);
            var schema = _dataSchemaService.FirstOrDefault(i => i.ID == schemaId);
            foreach (var user in legalOfficers)
            {
                BackgroundJob.Enqueue<ILegalOfficerNotificationService>(
                    s =>
                        s.ApprovedConsumerRequest(schema.Name, organisation.Name, user.ID));
            }
        }


        private Attachment GetLicense(int organizationLicenseId)
        {
            var customLicenseDetails = _licenseFileProvider.GetLicense(organizationLicenseId);
            return new Attachment(new MemoryStream(customLicenseDetails.Content), customLicenseDetails.FileName, customLicenseDetails.MimeType);
        }
    }
}

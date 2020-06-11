using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Services.Emails.Models;
using DataLinker.Services.Emails.Models.ConsumerProviderRegistrations;
using DataLinker.Services.FileProviders;
using DataLinker.Services.LicenseContent;
using Hangfire;
using NReco.PdfGenerator;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Services.Emails.Services
{
    public class ConsumerProviderRegistrationNotificationService : BaseEmailService, IConsumerProviderRegistrationNotificatonService
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

        public ConsumerProviderRegistrationNotificationService(IEmailSettings emailSettings,
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

        public void ConsumerLegalApprovalRequestInBackground(int userId, string linkToConfirmScreen, string linkToSchema, string linkToDataLinker, string schemaName, int consumerProviderRegistrationId)
        {
            BackgroundJob.Enqueue<IConsumerProviderRegistrationNotificatonService>(
                s => s.ConsumerLegalApproval(userId, linkToConfirmScreen, linkToSchema, linkToDataLinker, schemaName, consumerProviderRegistrationId)
            );
        }

        public void ConsumerLegalApproval(int userId, string linkToConfirmScreen, string linkToSchema, string linkToDataLinker, string schemaName, int consumerProviderRegistrationId)
        {
            var registeredProvider = _consumerProviderRegistrations.FirstOrDefault(i => i.ID == consumerProviderRegistrationId);
            var consumerApplication = _applicationService.FirstOrDefault(i => i.ID == registeredProvider.ConsumerApplicationID);
            var consumerOrganization = _organizationService.FirstOrDefault(i => i.ID == consumerApplication.OrganizationID);
            var orgLicense = _licenseService.FirstOrDefault(i => i.ID == registeredProvider.OrganizationLicenseID);


            ////var license = _licenseService.FirstOrDefault(i => i.ID == licenseId);
            var application = _applicationService.FirstOrDefault(i => i.ID == orgLicense.ApplicationID);
            var providerOrganization = _organizationService.FirstOrDefault(i => i.ID == application.OrganizationID);
            //var template = _licenseTemplateService.FirstOrDefault(i => i.ID == license.LicenseTemplateID.Value);
            //var licenseDocument = _licenseContentBuilder.GetLicenseContent(license.ID);
            //_licenseContentBuilder.InsertLicenseDetails(licenseDocument, linkToSchema, linkToDataLinker, providerOrganization.ID, application.IsProvider);
            //var pdfDocument = new HtmlToPdfConverter { PageFooterHtml = _licenseContentBuilder.GetFooterText(license.Status, linkToDataLinker) };
            //var pdfBytes = pdfDocument.GeneratePdf(licenseDocument.OuterXml);
            //var stream = new MemoryStream(pdfBytes);
            //var attachment = new List<Attachment>
            //{
            //    new Attachment(stream, $"{template.Name}{MailFileFormat}",
            //        MediaTypeNames.Application.Pdf)
            //};

            var attachments = new List<Attachment>();
            var attachment = GetLicense(orgLicense.ID, providerOrganization.ID, consumerOrganization.ID);
            attachments.Add(attachment);

            var user = _userService.FirstOrDefault(i => i.ID == userId);
            if (user != null)
            {
                var email = new ConsumerLegalApprovalEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    LinkToConfirmationScreen = linkToConfirmScreen,
                    ProviderOrganizationName = providerOrganization.Name,
                    SchemaName = schemaName,
                    Attachments = attachments,

                    ConsumerOrganizationName = consumerOrganization.Name
                };

                Send(email);
            }
        }
        
        public void ProviderLegalApprovalRequestInBackground(int userId, string linkToConfirmScreen, string linkToSchema, string linkToDataLinker, string schemaName, int consumerProviderRegistrationId)
        {
            BackgroundJob.Enqueue<IConsumerProviderRegistrationNotificatonService>(
                s => s.ProviderLegalApproval(userId, linkToConfirmScreen, linkToSchema, linkToDataLinker, schemaName, consumerProviderRegistrationId)
            );
        }

        public void ProviderLegalApproval(int userId, string linkToConfirmScreen, string linkToSchema, string linkToDataLinker, string schemaName, int consumerProviderRegistrationId)
        {
            var registeredProvider = _consumerProviderRegistrations.FirstOrDefault(i => i.ID == consumerProviderRegistrationId);
            var consumerApplication = _applicationService.FirstOrDefault(i => i.ID == registeredProvider.ConsumerApplicationID);
            var consumerOrganization = _organizationService.FirstOrDefault(i => i.ID == consumerApplication.OrganizationID);
            var orgLicense = _licenseService.FirstOrDefault(i => i.ID == registeredProvider.OrganizationLicenseID);


            //var license = _licenseService.FirstOrDefault(i => i.ID == licenseId);
            var application = _applicationService.FirstOrDefault(i => i.ID == orgLicense.ApplicationID);
            var providerOrganization = _organizationService.FirstOrDefault(i => i.ID == application.OrganizationID);
            //var template = _licenseTemplateService.FirstOrDefault(i => i.ID == license.LicenseTemplateID.Value);
            //var licenseDocument = _licenseContentBuilder.GetLicenseContent(license.ID);
            //_licenseContentBuilder.InsertLicenseDetails(licenseDocument, linkToSchema, linkToDataLinker, providerOrganization.ID, application.IsProvider);
            //var pdfDocument = new HtmlToPdfConverter { PageFooterHtml = _licenseContentBuilder.GetFooterText(license.Status, linkToDataLinker) };
            //var pdfBytes = pdfDocument.GeneratePdf(licenseDocument.OuterXml);
            //var stream = new MemoryStream(pdfBytes);
            //var attachment = new List<Attachment>
            //{
            //    new Attachment(stream, $"{template.Name}{MailFileFormat}",
            //        MediaTypeNames.Application.Pdf)
            //};

            var attachments = new List<Attachment>();
            var attachment = GetLicense(orgLicense.ID, providerOrganization.ID, consumerOrganization.ID);
            attachments.Add(attachment);

            var user = _userService.FirstOrDefault(i => i.ID == userId);
            if (user != null)
            {
                var email = new ProviderLegalApprovalEmail
                {
                    To = user.Email,
                    From = _emailSettings.SmtpFromEmail,
                    Name = user.Name,
                    LinkToConfirmationScreen = linkToConfirmScreen,
                    ProviderOrganizationName = providerOrganization.Name,
                    SchemaName = schemaName,
                    Attachments = attachments,

                    ConsumerOrganizationName = consumerOrganization.Name
                };

                Send(email);
            }
        }

        public void ProviderLegalApproveRequestInBackground(int userId, int consumerProviderRegistrationId, string schemaName, string linkToSchema, string linkToDataLinker) {
            BackgroundJob.Enqueue<IConsumerProviderRegistrationNotificatonService>(
                s =>
                    s.ProviderLegalApprove(userId, consumerProviderRegistrationId, schemaName, linkToSchema, linkToDataLinker));
        }

        public void ProviderLegalApprove(int userId, int consumerProviderRegistrationId, string schemaName, string linkToSchema, string linkToDataLinker)
        {
            var registration = _consumerProviderRegistrations.FirstOrDefault(i => i.ID == consumerProviderRegistrationId);
            var user = _userService.FirstOrDefault(p => p.ID == userId);

            
            // get consumer information
            var consApp = _applicationService.FirstOrDefault(p => p.ID == registration.ConsumerApplicationID);
            var consOrg = _organizationService.FirstOrDefault(p => p.ID == consApp.OrganizationID);

            // get provider information and get/build provider license agreements pdf
            var orgLicense = _licenseService.FirstOrDefault(i => i.ID == registration.OrganizationLicenseID);
            var provApp = _applicationService.FirstOrDefault(i => i.ID == orgLicense.ApplicationID);
            var provOrg = _organizationService.FirstOrDefault(i => i.ID == provApp.OrganizationID);

            var attachments = new List<Attachment>();
            var attachment = GetLicense(orgLicense.ID, provOrg.ID, consOrg.ID);
            attachments.Add(attachment);

            var email = new ProviderLegalApproveEmail {
                To = user.Email,
                From = _emailSettings.SmtpFromEmail,
                Name = user.Name,
                ProviderOrganizationName = provOrg.Name,
                ConsumerOrganizationName = consOrg.Name,
                SchemaName = schemaName,
                Attachments = attachments
            };
            Send(email);
        }

        public void ConsumerRegistrationCompletionRequestInBackground(int userId, int consumerProviderRegistrationId, string schemaName, string linkToSchema, string linkToDataLinker) {
            BackgroundJob.Enqueue<IConsumerProviderRegistrationNotificatonService>(
                s =>
                    s.ConsumerRegistrationCompletion(userId, consumerProviderRegistrationId, schemaName, linkToSchema, linkToDataLinker));
        }

        public void ConsumerRegistrationCompletion(int userId, int consumerProviderRegistrationId, string schemaName, string linkToSchema, string linkToDataLinker) {
            var registration = _consumerProviderRegistrations.FirstOrDefault(i => i.ID == consumerProviderRegistrationId);
            var user = _userService.FirstOrDefault(p => p.ID == userId);
            // get consumer information
            var consApp = _applicationService.FirstOrDefault(p => p.ID == registration.ConsumerApplicationID);
            var consOrg = _organizationService.FirstOrDefault(p => p.ID == consApp.OrganizationID);

            // get provider information and get/build provider license agreements pdf
            var orgLicense = _licenseService.FirstOrDefault(i => i.ID == registration.OrganizationLicenseID);
            var provApp = _applicationService.FirstOrDefault(i => i.ID == orgLicense.ApplicationID);
            var provOrg = _organizationService.FirstOrDefault(i => i.ID == provApp.OrganizationID);

            var attachments = new List<Attachment>();
            var attachment = GetLicense(orgLicense.ID, provOrg.ID, consOrg.ID);
            attachments.Add(attachment);

            var email = new ConsumerRegistrationCompletionEmail
            {
                To = user.Email,
                From = _emailSettings.SmtpFromEmail,
                Name = user.Name,
                ProviderOrganizationName = provOrg.Name,
                ConsumerOrganizationName = consOrg.Name,
                SchemaName = schemaName,
                Attachments = attachments
            };
            Send(email);
        }

        public void ConsumerRegistrationDeclineByProviderLegalRequestInBackground(int userId, int consumerProviderRegistrationId, string schemaName) {
            BackgroundJob.Enqueue<IConsumerProviderRegistrationNotificatonService>(
               s =>
                   s.ConsumerRegistrationDeclineByProviderLegal(userId, consumerProviderRegistrationId, schemaName));
        }

        public void ConsumerRegistrationDeclineByProviderLegal(int userId, int consumerProviderRegistrationId, string schemaName) {
            var registration = _consumerProviderRegistrations.FirstOrDefault(i => i.ID == consumerProviderRegistrationId);
            var user = _userService.FirstOrDefault(p => p.ID == userId);

            // Get provider info.
            var license = _licenseService.FirstOrDefault(i => i.ID == registration.OrganizationLicenseID);
            var provApp = _applicationService.FirstOrDefault(i => i.ID == license.ApplicationID);
            var provOrg = _organizationService.FirstOrDefault(i => i.ID == provApp.OrganizationID);

            var email = new ConsumerRegistrationDeclineByProviderEmail
            {
                To = user.Email,
                From = _emailSettings.SmtpFromEmail,
                Name = user.Name,
                ProviderOrganizationName = provOrg.Name,
                SchemaName = schemaName,
                DeclineReason = registration.Remarks,
            };
            Send(email);
        }

        public void ConsumerRegistrationDeclineByConsumerLegalRequestInBackground(int userId, int consumerProviderRegistrationId, string schemaName) {
            BackgroundJob.Enqueue<IConsumerProviderRegistrationNotificatonService>(
               s =>
                   s.ConsumerRegistrationDeclineByConsumerLegal(userId, consumerProviderRegistrationId, schemaName));
        }

        public void ConsumerRegistrationDeclineByConsumerLegal(int userId, int consumerProviderRegistrationId, string schemaName) {
            var registration = _consumerProviderRegistrations.FirstOrDefault(i => i.ID == consumerProviderRegistrationId);
            var user = _userService.FirstOrDefault(p => p.ID == userId);

            // Get provider info.
            var license = _licenseService.FirstOrDefault(i => i.ID == registration.OrganizationLicenseID);
            var provApp = _applicationService.FirstOrDefault(i => i.ID == license.ApplicationID);
            var provOrg = _organizationService.FirstOrDefault(i => i.ID == provApp.OrganizationID);

            var declineBy = _userService.FirstOrDefault(p => p.ID == registration.DeclinedBy);

            var email = new ConsumerRegistrationDeclineByConsumerEmail
            {
                To = user.Email,
                From = _emailSettings.SmtpFromEmail,
                Name = user.Name,
                ProviderOrganizationName = provOrg.Name,
                SchemaName = schemaName,
                DeclineReason = registration.Remarks,
                DeclineByName = declineBy.Name,
            };
            Send(email);
        }

        private Attachment GetLicense(int organizationLicenseId)
        {
            var customLicenseDetails = _licenseFileProvider.GetLicense(organizationLicenseId);
            return new Attachment(new MemoryStream(customLicenseDetails.Content), customLicenseDetails.FileName, customLicenseDetails.MimeType);
        }

        private Attachment GetLicense(int organizationLicenseId, int providerOrganizationId, int consumerOrganizationId)
        {
            var customLicenseDetails = _licenseFileProvider.GetLicense(organizationLicenseId, providerOrganizationId, consumerOrganizationId);
            return new Attachment(new MemoryStream(customLicenseDetails.Content), customLicenseDetails.FileName, customLicenseDetails.MimeType);
        }
    }
}

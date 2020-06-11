using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Configuration;
using DataLinker.Services.Exceptions;
using DataLinker.Services.LicenseContent;
using DataLinker.Services.Security;
using DataLinker.Services.Urls;
using NReco.PdfGenerator;
using Rezare.CommandBuilder.Services;
using System.Collections.Generic;

namespace DataLinker.Services.FileProviders
{
    internal class LicenseFileProvider : ILicenseFileProvider
    {
        private ISecurityService _security;
        private IConfigurationService _config;
        private IService<SchemaFile, int> _schemaFiles;
        private IService<LicenseTemplate, int> _licenseTemplates;
        private ILicenseContentBuilder _licenseContentBuilder;
        private IService<LicenseAgreement, int> _agreements;
        private IService<OrganizationLicense, int> _licenses;
        private IUrlProvider _urls;
        private IService<CustomLicense, int> _customLicenses;
        private IService<Application, int> _applications;
        private IService<Organization, int> _organisations;
        private IService<ConsumerProviderRegistration, int> _consumerProviderRegistrations;

        public LicenseFileProvider(ISecurityService security,
            IConfigurationService config,
            ILicenseContentBuilder contentBuilder,
            IUrlProvider urls,
            IService<SchemaFile, int> schemaFiles,
            IService<LicenseTemplate, int> licenseTemplates,
            IService<LicenseAgreement, int> agreements,
            IService<OrganizationLicense, int> licenses,
            IService<Application, int> applications,
            IService<Organization, int> organizations,
            IService<ConsumerProviderRegistration, int> consumerProviderRegistrations,
            IService<CustomLicense, int> customLicenses)
        {
            _security = security;
            _config = config;
            _licenseContentBuilder = contentBuilder;
            _urls = urls;
            _schemaFiles = schemaFiles;
            _licenseTemplates = licenseTemplates;
            _agreements = agreements;
            _licenses = licenses;
            _customLicenses = customLicenses;
            _applications = applications;
            _organisations = organizations;
            _consumerProviderRegistrations = consumerProviderRegistrations;
        }

        public CustomFileDetails GetLicenseForDownload(int appId, int schemaId, int licenseId, LoggedInUserDetails user)
        {
            // Check access
            _security.CheckBasicAccess(user);

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            return GetLicenseFileInfo(schemaId, licenseId);
        }

        public CustomFileDetails GetTemplatedLicenseForPreview(List<SectionsWithClauses> model, int orgId, int schemaId, LoggedInUserDetails user)
        {
            // Check access
            _security.CheckBasicAccess(user);

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException("Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Define result
            var result = new CustomFileDetails();

            // Get published license template
            var template = _licenseTemplates.FirstOrDefault(i => i.Status == (int)TemplateStatus.Active);

            // Get schema file
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schemaId);

            // Setup url to download schema
            var urlToSchema =_urls.ToDownloadSchema(schemaFile.ID);

            // Setup content for document
            var document = _licenseContentBuilder.GetDocument(model, schemaId, orgId, template, _config.DataLinkerHost, urlToSchema);

            // Create pdf document
            var pdfDocument = new HtmlToPdfConverter
            {
                PageFooterHtml = _licenseContentBuilder.GetFooterText((int)PublishStatus.Draft, _config.DataLinkerHost)
            };

            // Get bytes from document
            var bytes = pdfDocument.GeneratePdf(document.OuterXml);

            // Setup result
            result.Content = bytes;
            result.MimeType = "application/pdf";
            result.FileName = $"{template.Name}.pdf";

            // Return result
            return result;
        }

        public CustomFileDetails GetAgreement(int agreementId, LoggedInUserDetails user)
        {
            // Get agreements
            var agreement = _agreements.FirstOrDefault(i => i.ID == agreementId);

            // Return error if data not found
            if (agreement == null)
            {
                throw new BaseException("License Agreement not found");
            }

            // Check whether user has access
            _security.CheckBasicAccess(user);

            var consumerRegistration = _consumerProviderRegistrations.GetById(agreement.ConsumerProviderRegistrationId);
            var providerLicense = _licenses.FirstOrDefault(i => i.ID == consumerRegistration.OrganizationLicenseID);
            var consumerApp = _applications.FirstOrDefault(i => i.ID == consumerRegistration.ConsumerApplicationID);
            var providerApp = _applications.FirstOrDefault(i => i.ID == providerLicense.ApplicationID);
            var isFromConsumerSide = user.Organization.ID == consumerApp.OrganizationID;
            var isFromProviderSide = user.Organization.ID == providerApp.OrganizationID;
            var isFromAllowedOrganization = isFromProviderSide || isFromConsumerSide;

            // Return error if not access to data
            if (!user.IsSysAdmin && !isFromAllowedOrganization)
            {
                throw new BaseException("Access denied.");
            }

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Get provider licese
            if(providerLicense.CustomLicenseID != null)
            {
                // Get uploaded by provider file
                var customLicenseResult = GetCustomLicenseForDownload(providerLicense.CustomLicenseID.Value);

                // Return custom license result
                return customLicenseResult;
            }

            // Get template
            var template = _licenseTemplates.FirstOrDefault(i => i.ID == providerLicense.LicenseTemplateID.Value);

            // Get schema file
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == providerLicense.DataSchemaID);

            // Setup url to download schema
            var urlToSchema = _urls.ToDownloadSchema(schemaFile.ID);

            // Setup license content
            var licenseDocument = _licenseContentBuilder.GetLicenseContent(organizationLicenseId: providerLicense.ID);

            // Insert provider details
            _licenseContentBuilder.InsertAgreementDetails(licenseDocument, agreement.ID, urlToSchema, _config.DataLinkerHost);

            // Get butes for generated pdf
            var bytes = new HtmlToPdfConverter().GeneratePdf(licenseDocument.OuterXml);

            var result = new CustomFileDetails
            {
                Content = bytes,
                FileName = template.Name + ".pdf",
                MimeType = "application/pdf"
            };

            return result;
        }

        public CustomFileDetails GetLicense(int organizationLicenseId)
        {
            var orgLicense = _licenses.FirstOrDefault(p => p.ID == organizationLicenseId);
            return GetLicenseFileInfo(orgLicense.DataSchemaID, orgLicense.ID);
        }

        public CustomFileDetails GetLicense(int organizationLicenseId, int providerOrganizationId, int consumerOrganizationId)
        {
            var orgLicense = _licenses.FirstOrDefault(p => p.ID == organizationLicenseId);
            return GetLicenseFileInfo(orgLicense.DataSchemaID, orgLicense.ID, providerOrganizationId, consumerOrganizationId);
        }

        private CustomFileDetails GetCustomLicenseForDownload(int licenseId)
        {
            // define result
            var result = new CustomFileDetails();

            // Get license
            var customLicense = _customLicenses.FirstOrDefault(i => i.ID == licenseId);

            // Setup result
            result.Content = customLicense.Content;
            result.MimeType = customLicense.MimeType;
            result.FileName = customLicense.FileName;

            // Return result
            return result;
        }

        private CustomFileDetails GetTemplatedLicenseForDownload(OrganizationLicense license, int orgId, string urlToSchema)
        {
            var result = new CustomFileDetails();

            // Get license template
            var template = _licenseTemplates.FirstOrDefault(i => i.ID == license.LicenseTemplateID.Value);

            // Get license document
            var licenseDocument = _licenseContentBuilder.GetLicenseContent(organizationLicenseId: license.ID);

            // Get schema file
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == license.DataSchemaID);

            // Build content for templated license
            _licenseContentBuilder.InsertLicenseDetails(licenseDocument, urlToSchema, _config.DataLinkerHost,
                orgId, license.ProviderEndpointID != 0);

            // Generate pdf file with content
            var pdfDocument = new HtmlToPdfConverter { PageFooterHtml = _licenseContentBuilder.GetFooterText(license.Status, _config.DataLinkerHost) };
            var bytes = pdfDocument.GeneratePdf(licenseDocument.OuterXml);

            // Setup result
            result.Content = bytes;
            result.MimeType = "application/pdf";
            result.FileName = $"{template.Name}.pdf";

            // Return result
            return result;
        }

        private CustomFileDetails GetTemplatedLicenseForDownload(OrganizationLicense license, int providerOrgId, int consumerOrgId, string urlToSchema)
        {
            var result = new CustomFileDetails();

            // Get license template
            var template = _licenseTemplates.FirstOrDefault(i => i.ID == license.LicenseTemplateID.Value);

            // Get license document
            var licenseDocument = _licenseContentBuilder.GetLicenseContent(organizationLicenseId: license.ID);

            // Get schema file
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == license.DataSchemaID);

            // Build content for templated license
            _licenseContentBuilder.InsertLicenseDetails(licenseDocument, urlToSchema, _config.DataLinkerHost, providerOrgId, consumerOrgId);

            // Generate pdf file with content
            var pdfDocument = new HtmlToPdfConverter { PageFooterHtml = _licenseContentBuilder.GetFooterText(license.Status, _config.DataLinkerHost) };
            var bytes = pdfDocument.GeneratePdf(licenseDocument.OuterXml);

            // Setup result
            result.Content = bytes;
            result.MimeType = "application/pdf";
            result.FileName = $"{template.Name}.pdf";

            // Return result
            return result;
        }

        private CustomFileDetails GetLicenseFileInfo(int schemaId, int licenseId)
        {
            // Get license
            var license = _licenses.FirstOrDefault(i => i.ID == licenseId);
            var result = new CustomFileDetails();

            // Check whether license is custom
            var isCustom = license.CustomLicenseID != null;

            if (isCustom)
            {
                // Get content for custom license
                result = GetCustomLicenseForDownload(license.CustomLicenseID.Value);
            }
            else
            {
                // Get schema file
                var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schemaId);

                // Setup url to download schema
                var urlToSchema = _urls.ToDownloadSchema(schemaFile.ID);

                var application = _applications.FirstOrDefault(i => i.ID == license.ApplicationID);

                // Get content for templated license
                result = GetTemplatedLicenseForDownload(license, application.OrganizationID, urlToSchema);
            }

            // Return result
            return result;
        }

        private CustomFileDetails GetLicenseFileInfo(int schemaId, int licenseId, int providerOrganizationId, int consumerOrganizationId)
        {
            // Get license
            var license = _licenses.FirstOrDefault(i => i.ID == licenseId);
            var result = new CustomFileDetails();

            // Check whether license is custom
            var isCustom = license.CustomLicenseID != null;

            if (isCustom)
            {
                // Get content for custom license
                result = GetCustomLicenseForDownload(license.CustomLicenseID.Value);
            }
            else
            {
                // Get schema file
                var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schemaId);

                // Setup url to download schema
                var urlToSchema = _urls.ToDownloadSchema(schemaFile.ID);

                var application = _applications.FirstOrDefault(i => i.ID == license.ApplicationID);

                // Get content for templated license
                result = GetTemplatedLicenseForDownload(license, application.OrganizationID, consumerOrganizationId, urlToSchema);
            }

            // Return result
            return result;
        }
    }
}
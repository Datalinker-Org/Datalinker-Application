using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Configuration;
using DataLinker.Services.Emails;
using DataLinker.Services.Exceptions;
using DataLinker.Services.LicenseContent;
using DataLinker.Services.Security;
using DataLinker.Services.Tokens;
using DataLinker.Services.Urls;
using Rezare.CommandBuilder.Services;
using System;
using System.Linq;
using DataLinker.Services.Emails.Roles.User;

namespace DataLinker.Services.LicenseVerification
{
    internal class LicenseVerificationService : ILicenseVerificationService
    {
        private ITokenService _tokens;
        private IService<OrganizationLicense, int> _orgLicenses;
        private IService<LicenseApprovalRequest, int> _verificationRequests;
        private IService<User, int> _users;
        private IConfigurationService _config;
        private IService<Organization, int> _organisations;
        private ILicenseContentBuilder _licenseContent;
        private IUrlProvider _urls;
        private IService<SchemaFile, int> _schemaFiles;
        private IService<DataSchema, int> _dataSchemas;
        private IService<LicenseMatch, int> _licenseMatches;
        private ISecurityService _security;
        private INotificationService _notifications;
        private IUserNotificationService _userNotifications;

        private DateTime GetDate => DateTime.UtcNow;

        public LicenseVerificationService(ITokenService tokens,
            IService<OrganizationLicense, int> orgLicenses,
            IService<User, int> users,
            IService<Organization, int> organisations,
            IService<DataSchema, int> dataSchemas,
            IConfigurationService config,
            ILicenseContentBuilder licenseContent,
            IUrlProvider urls,
            IService<SchemaFile, int> schemaFiles,
            ISecurityService security,
            INotificationService notifications,
            IService<LicenseApprovalRequest, int> verificationRequests,
            IService<LicenseMatch, int> licenseMatches,
            IUserNotificationService userNotifications)
        {
            _tokens = tokens;
            _orgLicenses = orgLicenses;
            _verificationRequests = verificationRequests;
            _users = users;
            _organisations = organisations;
            _config = config;
            _licenseContent = licenseContent;
            _urls = urls;
            _schemaFiles = schemaFiles;
            _security = security;
            _dataSchemas = dataSchemas;
            _notifications = notifications;
            _licenseMatches = licenseMatches;
            _userNotifications = userNotifications;
        }

        public LicenseConfirmModel GetConfirmModel(string token, LoggedInUserDetails user)
        {
            OrganizationLicense license = null;
            LicenseApprovalRequest request = null;
            try
            {
                // Check access
                _security.CheckBasicAccess(user);

                // Process token
                var tokenInfo = _tokens.ParseLicenseVerificationToken(token);

                // Get license
                license = _orgLicenses.FirstOrDefault(i=>i.ID == tokenInfo.ID.Value);

                // Get request
                request = _verificationRequests.FirstOrDefault(i => i.Token == tokenInfo.Token);

                // Check whether token exists
                if (request == null)
                {
                    throw new BaseException("Access denied.");
                }

                // Check whether token belongs to user
                if (request.SentTo != user.ID.Value)
                {
                    request.ExpiresAt = GetDate;
                    _verificationRequests.Update(request);
                    throw new BaseException("Access denied.");
                }

                // Check whether token expired
                if (request.ExpiresAt != tokenInfo.TokenExpire || request.ExpiresAt < DateTime.Now)
                {
                    throw new BaseException(
                        "Approval link is expired.");
                }

                // Check whether user is Legal officer for organisation
                CheckForLegalOfficer(user);

                // Check whether organisation is active
                if (!user.Organization.IsActive)
                {
                    throw new BaseException(
                        "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker team.");
                }

                // Check whether licese is pending approval
                if (license.Status != (int)PublishStatus.PendingApproval)
                {
                    throw new BaseException("This license is not pending approval.");
                }

                // Get organisation details
                var organization = _organisations.FirstOrDefault(i=>i.ID == user.Organization.ID);
                
                // Determine license type
                var type = GetType(license);

                // Get content for license
                var licenseContent = GetLicenseContent(license, organization.ID, type);

                // Setup result
                var result = new LicenseConfirmModel
                {
                    OrganizationName = organization.Name,
                    ID = license.ID,
                    LicenseContent = licenseContent,
                    Type = type,
                    IsProvider = license.ProviderEndpointID != 0
                };

                // Return result
                return result;
            }
            catch (BaseException)
            {
                // Set license status to draft
                if (license != null && license.Status == (int)PublishStatus.PendingApproval)
                {
                    license.Status = (int)PublishStatus.Draft;
                    _orgLicenses.Update(license);
                }
                throw;
            }
            finally
            {
                // Always expire request
                if (request != null)
                {
                    request.ExpiresAt = GetDate;
                    _verificationRequests.Update(request);
                }
            }
        }

        public void Approve(int licenseId, string urlToLicenses, LoggedInUserDetails user)
        {
            // Check access
            var checkResult = CheckAccess(licenseId, user);

            // Setup license details
            checkResult.License.ApprovedAt = GetDate;
            checkResult.License.ApprovedBy = user.ID;
            checkResult.License.Status = (int)PublishStatus.ReadyToPublish;
            _orgLicenses.Update(checkResult.License);

            // Get organisation
            var organizaiton = _organisations.FirstOrDefault(i=>i.ID == user.Organization.ID);

            // Get schema
            var schema = _dataSchemas.FirstOrDefault(i=>i.ID == checkResult.License.DataSchemaID);

            var isProvider = checkResult.License.ProviderEndpointID != 0;

            // Notify user, who made an approval request
            _notifications.User.StatusForLicenseUpdatedInBackground(checkResult.VerificationRequest.SentBy,
                urlToLicenses, schema.Name, organizaiton.Name, isProvider, checkResult.License.Status);
            
            // Get schema file
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schema.ID);

            // Setup url to download schema
            var urlToDownloadSchema = _urls.ToDownloadSchema(schemaFile.ID);

            // Notify legal officer about approved license
            _notifications.LegalOfficer.LicenseApprovedSuccessfullyInBackground(user.ID.Value,
                urlToDownloadSchema, _config.DataLinkerHost, schema.Name, checkResult.License.ID, isProvider);
        }

        public void Decline(int licenseId, string urlToLicenses, LoggedInUserDetails user)
        {
            // Check whether user has access
            var checkResult = CheckAccess(licenseId, user);

            // Get organisation
            var organizaiton = _organisations.FirstOrDefault(i=>i.ID == user.Organization.ID);

            // Get schema
            var schema = _dataSchemas.FirstOrDefault(i=>i.ID == checkResult.License.DataSchemaID);

            // Check whether this is data provider
            var isProvider = checkResult.License.ProviderEndpointID != 0;
            
            // Update license
            checkResult.License.Status = (int)PublishStatus.Draft;
            _orgLicenses.Update(checkResult.License);

            // Notify user about status update
            _notifications.User.StatusForLicenseUpdatedInBackground(checkResult.VerificationRequest.SentBy,
                urlToLicenses, schema.Name, organizaiton.Name, isProvider, checkResult.License.Status);
        }

        private LicenseCheckResult CheckAccess(int licenseId, LoggedInUserDetails user)
        {
            var result = new LicenseCheckResult();

            // Check whether user is legal officer
            var legalOfficer = CheckForLegalOfficer(user);

            // Get license
            var license = _orgLicenses.FirstOrDefault(i=>i.ID==licenseId);

            // Check whether license exists
            if(license == null)
            {
                throw new BaseException($"License {licenseId} not found");
            }

            // Check whether license has valid satus
            if (license.Status != (int)PublishStatus.PendingApproval)
            {
                throw new BaseException("This license is not pending approval.");
            }

            // Get verification request
            var request = _verificationRequests.Where(i => i.OrganizationLicenseID == licenseId)
                    .FirstOrDefault(i => i.SentTo == user.ID.Value);

            // Check whether request exists
            if (request == null)
            {
                throw new BaseException("Access denied.");
            }

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Setup result
            result.LegalOfficer = legalOfficer;
            result.License = license;
            result.VerificationRequest = request;

            // Return result
            return result;
        }

        private User CheckForLegalOfficer(LoggedInUserDetails user)
        {
            // Check whether user is Legal officer for organisation
            var legalOfficer = _users.Where(i => i.OrganizationID == user.Organization.ID)
                .FirstOrDefault(p => p.ID == user.ID && p.IsIntroducedAsLegalOfficer == true && p.IsVerifiedAsLegalOfficer == true);
            if (legalOfficer == null)
            {
                throw new BaseException("Access denied.");
            }

            return legalOfficer;
        }

        private string GetLicenseContent(OrganizationLicense licenseToProceed, int organisationId, OrganisationLicenseType type)
        {
            // Define result
            var result = string.Empty;
            OrganizationLicense license = licenseToProceed;
            // Check whether license is for consumer
            if (licenseToProceed.ProviderEndpointID == 0)
            {
                // Get license match
                var licenseMatch = _licenseMatches.FirstOrDefault(i => i.ConsumerLicenseID == licenseToProceed.ID);
                // Use provider license for generating content
                license = _orgLicenses.GetById(licenseMatch.ProviderLicenseID);
            }

            // Setup license content for license type
            switch (type)
            {
                case OrganisationLicenseType.FromTemplate:
                    {
                        // Get schema file
                        var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == license.DataSchemaID);

                        // Setup url to schema
                        var urlToSchema = _urls.ToDownloadSchema(schemaFile.ID);

                        // Get content for license
                        var licenseDocument = _licenseContent.GetLicenseContent(organizationLicenseId: license.ID);

                        // Setup license content with details
                        _licenseContent.InsertLicenseDetails(licenseDocument, urlToSchema, _config.DataLinkerHost, organisationId, license.ProviderEndpointID != 0);

                        result = licenseDocument.OuterXml;
                        break;
                    }
                case OrganisationLicenseType.Custom:
                    {
                        // Get content for license
                        var urlToDownloadLicese = _urls.ToDownloadLicense(license.ApplicationID, license.DataSchemaID, license.ID);
                        result = urlToDownloadLicese;
                        break;
                    }
            }

            // return result
            return result;
        }

        private OrganisationLicenseType GetType(OrganizationLicense license)
        {
            // Setup default license type
            var licenseType = OrganisationLicenseType.FromTemplate;

            // Check whether license is custom
            if (license.CustomLicenseID != null)
            {
                licenseType = OrganisationLicenseType.Custom;
            }

            // Return license type
            return licenseType;
        }

        public LicenseConfirmModel GetLicenseForProviderAndSchema(int providerId, int schemaId, LoggedInUserDetails user)
        {
            try
            {
                CheckForLegalOfficer(user);
            }
            catch (BaseException ex)
            {
                // TODO: send request off to the consumers legal officer

                return null;
            }

            var license = _orgLicenses.FirstOrDefault(i => i.ID == providerId && i.DataSchemaID == schemaId);

            // Get organisation details
            var organization = _organisations.FirstOrDefault(i => i.ID == user.Organization.ID);

            // Determine license type
            var type = GetType(license);

            // Get content for license
            var licenseContent = GetLicenseContent(license, organization.ID, type);

            // Setup result
            var result = new LicenseConfirmModel
            {
                OrganizationName = organization.Name,
                ID = license.ID,
                LicenseContent = licenseContent,
                Type = type,
                IsProvider = false
            };

            return result;
        }

        public void ConsumerApprove(int licenseId, string urlToLicenses, LoggedInUserDetails user)
        {
            ////TODO: send the request off to the provider
            // Check access
            var checkResult = CheckAccess(licenseId, user);

            // Setup license details
            checkResult.License.ApprovedAt = GetDate;
            checkResult.License.ApprovedBy = user.ID;
            checkResult.License.Status = (int)PublishStatus.ReadyToPublish;
            _orgLicenses.Update(checkResult.License);

            // Get organisation
            var organizaiton = _organisations.FirstOrDefault(i => i.ID == user.Organization.ID);

            // Get schema
            var schema = _dataSchemas.FirstOrDefault(i => i.ID == checkResult.License.DataSchemaID);

            var isProvider = checkResult.License.ProviderEndpointID != 0;

            // Notify user, who made an approval request
            _notifications.User.StatusForLicenseUpdatedInBackground(checkResult.VerificationRequest.SentBy,
                urlToLicenses, schema.Name, organizaiton.Name, isProvider, checkResult.License.Status);

            // Get schema file
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schema.ID);

            // Setup url to download schema
            var urlToDownloadSchema = _urls.ToDownloadSchema(schemaFile.ID);

            // Notify legal officer about approved license
            _notifications.LegalOfficer.LicenseApprovedSuccessfullyInBackground(user.ID.Value,
                urlToDownloadSchema, _config.DataLinkerHost, schema.Name, checkResult.License.ID, isProvider);
        }

        public void ConsumerDecline(int licenseId, string urlToLicenses, LoggedInUserDetails user)
        {
            ////TODO: don't send the request to the provider
            // Check whether user has access
            var checkResult = CheckAccess(licenseId, user);

            // Get organisation
            var organizaiton = _organisations.FirstOrDefault(i => i.ID == user.Organization.ID);

            // Get schema
            var schema = _dataSchemas.FirstOrDefault(i => i.ID == checkResult.License.DataSchemaID);

            // Check whether this is data provider
            var isProvider = checkResult.License.ProviderEndpointID != 0;

            // Update license
            checkResult.License.Status = (int)PublishStatus.Draft;
            _orgLicenses.Update(checkResult.License);

            // Notify user about status update
            _notifications.User.StatusForLicenseUpdatedInBackground(checkResult.VerificationRequest.SentBy,
                urlToLicenses, schema.Name, organizaiton.Name, isProvider, checkResult.License.Status);
        }
    }
}

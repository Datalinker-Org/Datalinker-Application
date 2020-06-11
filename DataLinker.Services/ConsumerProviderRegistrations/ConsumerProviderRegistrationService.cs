using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Applications;
using DataLinker.Models.ConsumerProviderRegistration;
using DataLinker.Models.Enums;
using DataLinker.Services.Configuration;
using DataLinker.Services.Emails;
using DataLinker.Services.Exceptions;
using DataLinker.Services.LicenseContent;
using DataLinker.Services.OrganizationLicenses;
using DataLinker.Services.Security;
using DataLinker.Services.Tokens;
using DataLinker.Services.Urls;
using Rezare.CommandBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Services.ConsumerProviderRegistrations
{
    internal class ConsumerProviderRegistrationService : IConsumerProviderRegistrationService
    {
        private IService<ProviderEndpoint, int> _endpointService;
        private IService<OrganizationLicense, int> _licenseService;
        private IService<OrganizationLicenseClause, int> _orgLicenseClauses;
        private IService<LicenseClause, int> _licenseClauses;
        private IService<LicenseSection, int> _licenseSections;
        private IService<LicenseClauseTemplate, int> _licenseClauseTemplates;
        private IService<DataSchema, int> _schemaService;
        private IService<SchemaFile, int> _schemaFiles;
        private IService<Application, int> _application;
        private IService<User, int> _users;
        private IService<LicenseApprovalRequest, int> _verificationRequests;

        private ITokenService _tokens;
        private IService<ConsumerProviderRegistration, int> _consumerProviderRegistration;
        private IOrganizationLicenseService _organizationLicenseService;
        private IUrlProvider _urls;
        private INotificationService _notificationService;
        private IConfigurationService _config;
        private ILicenseContentBuilder _licenseContent;
        IService<Organization, int> _organisations;
        private ISecurityService _security;

        public ConsumerProviderRegistrationService(IService<ProviderEndpoint, int> endpointService,
            IService<OrganizationLicense, int> licenseService,
            IService<OrganizationLicenseClause, int> orgLicenseClauses,
            IService<LicenseClause, int> licenseClauses,
            IService<LicenseSection, int> licenseSections,
            IService<LicenseClauseTemplate, int> licenseClauseTemplates,
            IService<DataSchema, int> schemaService,
            IService<SchemaFile, int> schemaFiles,
            IService<Application, int> application,
            IService<LicenseApprovalRequest, int> verificationRequests,
            ITokenService tokens,
            IService<User, int> users,
            IService<ConsumerProviderRegistration, int> consumerProviderRegistration,
            IOrganizationLicenseService organizationLicenseService,
            IUrlProvider urls,
            INotificationService notificationService,
            IConfigurationService config,
            ILicenseContentBuilder licenseContent,
            IService<Organization, int> organisations,
            ISecurityService security)
        {
            _endpointService = endpointService;
            _licenseService = licenseService;
            _orgLicenseClauses = orgLicenseClauses;
            _licenseClauses = licenseClauses;
            _licenseSections = licenseSections;
            _licenseClauseTemplates = licenseClauseTemplates;
            _schemaService = schemaService;
            _schemaFiles = schemaFiles;
            _application = application;
            _verificationRequests = verificationRequests;
            _consumerProviderRegistration = consumerProviderRegistration;
            _organizationLicenseService = organizationLicenseService;
            _users = users;
            _tokens = tokens;
            _urls = urls;
            _notificationService = notificationService;
            _config = config;
            _licenseContent = licenseContent;
            _organisations = organisations;
            _security = security;
        }

        //public List<ProviderModel> GetProvidersBySchemaId(int consumerAppId, int schemaId)
        public SchemaProviderVm GetProvidersBySchemaId(int consumerAppId, int schemaId)
        {
            //var providers = new List<ProviderModel>();
            var providers = new List<ProviderVm>();

            var schema = _schemaService.FirstOrDefault(p => p.ID == schemaId);

            var endpoints = _endpointService.Where(p => p.DataSchemaID == schemaId);

            foreach (var end in endpoints)
            {
                var application = _application.FirstOrDefault(p => p.ID == end.ApplicationId);
                var providerLicense = _licenseService.FirstOrDefault(p => p.DataSchemaID == schemaId
                                                                                            && p.ProviderEndpointID == end.ID
                                                                                            && p.ApplicationID == end.ApplicationId 
                                                                                            && p.Status == (int) PublishStatus.Published);

                if(providerLicense == null)
                {
                    continue;
                }

                var licenseClauses = new List<LicenseClauseVm>();

                var orgClauses = _orgLicenseClauses.Where(p => p.OrganizationLicenseID == providerLicense.ID);
                foreach(var orgClause in orgClauses)
                {
                    // get title.
                    var licClause = _licenseClauses.GetById(orgClause.LicenseClauseID);
                    var licSection = _licenseSections.GetById(licClause.LicenseSectionID);

                    var licClauseTemplate = _licenseClauseTemplates.FirstOrDefault(p => p.LicenseClauseID == orgClause.LicenseClauseID);


                    var licenseClause = new LicenseClauseVm()
                    {
                        Title = licSection.Title,
                        OrgText = orgClause.ClauseData.Replace("{", string.Empty).Replace("}", string.Empty).Replace("each", "per"),
                        ShortText = licClauseTemplate.ShortText,
                        LegalText = licClauseTemplate.LegalText
                    };
                    licenseClauses.Add(licenseClause);
                }
                
                // Chech whether the consumer application has already subscribed to the provider.
                var status = ConsumerProviderRegistrationStatus.NotRegistered;
                var remarks = "";
                var registration = _consumerProviderRegistration.FirstOrDefault(p => p.OrganizationLicenseID == providerLicense.ID && p.ConsumerApplicationID == consumerAppId);
                if(registration != null)
                {
                    status = (ConsumerProviderRegistrationStatus) registration.Status;
                    remarks = registration.Remarks;
                }


                //var provider = new ProviderModel()
                //{
                //    SchemaId = schema.ID,
                //    SchemaName = schema.Name,
                //    ApplicationId = application.ID,
                //    ApplicationName = application.Name,
                //    LicenseId = providerLicense.ID,
                //    Status = status
                //};
                //providers.Add(provider);

                var provider = new ProviderVm()
                {
                    ApplicationId = application.ID,
                    ApplicationName = application.Name,
                    LicenseId = providerLicense.ID,
                    Status = status,
                    Remarks = remarks,
                    LicenseClauses = licenseClauses
                };
                providers.Add(provider);

            }

            var schemaProviders = new SchemaProviderVm
            {
                SchemaId = schema.ID,
                SchemaName = schema.Name,
                Providers = providers,
            };
            //return providers;
            return schemaProviders;
        }

        public void RequestForAccess(int consumerAppId, int providerLicenseId, LoggedInUserDetails user)
        {
            var registeredProvider =_consumerProviderRegistration.Where(p => p.ConsumerApplicationID == consumerAppId && p.OrganizationLicenseID == providerLicenseId).SingleOrDefault();
            if(registeredProvider == null)
            {
                registeredProvider = new ConsumerProviderRegistration()
                {
                    ConsumerApplicationID = consumerAppId,
                    OrganizationLicenseID = providerLicenseId,
                    Status = (int)ConsumerProviderRegistrationStatus.PendingConsumerApproval,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = user.ID.Value
                };
                _consumerProviderRegistration.Add(registeredProvider);
            } else
            {
                registeredProvider.Status = (int)ConsumerProviderRegistrationStatus.PendingConsumerApproval;
                _consumerProviderRegistration.Update(registeredProvider);
            }

            var license = _licenseService.FirstOrDefault(p => p.ID == providerLicenseId);

            // Get legal officers for organisation
            var legalOfficers = _users.Where(i => i.OrganizationID == user.Organization.ID)
                .Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true)
                .ToList();
                        
            // Get schema
            var schema = _schemaService.FirstOrDefault(i => i.ID == license.DataSchemaID);

            // Get schema file
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schema.ID);

            // Setup url to download schema
            var urlToSchema = _urls.ToDownloadSchema(schemaFile.ID);

            // Send notification to legal officers.
            SendNotificationToLegalOfficers(license, registeredProvider.ID, legalOfficers, schema.Name, urlToSchema, user, false);
        }

        public LegalApprovalModel GetLegalApprovalModel(string token, LoggedInUserDetails user)
        {
            _security.CheckBasicAccess(user);
            var tokenInfo =_tokens.ParseConsumerProviderRegistrationToken(token);


            // Get request
            var request = _verificationRequests.FirstOrDefault(i => i.Token == tokenInfo.Token);
            if(request == null)
            {
                throw new BaseException("Access denied. Request token does not exist.");
            }

            if(request.SentTo != user.ID.Value)
            {
                throw new BaseException("Access denied. Invalid user.");
            }

            // Check whether token expired
            if (request.ExpiresAt != tokenInfo.TokenExpire || request.ExpiresAt < DateTime.Now)
            {
                throw new BaseException("Approval link is expired.");
            }

            if (!IsLegalOfficer(user)) {
                throw new BaseException("Access denied. Not a legal officer.");
            }

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException("Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker team.");
            }

            // Get license
            var license = _licenseService.FirstOrDefault(p => p.ID == tokenInfo.ID.Value);

            // Get license type whether it is custom or default.
            var licenseType = license.CustomLicenseID != null ? OrganisationLicenseType.Custom : OrganisationLicenseType.FromTemplate;
            var result = string.Empty;
            // Get organisation details
            var organization = _organisations.FirstOrDefault(i => i.ID == user.Organization.ID);
            switch (licenseType)
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
                        _licenseContent.InsertLicenseDetails(licenseDocument, urlToSchema, _config.DataLinkerHost, organization.ID, license.ProviderEndpointID != 0);

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

            // Always expire request
            if (request != null)
            {
                request.ExpiresAt = DateTime.UtcNow;
                _verificationRequests.Update(request);
            }

            var legalApprovalModel = new LegalApprovalModel
            {
                ConsumerProviderRegistrationID = tokenInfo.ConsumerProviderRegistrationId.Value,
                LicenseContent = result,
                Type = licenseType
            };

            return legalApprovalModel;
        }

        public LegalApprovalModel GetLegalApprovalModel(int consumerProviderRegistrationId, LoggedInUserDetails user)
        {
            _security.CheckBasicAccess(user);
            var providerRegistration = _consumerProviderRegistration.FirstOrDefault(p => p.ID == consumerProviderRegistrationId);
            
            // Get license
            var license = _licenseService.FirstOrDefault(p => p.ID == providerRegistration.OrganizationLicenseID);

            // Get license type whether it is custom or default.
            var licenseType = license.CustomLicenseID != null ? OrganisationLicenseType.Custom : OrganisationLicenseType.FromTemplate;
            var result = string.Empty;
            // Get organisation details
            var organization = _organisations.FirstOrDefault(i => i.ID == user.Organization.ID);
            switch (licenseType)
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
                        _licenseContent.InsertLicenseDetails(licenseDocument, urlToSchema, _config.DataLinkerHost, organization.ID, license.ProviderEndpointID != 0);

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

            var consumerLegalApprovalModel = new LegalApprovalModel
            {
                ConsumerProviderRegistrationID = consumerProviderRegistrationId,
                LicenseContent = result,
                Type = licenseType
            };

            return consumerLegalApprovalModel;
        }

        public ConsumerProviderRegistrationDetail ApproveByConsumerLegal(int consumerProviderRegistrationId, LoggedInUserDetails user)
        {
            if(!user.IsLegalRep)
            {
                throw new BaseException("Access denied. Not a legal officer.");
            }
            var providerRegistration = _consumerProviderRegistration.FirstOrDefault(p => p.ID == consumerProviderRegistrationId);

            if (providerRegistration == null) throw new BaseException("Registration request is not valid.");
            _security.CheckAccessToApplication(user, providerRegistration.ConsumerApplicationID);
            if (providerRegistration.Status != (int)ConsumerProviderRegistrationStatus.PendingConsumerApproval) throw new BaseException("The request is already approved.");

            providerRegistration.ApprovedAt = DateTime.UtcNow;
            providerRegistration.ApprovedBy = user.ID.Value;
            providerRegistration.Status = (int)ConsumerProviderRegistrationStatus.PendingProviderApproval;
            _consumerProviderRegistration.Update(providerRegistration);


            // Find consumer application details.
            var consumerApp = _application.FirstOrDefault(p => p.ID == providerRegistration.ConsumerApplicationID);

            // Send notification to provider legal.
            // Find provider legal.
            var providerLicense = _licenseService.FirstOrDefault(p => p.ID == providerRegistration.OrganizationLicenseID);
            var providerApp = _application.FirstOrDefault(p => p.ID == providerLicense.ApplicationID);
            var legalOfficers = _users.Where(p => p.OrganizationID == providerApp.OrganizationID)
                .Where(p => p.IsActive == true && p.IsIntroducedAsLegalOfficer == true && p.IsVerifiedAsLegalOfficer == true)
                .ToList();

            // Get schema
            var schema = _schemaService.FirstOrDefault(i => i.ID == providerLicense.DataSchemaID);
            // Get schema file
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schema.ID);
            // Setup url to download schema
            var urlToSchema = _urls.ToDownloadSchema(schemaFile.ID);

            // Send notification to legal officers.
            SendNotificationToLegalOfficers(providerLicense, consumerProviderRegistrationId, legalOfficers, schema.Name, urlToSchema, user, true);

            return new ConsumerProviderRegistrationDetail() {
                ID = providerRegistration.ID,
                ConsumerApplicationID = providerRegistration.ConsumerApplicationID,
                ConsumerApplicationName = consumerApp.Name,
                ProviderApplicationID = providerApp.ID,
                ProviderApplicationName = providerApp.Name,
                SchemaID = providerLicense.DataSchemaID,
                SchemaName = schema.Name
            };
        }

        public ConsumerProviderRegistrationDetail DeclineByConsumerLegal(int consumerProviderRegistrationId, string declineReason, LoggedInUserDetails user)
        {
            if (!user.IsLegalRep)
            {
                throw new BaseException("Access denied. Not a legal officer.");
            }
            var providerRegistration = _consumerProviderRegistration.FirstOrDefault(p => p.ID == consumerProviderRegistrationId);
            if (providerRegistration == null) throw new BaseException("Registration request is not valid.");
            _security.CheckAccessToApplication(user, providerRegistration.ConsumerApplicationID);

            var providerLicense = _licenseService.FirstOrDefault(p => p.ID == providerRegistration.OrganizationLicenseID);

            providerRegistration.DeclinedAt = DateTime.UtcNow;
            providerRegistration.DeclinedBy = user.ID.Value;
            providerRegistration.Status = (int)ConsumerProviderRegistrationStatus.Declined;
            providerRegistration.Remarks = declineReason;
            _consumerProviderRegistration.Update(providerRegistration);

            // Get schema
            var schema = _schemaService.FirstOrDefault(i => i.ID == providerLicense.DataSchemaID);

            // Send email to consumer staff. Consumer staff is one who created the request.
            _notificationService.ConsumerProviderRegistration.ConsumerRegistrationDeclineByProviderLegalRequestInBackground(providerRegistration.CreatedBy, providerRegistration.ID, schema.Name);

            return new ConsumerProviderRegistrationDetail
            {
                ID = providerRegistration.ID,
                ConsumerApplicationID = providerRegistration.ConsumerApplicationID,
                ProviderApplicationID = providerLicense.ApplicationID,
                SchemaID = providerLicense.DataSchemaID
            };
        }

        public ConsumerProviderRegistrationDetail ApproveByProviderLegal(int consumerProviderRegistrationId, LoggedInUserDetails user)
        {
            if (!user.IsLegalRep)
            {
                throw new BaseException("Access denied. Not a legal officer.");
            }
            var providerRegistration = _consumerProviderRegistration.FirstOrDefault(p => p.ID == consumerProviderRegistrationId);
            if (providerRegistration == null) throw new BaseException("Registration request is not valid.");

            var providerLicense = _licenseService.FirstOrDefault(p => p.ID == providerRegistration.OrganizationLicenseID);
            _security.CheckAccessToApplication(user, providerLicense.ApplicationID);

            var provApp = _application.FirstOrDefault(p => p.ID == providerLicense.ApplicationID);
            

            providerRegistration.ProviderApprovedAt = DateTime.UtcNow;
            providerRegistration.ProviderApprovedBy = user.ID.Value;
            providerRegistration.Status = (int)ConsumerProviderRegistrationStatus.Approved;
            _consumerProviderRegistration.Update(providerRegistration);

            
            // Get schema information
            var schema = _schemaService.FirstOrDefault(i => i.ID == providerLicense.DataSchemaID);
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schema.ID);
            var linkToSchema = _urls.ToDownloadSchema(schemaFile.ID);

            // Get datalinker information
            var linkToDataLinker = _config.DataLinkerHost;

            // Send email provider legal
            var provLegals = GetLegalOfficers(provApp.OrganizationID);
            foreach(var legal in provLegals) { 
                _notificationService.ConsumerProviderRegistration.ProviderLegalApproveRequestInBackground(legal.ID, providerRegistration.ID, schema.Name, linkToSchema, linkToDataLinker);
            }

            // Send email to consumer legal
            var consApp = _application.FirstOrDefault(p => p.ID == providerRegistration.ConsumerApplicationID);
            var consLegals = GetLegalOfficers(consApp.OrganizationID);
            foreach(var legal in consLegals)
            {
                _notificationService.ConsumerProviderRegistration.ConsumerRegistrationCompletionRequestInBackground(legal.ID, providerRegistration.ID, schema.Name, linkToSchema, linkToDataLinker);
            }

            //// Send email to consumer requested the service.
            //_notificationService.ConsumerProviderRegistration.ConsumerRegistrationCompletionRequestInBackground(providerRegistration.CreatedBy, providerRegistration.ID, schema.Name, linkToSchema, linkToDataLinker);


            return new ConsumerProviderRegistrationDetail
            {
                ID = providerRegistration.ID,
                ConsumerApplicationID = providerRegistration.ConsumerApplicationID,
                ProviderApplicationID = provApp.ID,
                ProviderApplicationName = provApp.Name,
                SchemaID = providerLicense.DataSchemaID,
                SchemaName = schema.Name
            };
        }

        public ConsumerProviderRegistrationDetail DeclineByProviderLegal(int consumerProviderRegistrationId, string declineReason, LoggedInUserDetails user)
        {
            if (!user.IsLegalRep)
            {
                throw new BaseException("Access denied. Not a legal officer.");
            }
            var providerRegistration = _consumerProviderRegistration.FirstOrDefault(p => p.ID == consumerProviderRegistrationId);
            if (providerRegistration == null) throw new BaseException("Registration request is not valid.");

            var providerLicense = _licenseService.FirstOrDefault(p => p.ID == providerRegistration.OrganizationLicenseID);
            _security.CheckAccessToApplication(user, providerLicense.ApplicationID);

            providerRegistration.DeclinedAt = DateTime.UtcNow;
            providerRegistration.DeclinedBy = user.ID.Value;
            providerRegistration.Status = (int)ConsumerProviderRegistrationStatus.Declined;
            providerRegistration.Remarks = declineReason;
            _consumerProviderRegistration.Update(providerRegistration);

            // Get schema information
            var schema = _schemaService.FirstOrDefault(i => i.ID == providerLicense.DataSchemaID);
            var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schema.ID);
            var linkToSchema = _urls.ToDownloadSchema(schemaFile.ID);

            // Get datalinker information
            var linkToDataLinker = _config.DataLinkerHost;

            // Send email to consumer legal
            var consApp = _application.FirstOrDefault(p => p.ID == providerRegistration.ConsumerApplicationID);
            var consLegals = GetLegalOfficers(consApp.OrganizationID);
            foreach (var legal in consLegals)
            {
                _notificationService.ConsumerProviderRegistration.ConsumerRegistrationDeclineByProviderLegalRequestInBackground(legal.ID, providerRegistration.ID, schema.Name);
            }

            //// Send email to consumer requested the service
            //_notificationService.ConsumerProviderRegistration.ConsumerRegistrationDeclineByProviderLegalRequestInBackground(providerRegistration.CreatedBy, providerRegistration.ID, schema.Name);

            return new ConsumerProviderRegistrationDetail
            {
                ID = providerRegistration.ID,
                ConsumerApplicationID = providerRegistration.ConsumerApplicationID,
                ProviderApplicationID = providerLicense.ApplicationID,
                SchemaID = providerLicense.DataSchemaID
            };
        }

        public ConsumerProviderRegistrationDetail GetConsumerProviderRegistrationDetail(int consumerProviderRegistrationId, LoggedInUserDetails user)
        {
            var providerRegistration = _consumerProviderRegistration.FirstOrDefault(p => p.ID == consumerProviderRegistrationId);
            if (providerRegistration == null) throw new BaseException("Registration request is not valid.");

            var providerLicense = _licenseService.FirstOrDefault(p => p.ID == providerRegistration.OrganizationLicenseID);
            var providerApp = _application.FirstOrDefault(p => p.ID == providerLicense.ApplicationID);
            var schema = _schemaService.FirstOrDefault(i => i.ID == providerLicense.DataSchemaID);

            return new ConsumerProviderRegistrationDetail
            {
                ID = providerRegistration.ID,
                ConsumerApplicationID = providerRegistration.ConsumerApplicationID,
                ProviderApplicationID = providerLicense.ApplicationID,
                ProviderApplicationName = providerApp.Name,
                SchemaID = providerLicense.DataSchemaID,
                SchemaName = schema.Name
            };
        }

        private void SendNotificationToLegalOfficers(OrganizationLicense license, int consumerProviderRegistrationId, List<User> legalOfficers, string schemaName, string urlToSchema, LoggedInUserDetails user, bool toProviderLegal)
        {
            // Send notification to legal officers
            foreach (var legalOfficer in legalOfficers)
            {
                // Generate secure token
                //var tokenInfo = _tokens.GenerateLicenseVerificationToken(legalOfficer.ID, license.ID);
                var tokenInfo = _tokens.GenerateConsumerProviderRegistrationToken(legalOfficer.ID, license.ID, consumerProviderRegistrationId);

                // Create approval request
                var request = new LicenseApprovalRequest
                {
                    accessToken = tokenInfo.TokenInfoEncoded,
                    Token = tokenInfo.Token,
                    ExpiresAt = tokenInfo.TokenExpire.Value,
                    OrganizationLicenseID = license.ID,
                    SentAt = DateTime.UtcNow,
                    SentBy = user.ID.Value,
                    SentTo = legalOfficer.ID
                };

                // Save request
                _verificationRequests.Add(request);

                // Setup url to confirmation screen
                // var urlToConfirmScreen = _urls.ToLicenseVerification(license.ApplicationID, license.DataSchemaID, tokenInfo.TokenInfoEncoded);
                var urlToConfirmScreen = $"{_config.DataLinkerHost}/consumer-provider-registration/{consumerProviderRegistrationId}/consumer-legal-approval?token={tokenInfo.TokenInfoEncoded}";
                if (toProviderLegal)
                {
                    urlToConfirmScreen = $"{_config.DataLinkerHost}/consumer-provider-registration/{consumerProviderRegistrationId}/provider-legal-approval?token={tokenInfo.TokenInfoEncoded}";
                } 

                if(license.LicenseTemplateID != null)
                {
                    urlToConfirmScreen = $"{urlToConfirmScreen}#Clauses";
                }

                // Send notificaion to legal officer with templated license
                //if (license.LicenseTemplateID != null)
                //{
                    if(toProviderLegal)
                    {
                        _notificationService.ConsumerProviderRegistration.ProviderLegalApprovalRequestInBackground(legalOfficer.ID, urlToConfirmScreen, urlToSchema, _config.DataLinkerHost, schemaName, consumerProviderRegistrationId);
                    } else
                    {
                        _notificationService.ConsumerProviderRegistration.ConsumerLegalApprovalRequestInBackground(legalOfficer.ID, urlToConfirmScreen, urlToSchema, _config.DataLinkerHost, schemaName, consumerProviderRegistrationId);
                    }
                    
                //}

                //// Send notificaion to legal officer with custom license
                //if (license.CustomLicenseID != null)
                //{
                //    if(toProviderLegal)
                //    {
                //        _notificationService.ConsumerProviderRegistration.ProviderLegalCustomLicenseApprovalRequestInBackground(legalOfficer.ID, urlToConfirmScreen, schemaName, license.ID);
                //    }
                //    else
                //    {
                //        _notificationService.ConsumerProviderRegistration.ConsumerLegalCustomLicenseApprovalRequestInBackground(legalOfficer.ID, urlToConfirmScreen, schemaName, license.ID);
                //    }
                //}
            }
        }

        private bool IsLegalOfficer(LoggedInUserDetails user)
        {
            var legalOfficer = _users.Where(i => i.OrganizationID == user.Organization.ID)
                .FirstOrDefault(p => p.ID == user.ID && p.IsIntroducedAsLegalOfficer == true && p.IsVerifiedAsLegalOfficer == true);
            if (legalOfficer == null)
            {
                return false;
            }
            return true;
        }

        private IEnumerable<User> GetLegalOfficers(int organizationId)
        {
            return _users.Where(p => p.OrganizationID == organizationId)
                .Where(p => p.IsActive == true && p.IsIntroducedAsLegalOfficer == true && p.IsVerifiedAsLegalOfficer == true);
        }
    }

}

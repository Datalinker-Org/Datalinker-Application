using System;
using System.Collections.Generic;
using System.Linq;
using DataLinker.Services.Configuration;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Mappings;
using DataLinker.Services.Security;
using DataLinker.Services.LicenseContent;
using DataLinker.Services.Emails;
using DataLinker.Services.Tokens;
using DataLinker.Services.Urls;
using Rezare.CommandBuilder.Services;
using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.OrganizationLicenses
{
    internal class OrganizationLicenseService : IOrganizationLicenseService
    {
        private IConfigurationService _config;
        private IService<OrganizationLicense, int> _service;
        private ILicenseMatchesService _licenseMatches;
        private IService<DataSchema, int> _dataSchemaService;
        private IService<Application, int> _applicationService;
        private IService<LicenseClauseTemplate, int> _clauseTemplateService;
        private IService<LicenseSection, int> _sectionService;
        private IService<LicenseClause, int> _clauseService;
        private IService<OrganizationLicenseClause, int> _licenseClauseService;
        private IService<ProviderEndpoint, int> _endpoints;
        private IService<Organization, int> _organisations;
        private IService<LicenseTemplate, int> _licenseTemplates;
        private IService<CustomLicense, int> _customLicenses;
        private IOrganizationLicenseClauseService _licenseClauses;
        private ISecurityService _security;
        private ILicenseContentBuilder _licenseContentBuilder;
        private IService<LicenseApprovalRequest, int> _verificationRequests;
        private IService<LicenseAgreement, int> _agreements;
        private INotificationService _notificationService;
        private ITokenService _tokens;
        private IService<User, int> _users;
        private IUrlProvider _urls;

        private const string orgIsInactiveError = "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.";

        private IService<SchemaFile, int> _schemaFileService;
        private DateTime GetDate => DateTime.UtcNow;

        public OrganizationLicenseService(IService<OrganizationLicense, int> service,
            ILicenseMatchesService matchesService,
            IService<DataSchema, int> schemaService,
            IService<Application, int> applicationService,
            IService<LicenseClauseTemplate, int> clauseTemplates,
            IService<LicenseSection, int> sections,
            IService<LicenseClause, int> clauses,
            IService<ProviderEndpoint, int> endpoints,
            IService<OrganizationLicenseClause, int> orgLicenseClauseService,
            IService<SchemaFile, int> schemaFile,
            IService<Organization, int> organisations,
            IService<LicenseTemplate, int> licenseTemplates,
            ISecurityService security,
            IOrganizationLicenseClauseService licenseClauses,
            IConfigurationService config,
            ILicenseContentBuilder licenseContentBuilder,
            INotificationService notifications,
            IService<CustomLicense, int> customLicenses,
            IService<LicenseApprovalRequest, int> verificationRequests,
            ITokenService tokens,
            IService<User, int> users,
            IService<LicenseAgreement, int> agreements,
            IUrlProvider urls,
            IConfigurationService configuration)
        {
            _service = service;
            _licenseMatches = matchesService;
            _dataSchemaService = schemaService;
            _applicationService = applicationService;
            _clauseTemplateService = clauseTemplates;
            _sectionService = sections;
            _clauseService = clauses;
            _licenseClauseService = orgLicenseClauseService;
            _schemaFileService = schemaFile;
            _licenseClauses = licenseClauses;
            _security = security;
            _endpoints = endpoints;
            _organisations = organisations;
            _licenseTemplates = licenseTemplates;
            _licenseContentBuilder = licenseContentBuilder;
            _customLicenses = customLicenses;
            _config = config;
            _tokens = tokens;
            _verificationRequests = verificationRequests;
            _users = users;
            _agreements = agreements;
            _notificationService = notifications;
            _urls = urls;
        }
        
        public IEnumerable<OrganizationLicense> GetAllProviderLicensesForMonth(DateTime date)
        {
            // Setup start day
            var startDay = new DateTime(date.Year, date.Month, 1);

            // Setup end day
            var endDay = startDay.AddMonths(1).AddDays(-1);

            // Query data
            var result = _service.Where(i => i.CreatedAt >= startDay).Where(i => i.CreatedAt <= endDay && i.ProviderEndpointID != 0);

            // Return data
            return result;
        }
        
        public IEnumerable<OrganizationLicense> GetForApplicationAndSchema(int appId, int schemaId, bool isPublished = false)
        {
            // Setup result
            var result = _service.Where(i => i.ApplicationID == appId).Where(i => i.DataSchemaID == schemaId);

            // Check whether status filter should be apllied
            if(isPublished)
            {
                result = result.Where(i => i.Status == (int)PublishStatus.Published);
            }

            // Return result
            return result;
        }

        public ProviderLicensesModel SetupProviderLicensesModel(int applicationId, int schemaId, LoggedInUserDetails user)
        {
            // Check whether user has apropriate access
            var application = _security.CheckAccessToApplication(user, applicationId);

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(orgIsInactiveError);
            }
            
            // Get licenses
            var licenses = _service.Where(i => i.ApplicationID == applicationId)
                .Where(i => i.Status != (int)PublishStatus.Retracted && i.DataSchemaID == schemaId)
                .ToList();

            // Setup license models
            var licenseModels = SetupLicenses(licenses);

            var isPendingApprovalPresent = licenses.Any(i => i.Status == (int)PublishStatus.PendingApproval);
            var isPublishedLicensePresent = licenses.Any(i => i.Status == (int)PublishStatus.Published);

            // Get schema
            var schema = _dataSchemaService.FirstOrDefault(i=>i.ID == schemaId);

            // Setup result model
            var result = new ProviderLicensesModel();
            result.AnyInVerificationProcess = isPendingApprovalPresent;
            result.AnyPublished = isPublishedLicensePresent;
            result.Items = licenseModels;
            result.SchemaName = schema.Name;
            result.IsProvider = application.IsProvider;

            // Return result
            return result;
        }
        
        public BuildLicenseModel SetupBuildLicenseModel(int applicationId, int schemaId, LoggedInUserDetails user)
        {
            // Check whether user has appropriate access
            var application = _security.CheckAccessToApplication(user, applicationId);

            // Get published provider licenses
            var providerLicenses = _service.Where(i => i.Status == (int)PublishStatus.Published);

            // Filter provider licenses
            if (!application.IsProvider)
            {
                providerLicenses = providerLicenses.Where(i => i.ProviderEndpointID != 0).ToList();
            }

            // Check whether any provider has published license to match
            var isPublishedLicensePresent = providerLicenses.Any();

            // Setup schema name
            var schemaName = _dataSchemaService.FirstOrDefault(i=>i.ID == schemaId).Name;

            // Setup basic model details
            var result = new BuildLicenseModel()
            {
                SchemaName = schemaName,
                IsProvider = application.IsProvider,
                IsPublishedProviderLicensePresent = isPublishedLicensePresent,
                Providers = new List<DataProvider>()
            };

            // Setup data providers with custom licenses for consumer
            if(!application.IsProvider)
            {
                var customProviderLicenses = providerLicenses.Where(i => i.CustomLicenseID != null);
                result.Providers = GetCustomProviders(customProviderLicenses);
            }
            
            // Get published clause templates
            var publishedClauseTemplates = _clauseTemplateService.Where(i => i.Status == (int)TemplateStatus.Active).ToList();

            // Select clause template identifiers
            var publishedTemplateIds = publishedClauseTemplates.Select(i => i.LicenseClauseID).Distinct();

            // Get license clauses for each template
            var licenseClauses = publishedTemplateIds.Select(i => _clauseService.FirstOrDefault(p=>p.ID==i));

            // Get active section identifiers
            var sectionIdentifiers = licenseClauses.Select(i => i.LicenseSectionID).Distinct();

            // Get sections
            var sections = sectionIdentifiers.Select(i => _sectionService.FirstOrDefault(p=>p.ID == i)).ToList();

            // Setup result model
            var sectionModels = new List<SectionsWithClauses>();

            // Setup sections
            foreach (var section in sections)
            {
                section.Title = section.Title.Replace("_additional", " (Optional)");
                sectionModels.Add(new SectionsWithClauses
                {
                    Section = section.ToModel(),
                    ApplicationId = applicationId,
                    IsForProvider = application.IsProvider,
                    Clauses = new List<ClauseModel>()
                });
            }

            // Return error if model does not have any items
            if (!sectionModels.Any())
            {
                throw new BaseException("There are no sections in license");
            }

            // Setup clauses for each section
            foreach (var item in sectionModels)
            {
                item.Clauses = GetClauseModelsForSection(publishedClauseTemplates.AsReadOnly(), item.Section.ID);
            }

            // Setup sections
            result.Sections = sectionModels;

            // Return result
            return result;
        }
        
        public void CreateConsumerTemplatedLicense(int appId, int schemaId, int providerLicenseId, LoggedInUserDetails user)
        {
            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(orgIsInactiveError);
            }

            // Get published template
            var licenseTemplate = _licenseTemplates.FirstOrDefault(i => i.Status == (int)TemplateStatus.Active);

            var providerLicense = _service.GetById(providerLicenseId);
            if(providerLicense == null)
            {
                throw new BaseException($"Provider license not found {providerLicenseId}");
            }

            // Create consumer license
            var consumerLicense = new OrganizationLicense
            {
                ApplicationID = appId,
                DataSchemaID = schemaId,
                LicenseTemplateID = providerLicense.LicenseTemplateID,
                CustomLicenseID = providerLicense.CustomLicenseID,
                CreatedAt = GetDate,
                Status = (int)PublishStatus.Draft,
                CreatedBy = user.ID.Value
            };

            _service.Add(consumerLicense);
            
            // Setup consumer match
            var consumerMatch = new LicenseMatch
            {
                ConsumerLicenseID = consumerLicense.ID,
                ProviderLicenseID = providerLicenseId,
                CreatedBy = user.ID.Value,
                CreatedAt = GetDate
            };

            // Save consumer match
            _licenseMatches.Add(consumerMatch);

            // Send for legal approval
            RequestLicenseVerification(consumerLicense.ID, appId, schemaId, user);
        }

        public List<OrganizationLicense> CreateConsumerCustomLicense(int appId, int schemaId, List<DataProvider> providers, LoggedInUserDetails user)
        {
            // Check access to application
            _security.CheckAccessToApplication(user, appId);

            // Get selected provider licenses
            var selectedProviders = providers.Where(i => i.IsSelected == true);

            // Define result
            var result = new List<OrganizationLicense>();

            // Create organisation license with the same custom license id
            foreach(var providerLicense in selectedProviders)
            {
                var consumerLicense = new OrganizationLicense
                {
                    ApplicationID = appId,
                    DataSchemaID = schemaId,
                    CustomLicenseID = providerLicense.CustomLicenseId,
                    CreatedBy = user.ID.Value,
                    CreatedAt = GetDate,
                    Status = (int)PublishStatus.Draft
                };

                // Save new consumer license to database
                _service.Add(consumerLicense);

                // Create license match for consumer & provider license
                var licenseMatch = new LicenseMatch
                {
                    ConsumerLicenseID = consumerLicense.ID,
                    ProviderLicenseID = providerLicense.LicenseId,
                    CreatedAt = GetDate,
                    CreatedBy = user.ID.Value
                };

                // Save new license match
                _licenseMatches.Add(licenseMatch);

                // Add created license to result
                result.Add(consumerLicense);
            }

            // Return created data
            return result;
        }
                
        public void RequestLicenseVerification(int id, int appId, int schemaId, LoggedInUserDetails user)
        {
            // Check access
           var application = _security.CheckAccessToApplication(user, appId);

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(orgIsInactiveError);
            }

            // Get license
            var license = _service.FirstOrDefault(i=>i.ID==id);

            // Only draft license can be sent for legal approval
            if (license.Status != (int)PublishStatus.Draft)
            {
                throw new BaseException("Only in draft status license can be sent to Legal Officer.");
            }

            var pendingApprovalLicenses = new List<OrganizationLicense>();
            // Check whether it's a provider
            if(application.IsProvider)
            {
                // Provider can have only one pending approval license
                pendingApprovalLicenses = _service.Where(i => i.ApplicationID == appId)
                    .Where(i => i.DataSchemaID == schemaId && i.Status == (int)PublishStatus.PendingApproval).ToList();
            }
            else
            {
                // Update status for another templated pending approval licenses for consumer
                pendingApprovalLicenses = _service.Where(i => i.ApplicationID == appId)
                    .Where(i => i.DataSchemaID == schemaId && i.Status == (int)PublishStatus.PendingApproval && i.LicenseTemplateID != 0).ToList();
            }

            // Change status to draft for other pending approval licenses
            foreach (var pendingApprovalLicense in pendingApprovalLicenses)
            {
                pendingApprovalLicense.Status = (int)PublishStatus.Draft;
                _service.Update(pendingApprovalLicense);
            }

            // Update license status
            license.Status = (int)PublishStatus.PendingApproval;
            _service.Update(license);

            // Get schema
            var schema = _dataSchemaService.FirstOrDefault(i=>i.ID == license.DataSchemaID);

            // Get schema file
            var schemaFile = _schemaFileService.FirstOrDefault(i => i.DataSchemaID == schema.ID);

            // Setup url to download schema
            var urlToSchema = _urls.ToDownloadSchema(schemaFile.ID);

            // Get legal officers for organisation
            var legalOfficers = _users.Where(i => i.OrganizationID == user.Organization.ID)
                .Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true)
                .ToList();

            // Send notification to legal officers
            SendNotificationToLegalOfficers(license, legalOfficers, schema.Name, urlToSchema, user);
        }

        public void Publish(int id, int appId, int schemaId, LoggedInUserDetails user)
        {
            // Check access
            _security.CheckBasicAccess(user);

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(orgIsInactiveError);
            }

            // Get license
            var license = _service.FirstOrDefault(i => i.ID == id);

            // Get published license
            var licenses = _service.Where(i => i.ApplicationID == appId)
                .Where(i => i.Status == (int)PublishStatus.Published && i.DataSchemaID == schemaId);

            // Check whether published license alredy present
            if (licenses.Any())
            {
                throw new BaseException("You can't publish if published license already exists.");
            }

            // Check whether license is ready to be published
            if (license.Status != (int)PublishStatus.ReadyToPublish)
            {
                throw new BaseException("Only ready to publish licenses can be published.");
            }

            // Update details
            license.Status = (int)PublishStatus.Published;
            license.PublishedAt = GetDate;
            license.PublishedBy = user.ID.Value;

            // Save changes
            _service.Update(license);

            // Setup url to download license
            //var linkToDownloadLicense = _urls.ToDownloadLicense(appId, schemaId, id);
            var linkToDownloadLicense = _urls.ToDownloadLicenseView(appId, schemaId, id);

            // Send notification about new data provider
            if (license.ProviderEndpointID != 0)
            {
                _notificationService.User.NewProviderLicenseInBackground(license.ID, appId, linkToDownloadLicense);
            }
        }

        public void Retract(int licenseId, LoggedInUserDetails user)
        {
            // Check access
            _security.CheckBasicAccess(user);

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(orgIsInactiveError);
            }

            // Get license
            var license = _service.FirstOrDefault(i => i.ID == licenseId);

            // Setup properties
            license.Status = (int)PublishStatus.Retracted;
            license.UpdatedAt = GetDate;
            license.UpdatedBy = user.ID.Value;

            // Save changes
            _service.Update(license);
        }
        
        public void CreateProviderTemplatedLicense(int appId, int schemaId, BuildLicenseModel model, LoggedInUserDetails user)
        {
            // Check access
            _security.CheckBasicAccess(user);

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(orgIsInactiveError);
            }

            // Get published license template
            var publishedTemplate = _licenseTemplates.FirstOrDefault(i=>i.Status == (int)TemplateStatus.Active);
            if (publishedTemplate == null)
            {
                throw new BaseException("Published template not found");
            }

            // Create provider license
            SaveProviderLicense(publishedTemplate.ID, schemaId, appId, model.Sections, user);
        }

        public void Draft(int licenseId, LoggedInUserDetails user)
        {
            // Check access
            _security.CheckBasicAccess(user);

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(orgIsInactiveError);
            }

            // Get license
            var license = _service.FirstOrDefault(i => i.ID == licenseId);

            // Handle not found case
            if (license == null)
            {
                throw new BaseException("License not found");
            }

            //  Make sure license is not already drafted or published
            if (license.Status == (int)PublishStatus.Draft || license.Status == (int)PublishStatus.Published)
            {
                throw new BaseException("Only pending approval or ready to publish licenses can be moved to draft.");
            }

            // Setup details
            license.UpdatedAt = GetDate;
            license.UpdatedBy = user.ID;
            license.Status = (int)PublishStatus.Draft;

            // Save changes
            _service.Update(license);
        }

        private List<DataProvider> GetCustomProviders(IEnumerable<OrganizationLicense> providerLicenses)
        {
            // Define result
            var result = new List<DataProvider>();

            foreach (var providerLicense in providerLicenses)
            {
                // Get provider application
                var providerApp = _applicationService.FirstOrDefault(i=>i.ID == providerLicense.ApplicationID);

                // Get provder organisation
                var providerOrg = _organisations.FirstOrDefault(i=>i.ID==providerApp.OrganizationID);

                // Setup provider model
                var model = new DataProvider
                {
                    CustomLicenseId = providerLicense.CustomLicenseID.Value,
                    LicenseId = providerLicense.ID,
                    Name = providerOrg.Name
                };

                // Add model to result
                result.Add(model);
            }

            // Return result
            return result;
        }

        private void SaveProviderLicense(int licenseTemplateId, int schemaId, int applicationId, List<SectionsWithClauses> model, LoggedInUserDetails user)
        {
            // Get endpoint
            var endpoint = _endpoints.Where(i => i.ApplicationId == applicationId)
                .FirstOrDefault(i => i.DataSchemaID == schemaId);

            // Setup organization license
            var license = new OrganizationLicense
            {
                LicenseTemplateID = licenseTemplateId,
                ApplicationID = applicationId,
                DataSchemaID = schemaId,
                ProviderEndpointID = endpoint.ID,
                CreatedBy = user.ID.Value,
                CreatedAt = GetDate,
                Status = (int)TemplateStatus.Draft
            };

            // Save organization license
            _service.Add(license);

            // Save license clauses
            foreach (var section in model)
            {
                // Skip not selected section
                if (!_licenseClauses.IsClauseSelected(section)) continue;
                var selectedClause = section.Clauses.First(p => p.ClauseTemplateId == section.SelectedClause);
                var clause = SetupLicenseClause(selectedClause, license, user);
                _licenseClauseService.Add(clause);
            }
        }

        private List<ProviderLicenseModel> SetupLicenses(List<OrganizationLicense> licenses)
        {
            // Setup license models
            var result = new List<ProviderLicenseModel>();
            foreach (var license in licenses)
            {
                // Setup license model
                var licenseModel = new ProviderLicenseModel
                {
                    ID = license.ID,
                    Status = (PublishStatus)license.Status,
                    CreatedAt = license.CreatedAt.ToLocalTime()
                };

                // Setup custom license
                if (license.CustomLicenseID != null)
                {
                    // Get provider license
                    var providerLicense = _service.Where(i => i.CustomLicenseID == license.CustomLicenseID).FirstOrDefault(i => i.ProviderEndpointID != 0);

                    // Get applications
                    var application = _applicationService.FirstOrDefault(i=>i.ID ==providerLicense.ApplicationID);

                    // Get organisation
                    var organisation = _organisations.FirstOrDefault(i=>i.ID == application.OrganizationID);

                    // Setup data
                    licenseModel.CustomLicenseId = license.CustomLicenseID.Value;
                    licenseModel.OrgName = organisation.Name;
                }
                else
                {
                    // Setup templated license
                    var template = _licenseTemplates.FirstOrDefault(i=>i.ID==license.LicenseTemplateID.Value);
                    licenseModel.TemplateName = $"from {template.Name}";
                }

                // Add to result
                result.Add(licenseModel);
            }

            // Return result
            return result;
        }

        private IList<ClauseModel> GetClauseModelsForSection(IReadOnlyList<LicenseClauseTemplate> clauseTemplates, int sectionId)
        {
            var result = new List<ClauseModel>();
            // Get clause for section
            var sectionClauses = _clauseService.Where(p => p.LicenseSectionID == sectionId);
            foreach (var sectionClause in sectionClauses)
            {
                // Get templates for clause
                var templates = clauseTemplates.Where(p => p.LicenseClauseID == sectionClause.ID);
                foreach (var template in templates)
                {
                    // Setup model for template
                    var item = template.ToModel(sectionId);
                    // Add to result
                    result.Add(item);
                }
            }

            // Return result
            return result;
        }
                
        private OrganizationLicenseClause SetupLicenseClause(ClauseModel clause, OrganizationLicense license, LoggedInUserDetails user)
        {
            var clauseValue = _licenseClauses.GetClauseData(clause);
            var consumerClause = new OrganizationLicenseClause
            {
                LicenseClauseID = clause.ClauseId,
                OrganizationLicenseID = license.ID,
                CreatedBy = user.ID.Value,
                CreatedAt = GetDate,
                ClauseData = clauseValue
            };
            return consumerClause;
        }
        
        private void SendNotificationToLegalOfficers(OrganizationLicense license, List<User> legalOfficers, string schemaName, string urlToSchema, LoggedInUserDetails user)
        {
            // Send notification to legal officers
            foreach (var legalOfficer in legalOfficers)
            {
                // Generate secure token
                var tokenInfo = _tokens.GenerateLicenseVerificationToken(legalOfficer.ID, license.ID);

                // Create approval request
                var request = new LicenseApprovalRequest
                {
                    accessToken = tokenInfo.TokenInfoEncoded,
                    Token = tokenInfo.Token,
                    ExpiresAt = tokenInfo.TokenExpire.Value,
                    OrganizationLicenseID = license.ID,
                    SentAt = GetDate,
                    SentBy = user.ID.Value,
                    SentTo = legalOfficer.ID
                };

                // Save request
                _verificationRequests.Add(request);

                // Setup url to confirmation screen
                var urlToConfirmScreen = _urls.ToLicenseVerification(license.ApplicationID, license.DataSchemaID, tokenInfo.TokenInfoEncoded);

                // Send notificaion to legal officer with templated license
                if (license.LicenseTemplateID != null)
                {
                    // Send notification
                    _notificationService.LegalOfficer.LicenseIsPendingApprovalInBackground(legalOfficer.ID, $"{urlToConfirmScreen}#Clauses",
                        urlToSchema, _config.DataLinkerHost, schemaName, license.ID);
                }

                // Send notificaion to legal officer with custom license
                if (license.CustomLicenseID != null)
                {
                    // Send notification
                    _notificationService.LegalOfficer.LicenseVerificationRequiredInBackground(legalOfficer.ID, urlToConfirmScreen, schemaName,
                        license.ID);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DataLinker.Services.Mappings;
using DataLinker.Services.Security;
using DataLinker.Services.Exceptions;
using DataLinker.Models.Enums;
using DataLinker.Services.Emails;
using DataLinker.Services.Helpers;
using Rezare.CommandBuilder.Services;
using DataLinker.Database.Models;
using DataLinker.Models;
using System.Configuration;
using DataLinker.Services.OrganizationLicenses;
using DataLinker.Models.ConsumerProviderRegistration;

namespace DataLinker.Services.Applications
{
    internal class ApplicationsService : IApplicationsService
    {
        private IService<Application, int> _applications;
        private IService<Organization, int> _organisations;
        private IService<ApplicationAuthentication, int> _authentications;
        private IService<ApplicationToken, int> _tokens;
        private IService<DataSchema, int> _schemas;
        private IService<SchemaFile, int> _schemaFiles;
        private IService<OrganizationLicense, int> _licenses;
        private IService<ProviderEndpoint, int> _endpoints;
        private IService<LicenseTemplate, int> _licenseTemplates;
        private ISecurityService _security;
        private INotificationService _notifications;
        private readonly ILicenseMatchesService _licenseMatches;
        private IService<ConsumerProviderRegistration, int> _consumerProviderRegistrations;

        private DateTime GetDate => DateTime.UtcNow;

        public ApplicationsService(IService<Application, int> apps,
            ISecurityService security,
            IService<ApplicationAuthentication, int> auth,
            IService<ApplicationToken, int> tokens,
            IService<DataSchema, int> schemas,
            IService<SchemaFile, int> schemaFiles,
            IService<OrganizationLicense, int> licenses,
            IService<ProviderEndpoint, int> endpoints,
            IService<LicenseTemplate, int> licenseTemplates,
            INotificationService notifications,
            ILicenseMatchesService matches,
            IService<Organization, int> orgs,
            IService<ConsumerProviderRegistration, int> consumerProviderRegistrations)
        {
            _applications = apps;
            _organisations = orgs;
            _authentications = auth;
            _tokens = tokens;
            _schemas = schemas;
            _schemaFiles = schemaFiles;
            _licenses = licenses;
            _endpoints = endpoints;
            _security = security;
            _notifications = notifications;
            _licenseTemplates = licenseTemplates;
            _licenseMatches = matches;
            _consumerProviderRegistrations = consumerProviderRegistrations;
        }

        public void UpdateStatus(int id, bool value, LoggedInUserDetails user)
        {
            // Check whether user has access
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Get application details
            var service = _applications.FirstOrDefault(i=>i.ID == id);

            // Setup status details
            service.IsActive = value;
            service.UpdatedAt = GetDate;
            service.UpdatedBy = user.ID;

            // Save changes
            _applications.Update(service);
        }

        public List<ApplicationDetails> GetApplications(LoggedInUserDetails user)
        {
            // Define result
            var result = new List<ApplicationDetails>();

            // Check whether user is admin
            if (user.IsSysAdmin)
            {
                // Get applications for admin
                var apps = _applications.All();
                foreach (var app in apps)
                {
                    var org = _organisations.FirstOrDefault(i=>i.ID == app.OrganizationID);
                    var model = app.ToModel();
                    model.OrganizationName = org.Name;
                    result.Add(model);
                }
            }
            else
            {
                // Get applications for user
                var apps = _applications.Where(i => i.OrganizationID == user.Organization.ID);
                foreach (var app in apps)
                {
                    var model = app.ToModel();
                    result.Add(model);
                }
            }

            // Return result
            return result;
        }

        public ApplicationDetails GetApplicationDetailsModel(int id, LoggedInUserDetails user)
        {
            // Check whether user has access to application
            _security.CheckAccessToApplication(user, id);

            // Get applications
            var app = _applications.FirstOrDefault(i=>i.ID == id);

            // Check whether application is not found
            if (app == null)
            {
                throw new BaseException($"Application {id} not found.");
            }

            // Setup result model
            var result = app.ToModel();

            // Setup application details for provider
            if (app.IsProvider)
            {
                // Define list of provider endpoints
                result.Endpoints = GetEndpointModels(id);

                // Setup app authentication
                result.AuthDetails = GetAppAuthModel(id);
                result.RegistrationDetails = GetConsumerProviderRegistration(id);
            }
            else
            {
                // Setup schemas for consumer
                result.Schemas = GetSchemaModelsForConsumer(id);
            }

            // Setup host models
            result.Hosts = GetHostModels(id);
            
            // Setup flag whether published license template present
            result.IsLicenseTemplatePresent = _licenseTemplates.Where(i => i.Status == (int)TemplateStatus.Active).Any();

            // Setup flag whether any published schemas found
            result.AreSchemasPresent = _schemas.Where(i => i.Status == (int)TemplateStatus.Active).Any();

            // Return result;
            return result;
        }

        public ApplicationDetails GetDetailsModelForEdit(int id, LoggedInUserDetails user)
        {
            // Check whether user has access to application
            _security.CheckAccessToApplication(user, id);

            // Get applications
            var app = _applications.FirstOrDefault(i=>i.ID == id);

            // Check whether application is not found
            if (app == null)
            {
                throw new BaseException($"Application {id} not found.");
            }

            // Setup result model
            var result = app.ToModel();

            // Return result;
            return result;
        }

        private List<SchemaModel> GetSchemaModelsForConsumer(int id)
        {
            // Get published schemas
            var availableSchemas = _schemas.Where(i => i.Status == (int)TemplateStatus.Active).ToList();

            // Define schemas
            var result = new List<SchemaModel>();

            // Get licenses for application
            var consumerLicenses = _licenses.Where(i => i.ApplicationID == id).ToList();

            foreach (var schema in availableSchemas)
            {
                // Define schema model
                SchemaModel schemaModel;

                // Setup schema model
                schemaModel = schema.ToModel();

                // Setup schema status
                schemaModel.LicenseStatus = (PublishStatus)GetLicenseStatus(id, schema.ID);

                // Get schema file
                var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schema.ID);

                // Setup schema file Id for download url
                schemaModel.SchemaFileId = schemaFile.ID;

                // Add to schema model to result
                result.Add(schemaModel);
            }

            // Return result
            return result;
        }

        private List<ApplicationTokenDetails> GetHostModels(int id)
        {
            var result = new List<ApplicationTokenDetails>();

            // Get application tokens
            var tokens = _tokens.Where(i => i.ApplicationID == id).Where(i => i.ExpiredAt == null);

            // Setup host models
            foreach (var token in tokens)
            {
                var model = token.ToModel();
                result.Add(model);
            }

            // Return result
            return result;
        }

        private List<ProviderEndpointModel> GetEndpointModels(int applicationId)
        {
            // Define result
            var result = new List<ProviderEndpointModel>();

            // Get endpoints for provider
            var endpoints = _endpoints.Where(p => p.ApplicationId == applicationId).ToList();

            // Setup provider endpoints
            foreach (var providerEndpoint in endpoints)
            {
                // Get schema
                var schema = _schemas.FirstOrDefault(i=>i.ID == providerEndpoint.DataSchemaID);

                // Setup endpoint model
                var endpointModel = new ProviderEndpointModel()
                {
                    Description =providerEndpoint.Description,
                    ID = providerEndpoint.ID,
                    Name = providerEndpoint.Name
                };

                endpointModel.Schema = schema.ToModel();

                // Setup endpoint status
                endpointModel.LicenseStatus = (PublishStatus)GetLicenseStatus(applicationId, schema.ID);

                // Get schema file
                var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == providerEndpoint.DataSchemaID);

                // Setup schema file Id for download URL
                endpointModel.Schema.SchemaFileId = schemaFile.ID;
                result.Add(endpointModel);
            }

            // Return result
            return result;
        }

        private int GetLicenseStatus(int appId, int schemaId)
        {
            // Setup default license status  for application
            var result = 0;

            // Get licenses
            var licenses = _licenses
                .Where(license => license.ApplicationID == appId)
                .Where(i => i.DataSchemaID == schemaId).ToList();

            // Check whether any licenses found
            if (licenses.Any())
            {
                // Setup biggest license status for application
                result = licenses.Max(i => i.Status);
            }

            // Return result
            return result;
        }

        public Application Create(string url, NewApplicationDetails model, LoggedInUserDetails user)
        {
            // Check whether user has access
            if (user.IsSysAdmin)
            {
                throw new BaseException("Admin can not create an application.");
            }

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Check whether application name already used within the organisation
            if (IsApplicationExistsForThisOrganization(model.Name, string.Empty, user))
            {
                throw new BaseException("Application name already in use.");
            }

            // Check whether hosts provided
            if (string.IsNullOrEmpty(model.OriginHosts))
            {
                throw new BaseException("You should define at least one host.");
            }

            // TODO: check whether all required data provided[Failed when auth tab was now shown in create provider app]

            // Setup application model
            var application = new Application
            {
                Name = model.Name,
                Description = model.Description,
                PublicID = Guid.NewGuid(),
                IsProvider = model.IsProvider,
                IsIntroducedAsIndustryGood = model.IsIntroducedAsIndustryGood,
                OrganizationID = user.Organization.ID,
                CreatedAt = GetDate,
                IsActive = !model.IsIntroducedAsIndustryGood,
                CreatedBy = user.ID.Value
            };

            // Add application
            _applications.Add(application);

            if (application.IsProvider)
            {
                // Setup application authentication
                var appAuth = new ApplicationAuthentication
                {
                    ApplicationID = application.ID,
                    WellKnownUrl = string.IsNullOrEmpty(model.WellKnownUrl) ? string.Empty : model.WellKnownUrl,
                    Issuer = string.IsNullOrEmpty(model.Issuer) ? string.Empty : model.Issuer,
                    JwksUri = string.IsNullOrEmpty(model.JwksUri) ? string.Empty : model.JwksUri,
                    AuthorizationEndpoint = model.AuthorizationEndpoint,
                    TokenEndpoint = model.TokenEndpoint,
                    RegistrationEndpoint = model.RegistrationEndpoint,
                    UserInfoEndpoint = string.Empty,
                    EndSessionEndpoint = string.Empty,
                    CheckSessionIFrame = string.Empty,
                    RevocationEndpoint = string.Empty,
                    CreatedAt = GetDate,
                    CreatedBy = user.ID.Value
                };

                // Add application authentication
                _authentications.Add(appAuth);
            }

            foreach (var host in model.OriginHosts.Split(','))
            {
                var appToken = new ApplicationToken()
                {
                    ApplicationID = application.ID,
                    OriginHost = host,
                    Token = TokensHelper.GenerateToken(),
                    CreatedAt = GetDate,
                    CreatedBy = user.ID.Value
                };

                // Add token
                _tokens.Add(appToken);
            }

            // Send verification request to admin for industry good application
            if (application.IsIntroducedAsIndustryGood)
            {
                _notifications.Admin.NewIndustryGoodApplicationInBackground(url, application.OrganizationID);
            }

            return application;
        }

        public bool IsApplicationExistsForThisOrganization(string name, string InitialName, LoggedInUserDetails user)
        {
            if (string.Equals(name, InitialName, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            return _applications.Where(i=>i.OrganizationID == user.Organization.ID).Any(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public Application EditApplication(int id, string url, ApplicationDetails appDetails, LoggedInUserDetails user)
        {
            //TODO Check whether user has access to application

            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException("Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Check whether user has access to application
            var application = _security.CheckAccessToApplication(user, id);

            var originalIsIntroducedAsIndustryGood = application.IsIntroducedAsIndustryGood;

            // Check for the duplication of the name
            if (IsApplicationExistsForThisOrganization(appDetails.Name, application.Name,user))
            {
                throw new BaseException("Application name already in use.");
            }

            // Setup application details
            application.Name = appDetails.Name;
            application.Description = appDetails.Description;
            application.IsIntroducedAsIndustryGood = appDetails.IsIntroducedAsIndustryGood;
            application.UpdatedAt = GetDate;
            application.UpdatedBy = user.ID.Value;

            // Setup industry good settings
            if (!appDetails.IsIntroducedAsIndustryGood && application.IsVerifiedAsIndustryGood)
            {
                application.IsVerifiedAsIndustryGood = false;
            }

            // Check whether verification requred
            var isVerificationRequired = !originalIsIntroducedAsIndustryGood && appDetails.IsIntroducedAsIndustryGood;

            // Setup application if it's marked as industry good
            if (isVerificationRequired)
            {
                // Deactivate service for verification process
                application.IsActive = false;
            }

            // Save changes
            _applications.Update(application);

            // Check whether notification for verification is required
            if (isVerificationRequired)
            {
                // Send notification to admin about new verification request
                _notifications.Admin.NewIndustryGoodApplicationInBackground(url, application.OrganizationID);
            }

            return application;
        }

        public List<ApplicationTokenDetails> GetHosts(int id, LoggedInUserDetails user)
        {
            // Check access to application
            var application = _security.CheckAccessToApplication(user, id);
            
            // Check whether organisation deactivated
            if (!user.Organization.IsActive)
            {
                throw new BaseException("Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Get application tokens
            var tokens = _tokens.Where(i => i.ApplicationID == id).Where(i => i.ExpiredAt == null);

            // Define result
            var result = new List<ApplicationTokenDetails>();

            // Setup result
            foreach (var token in tokens)
            {
                var model = token.ToModel();
                result.Add(model);
            }

            return result;
        }

        public void AddHost(int id, string host, LoggedInUserDetails user)
        {
            // Check whether organisation is not active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Check whether host is a valid uri
            var isValidUrl = Uri.TryCreate(host, UriKind.Absolute, out var result);

            // Check whether url scheme specified
            var urlWithScheme = isValidUrl && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
            if (!urlWithScheme)
            {
                throw new BaseException($"Invalid host '{result}'");
            }

            // Get application
            var application = _security.CheckAccessToApplication(user, id);

            // Setup new application token
            var appToken = new ApplicationToken
            {
                ApplicationID = application.ID,
                OriginHost = host,
                Token = TokensHelper.GenerateToken(),
                CreatedAt = GetDate,
                CreatedBy = user.ID.Value
            };

            // Add new token
            _tokens.Add(appToken);
        }

        public ApplicationToken CreateNewToken(int id, int tokenId, LoggedInUserDetails user)
        {
            // Check whehter organisation is not active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Check whether application belongs to a user
            _security.CheckAccessToApplication(user, id);

            // Get application token
            var appToken = _tokens.FirstOrDefault(i=>i.ID == tokenId);

            // Check whether app token not found
            if (appToken == null)
            {
                throw new BaseException("Unable to find service host.");
            }

            // Generate new token
            var generatedToken = TokensHelper.GenerateToken();
            var result = new ApplicationToken()
            {
                ApplicationID = appToken.ApplicationID,
                OriginHost = appToken.OriginHost,
                Token = generatedToken,
                CreatedAt = GetDate,
                CreatedBy = user.ID.Value
            };

            // Save token
            _tokens.Add(result);

            // Setup expiration details for old token
            appToken.ExpiredAt = GetDate;
            appToken.ExpiredBy = user.ID.Value;

            // Save changes
            _tokens.Update(appToken);

            // Return result
            return result;
        }

        public void ExpireAppToken(int id, int tokenId, LoggedInUserDetails user)
        {
            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Check whether app relates to user
            _security.CheckAccessToApplication(user, id);

            // Get app token
            var appToken = _tokens.FirstOrDefault(i=>i.ID == tokenId);

            // Check whether token does not exists
            if (appToken == null)
            {
                throw new BaseException("Not found");
            }

            // Check whether app token does not belong to application
            if (appToken.ApplicationID != id)
            {
                throw new BaseException("Access denied.");
            }

            // Setup expiration details
            appToken.ExpiredAt = GetDate;
            appToken.ExpiredBy = user.ID.Value;

            // Save changes
            _tokens.Update(appToken);
        }

        public ApplicationAuthenticationDetails GetApplicationAuthModel(int id, LoggedInUserDetails user)
        {
            // Check whether organisation is inactive
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Get application
            var app = _security.CheckAccessToApplication(user, id);

            // Setup result model
            var authDetails = GetAppAuthModel(id);
            return authDetails;
        }

        private ApplicationAuthenticationDetails GetAppAuthModel(int appId)
        {
            // Get application authentication
            var appAuth = _authentications.FirstOrDefault(i => i.ApplicationID == appId);

            // Check whether authentication not found
            if (appAuth == null)
            {
                throw new BaseException($"Authentication details for application {appId} not found");
            }

            // Setup result
            var result = appAuth.ToModel();

            // Return result
            return result;
        }

        public ApplicationAuthenticationDetails SetupEditAppAuthModel(int id, LoggedInUserDetails user)
        {
            // Check whether organisation is not active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Get applciation
            var app = _security.CheckAccessToApplication(user, id);

            // Setup result model
            var result = new ApplicationAuthenticationDetails();

            // Get auth details
            var appAuth = _authentications.FirstOrDefault(i => i.ApplicationID == app.ID);

            // Check whether
            if (appAuth == null)
            {
                throw new BaseException("Auth not found");
            }

            // Setup result
            result = appAuth.ToModel();
            return result;
        }

        public void EditAuthentication(int id, ApplicationAuthenticationDetails model, LoggedInUserDetails user)
        {
            // Check whether organisation is active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }
            // Check whether user has access to application
            var application = _security.CheckAccessToApplication(user, id);

            // Get auth details
            var appAuth = _authentications.FirstOrDefault(i => i.ApplicationID == application.ID);

            // Setup update details
            appAuth.UpdatedAt = GetDate;
            appAuth.UpdatedBy = user.ID;
            appAuth.WellKnownUrl = string.IsNullOrEmpty(model.WellKnownUrl) ? string.Empty : model.WellKnownUrl;
            appAuth.Issuer = string.IsNullOrEmpty(model.Issuer) ? string.Empty : model.Issuer;
            appAuth.JwksUri = string.IsNullOrEmpty(model.JwksUri) ? string.Empty : model.JwksUri;
            appAuth.AuthorizationEndpoint = model.AuthorizationEndpoint;
            appAuth.TokenEndpoint = model.TokenEndpoint;
            appAuth.RegistrationEndpoint = model.RegistrationEndpoint;
            appAuth.UserInfoEndpoint = string.Empty;
            appAuth.EndSessionEndpoint = string.Empty;
            appAuth.CheckSessionIFrame = string.Empty;
            appAuth.RevocationEndpoint = string.Empty;

            // Save changes
            _authentications.Update(appAuth);
        }

        public ProviderEndpointModel SetupAddProviderEndpointModel(int id, LoggedInUserDetails user)
        {
            // Check whether organisation is not active
            if (!user.Organization.IsActive)
            {
                throw new BaseException("Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Check whether application belongs to user
            _security.CheckAccessToApplication(user, id);

            // Get published schemas
            var schemas = _schemas.Where(i => i.Status == (int)TemplateStatus.Active).ToList();

            // Check whether no published schemas
            if (!schemas.Any())
            {
                throw new BaseException("Sorry, we don't have published schemas.");
            }

            // Get schema ids used by this application
            var schemaIds = _endpoints.Where(i => i.ApplicationId == id).Select(i => i.DataSchemaID);

            // Get schemas unused by this application 
            var availableSchemas = schemas.Where(i => schemaIds.All(p => p != i.ID));

            // Setup model
            var result = new ProviderEndpointModel
            {
                Schemas = new Dictionary<string, string>()
            };

            // Setup schemas
            foreach (var schema in availableSchemas)
            {
                result.Schemas.Add(schema.ID.ToString(), schema.Name);
            }

            // Check whether no schemas available for usage
            if (!result.Schemas.Any())
            {
                throw new BaseException("Your application already uses all available schemas. DataLinker allows only one endpoint per schema.");
            }

            return result;
        }

        public ProviderEndpointModel SetupProviderEndpointModel(int appId, int endpointId, LoggedInUserDetails user)
        {
            // Check whether user has access to application
            _security.CheckAccessToApplication(user, appId);

            // Check whether organisation is not active
            if (!user.Organization.IsActive)
            {
                throw new BaseException("Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }
            
            // Get endpoint
            var endpoint = _endpoints.FirstOrDefault(i=>i.ID== endpointId);

            // Check whether endpoint found
            if(endpoint == null)
            {
                throw new BaseException("Endpoint not found");
            }

            // Get schemas
            var schema = _schemas.FirstOrDefault(i=>i.ID == endpoint.DataSchemaID);
            // Setup result model
            var result = endpoint.ToModel();

            // Setup schema details
            result.SchemaPublicID = schema.PublicID;
            result.Schema = schema.ToModel();

            // Return result
            return result;
        }
        
        public ProviderEndpoint EditEndpoint(int appId, int endpointId, ProviderEndpointModel model, LoggedInUserDetails user)
        {
            // Check whether organisation is not active
            if (!user.Organization.IsActive)
            {
                throw new BaseException(
                    "Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Get endpoint
            var endpoint = _endpoints.FirstOrDefault(i=>i.ID == endpointId);
            
            // Check whether endpoint found
            if (endpoint == null)
            {
                throw new BaseException("Endpoint not found");
            }

            // Check whether user has access to application
            _security.CheckAccessToApplication(user, appId);

            // Setup update details
            endpoint.Description = model.Description;
            endpoint.Name = string.IsNullOrEmpty(model.Name) ? string.Empty : model.Name;
            endpoint.ServiceUri = model.Uri;
            endpoint.IsActive = model.IsActive;
            endpoint.UpdatedAt = GetDate;
            endpoint.UpdatedBy = user.ID;

            // Save changes
            _endpoints.Update(endpoint);
            return endpoint;
        }
        
        public void ApproveIndustryGoodRequest(int id,LoggedInUserDetails user)
        {
            Application app = ValidateIndustryGoodRequest(id, user);

            // Setup update details
            app.IsVerifiedAsIndustryGood = true;

            // Save changes
            _applications.Update(app);
        }

        private Application ValidateIndustryGoodRequest(int id, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get application
            var app = _applications.FirstOrDefault(i=>i.ID == id);

            // Check whether application not found
            if (app == null)
            {
                throw new BaseException("Not found");
            }

            // Check whether application already verified
            if (app.IsVerifiedAsIndustryGood)
            {
                throw new BaseException("Application is already verified.");
            }

            // Check whether application not marked as industry good
            if (!app.IsIntroducedAsIndustryGood)
            {
                throw new BaseException("This application is not Industry Good.");
            }

            return app;
        }

        public void DeclineIndustryGoodRequest(int id,LoggedInUserDetails user)
        {
            // Validate request
            Application app = ValidateIndustryGoodRequest(id, user);

            // Setup update details
            app.IsIntroducedAsIndustryGood = false;

            // Save changes
            _applications.Update(app);
        }

        public void AddEndpoint(int id, ProviderEndpointModel model, LoggedInUserDetails user)
        {
            // Check whether organisation is not active
            if (!user.Organization.IsActive)
            {
                throw new BaseException("Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Check whether application does not belong to user
            _security.CheckAccessToApplication(user, id);

            // Get schema identifiers used by this application
            var usedSchemas = _endpoints.Where(i => i.ApplicationId == id).Select(i => i.DataSchemaID);

            // Get all published schemas
            var schemas = _schemas.Where(i => i.Status == (int)TemplateStatus.Active).Select(i => i.ID);

            // Get unused published schemas
            var availableSchemas = schemas.Except(usedSchemas).ToList();

            // Check whether no available schemas found
            if (!availableSchemas.Any() || !availableSchemas.Contains(model.SelectedSchemaID))
            {
                // Return error if all schemas in use
                throw new BaseException("Your application already uses all available schemas. DataLinker allows only one endpoint per schema.");
            }

            // Get selected schema
            var dataSchema = _schemas.FirstOrDefault(i=>i.ID == model.SelectedSchemaID);

            // Setup endpoint details
            var endPoint = new ProviderEndpoint
            {
                DataSchemaID = model.SelectedSchemaID,
                ApplicationId = id,
                CreatedAt = GetDate,
                CreatedBy = user.ID.Value,
                Description = model.Description,
                IsActive = true,
                IsIndustryGood = dataSchema.IsIndustryGood,
                Name = string.IsNullOrEmpty(model.Name) ? string.Empty : model.Name,
                ServiceUri = model.Uri
            };

            // Save new endpoint
            _endpoints.Add(endPoint);
        }


        public List<appDetails> GetProviders(string[] schemas, LoggedInApplication LoggedInApp)
        {
            if (LoggedInApp.IsProvider)
            {
                throw new BaseException("Access denied");
            }

            var logoUrl = ConfigurationManager.AppSettings["UrlToImageInEmail"];
            var resultModel = new List<appDetails>();
            // Get all consumer licenses
            var allConsumerLicenses = _licenses.Where(i => i.ApplicationID == LoggedInApp.ID).ToList();
            foreach (var schemaKey in schemas)
            {
                // Get schema with public key
                var schema = _schemas.FirstOrDefault(i => i.PublicID == schemaKey);
                if (schema == null)
                {
                    throw new BaseException("Not found");
                }
                // Get consumer licenses for schema
                var consumerLicenses = allConsumerLicenses.Where(i => i.DataSchemaID == schema.ID);
                var modelsForSchema = GetResponseModel(consumerLicenses, schema, logoUrl);
                // Build result model
                resultModel.AddRange(modelsForSchema);
            }

            return resultModel;
        }

        private IList<appDetails> GetResponseModel(IEnumerable<OrganizationLicense> consumerLicenses, DataSchema schema, string logoUrl)
        {
            // Collect data for building result model
            var endpointsForSchema = GetProviderEndpointsForSchema(consumerLicenses, schema);
            var licenses = new List<OrganizationLicense>();
            var applications = new List<Application>();
            var organizations = new List<Organization>();
            var oauthList = new List<ApplicationAuthentication>();
            // Init data for building response
            foreach (var endpoint in endpointsForSchema)
            {
                var result = _licenses.Where(i => i.ProviderEndpointID == endpoint.ID).Where(i => i.Status == (int)PublishStatus.Published).ToList();
                if (result.Any())
                {
                    // Get published provider license for endpoint
                    licenses.AddRange(result);
                    // Get application for endpoint
                    var app = _applications.FirstOrDefault(i => i.ID == endpoint.ApplicationId);
                    if (applications.All(i => i.ID != app.ID))
                    {
                        // Get organizations for these applications
                        organizations.Add(_organisations.FirstOrDefault(i => i.ID == app.OrganizationID));
                        // Get Oauth info for these applications
                        oauthList.Add(_authentications.FirstOrDefault(i => i.ApplicationID == app.ID));
                        applications.Add(app);
                    }
                }
            }
            // Build result model based on collected data
            var resultModels = GetProviderDetailsForSchema(organizations, applications, oauthList, endpointsForSchema, licenses,
                schema, logoUrl).ToList();
            return resultModels;
        }

        private List<ProviderEndpoint> GetProviderEndpointsForSchema(IEnumerable<OrganizationLicense> consumerLicenses, DataSchema schema)
        {
            var endpoints = new List<ProviderEndpoint>();
            foreach (var consumerLicense in consumerLicenses)
            {
                if (consumerLicense.Status == (int)PublishStatus.Published)
                {
                    // Get matches for consumer license
                    var matches = _licenseMatches.GetForConsumer(consumerLicense.ID);
                    foreach (var match in matches)
                    {
                        // For each match get provider license
                        var providerLicense = _licenses.FirstOrDefault(i => i.ID == match.ProviderLicenseID);
                        // For provider license get endpoint
                        var endpoint = _endpoints.FirstOrDefault(i => i.ID == providerLicense.ProviderEndpointID);
                        // Add endpoint with schema ID for published license
                        if (endpoint.DataSchemaID == schema.ID && providerLicense.Status == (int)PublishStatus.Published && endpoint.IsActive)
                        {
                            endpoints.Add(endpoint);
                        }
                    }
                }
            }
            return endpoints;
        }

        private IEnumerable<appDetails> GetProviderDetailsForSchema(IEnumerable<Organization> organizations,
            List<Application> applicationsForSchema, IList<ApplicationAuthentication> oauthList, IList<ProviderEndpoint> endpoints,
            IList<OrganizationLicense> licenses,
            DataSchema schema,
            string logoUrl)
        {
            var model = new List<appDetails>();
            foreach (var application in applicationsForSchema)
            {
                // Get applications for organization
                var organization = organizations.FirstOrDefault(i => i.ID == application.OrganizationID);
                var applicationDetails = GetApplicationDetails(application, oauthList, endpoints, licenses, schema, organization, logoUrl);
                var isProviderHasPublishedLicenses = applicationDetails != null && applicationDetails.Endpoints.Any();
                if (isProviderHasPublishedLicenses)
                {
                    // Add application details
                    model.Add(applicationDetails);
                }
            }
            return model;
        }

        private appDetails GetApplicationDetails(Application application, IList<ApplicationAuthentication> oauthList, IList<ProviderEndpoint> endpoints, IList<OrganizationLicense> licenses,
            DataSchema schema, Organization organization, string logoUrl)
        {
            // Init Application details
            var applicationDetails = new appDetails
            {
                ProviderName = organization.Name,
                ProviderAddress = organization.Address,
                ProviderPhone = organization.Phone,
                LogoUrl = logoUrl,
                PublicID = application.PublicID.ToString()
            };

            // Get OAuth details for application
            var oauth = oauthList.First(i => i.ApplicationID == application.ID);
            // Setup OAuth details for application
            applicationDetails.OAuthInfo = oauth.ToApiModel();
            // Get endpoints for application
            var endpoint = endpoints.FirstOrDefault(i => i.ApplicationId == application.ID);
            // Setup models for endpoints of application
            applicationDetails.Endpoints = GetSchemaEndpoints(endpoint, licenses, schema, application.Name);
            return applicationDetails;
        }

        private List<schemaEndpoint> GetSchemaEndpoints(ProviderEndpoint endpoint,
            IList<OrganizationLicense> licenses, DataSchema schema, string appName)
        {
            var result = new List<schemaEndpoint>();
            // provider should has a published license for endpoint
            if (licenses.Any(i => i.ProviderEndpointID == endpoint.ID))
            {
                // Setup model for endpoint model
                var schemaEndpoint = new schemaEndpoint
                {
                    Endpoint = new endpointDetails
                    {
                        ServiceUri = endpoint.ServiceUri,
                        Description = endpoint.Description,
                        Name = appName
                    },
                    Schema = schema.ToDetails()
                };
                // Add model to result
                result.Add(schemaEndpoint);
            }
            return result;
        }


        public LoggedInApplication GetLoggedInApp(string referer, string apiKey)
        {
            var appToken = _tokens.FirstOrDefault(i => i.Token == apiKey);
            // Return error if token was not found
            if (appToken == null || appToken.IsExpired)
            {
                throw new BaseException($"Authorization token {apiKey} does not exists or expired.");
            }
            var application = _applications.FirstOrDefault(i => i.ID == appToken.ApplicationID);
            // Return error if application was not found or not active
            if (application == null || !application.IsActive)
            {
                throw new BaseException($"Application with token {apiKey} was not found or not active.");
            }
            // Get organization
            var organization = _organisations.FirstOrDefault(i => i.ID == application.OrganizationID);
            // Return error if organization is not active
            if (!organization.IsActive)
            {
                throw new BaseException("Organization is inactive.");
            }

            // Request referer must be the same as registered host for this token
            var originUri = new Uri(appToken.OriginHost);
            var providedUri = new Uri(referer);
            var providedReferer = $"{providedUri.Scheme}//{providedUri.Authority}";
            var allowedReferer = $"{originUri.Scheme}//{originUri.Authority}";

            var isValid = string.Equals(providedReferer, allowedReferer, StringComparison.CurrentCultureIgnoreCase);
            if (!isValid)
            {
                throw new BaseException($"Access denied. Provided referer '{referer}' is not associated with token '{apiKey}'");
            }

            // Setup details about 'logged in' application
            var result = new LoggedInApplication
            {
                ID = application.ID,
                Name = application.Name,
                PublicID = application.PublicID,
                IsProvider = application.IsProvider,
                IsIndustryGood = application.IsIntroducedAsIndustryGood && application.IsVerifiedAsIndustryGood,
                Organization = organization.ToLoggedInOrg(),
                TokenUsedToAuthorize = apiKey
            };

            return result;
        }

        private List<ConsumerProviderRegistrationDetail> GetConsumerProviderRegistration(int id)
        {
            // id = application id.
            var license = _licenses.FirstOrDefault(p => p.ApplicationID == id
                                                                        && p.Status == (int)PublishStatus.Published);
            var registrations = _consumerProviderRegistrations.Where(p => p.OrganizationLicenseID == license.ID
                                                                                                 && p.Status == (int) ConsumerProviderRegistrationStatus.PendingProviderApproval);
            var results = new List<ConsumerProviderRegistrationDetail> ();
            foreach (var registration in registrations)
            {
                var model = new ConsumerProviderRegistrationDetail();
                model.ID = registration.ID;
                model.ConsumerApplicationID = registration.ConsumerApplicationID;
                model.ConsumerApplicationName = _applications.FirstOrDefault(p => p.ID == registration.ConsumerApplicationID).Name;
                results.Add(model);
            }
            return results;
        }
    }
}

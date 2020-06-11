using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using Newtonsoft.Json;
using DataLinker.Services.Security;
using Rezare.CommandBuilder.Services;
using DataLinker.Database.Models;
using DataLinker.Models;
using Claim = System.Security.Claims.Claim;
using DataLinker.Services.Mappings;
using DataLinker.Services.Emails;
using System.Configuration;
using DataLinker.Services.Urls;

namespace DataLinker.Services.SoftwareStatements
{
    internal class SoftwareSoftwareStatementService : ISoftwareStatementService
    {
        private readonly IService<SoftwareStatement, int> _softwareStatements;
        private readonly IService<Application, int> _applications;
        private readonly IService<OrganizationLicense, int> _organisationLicenses;
        private readonly IService<DataSchema, int> _dataSchemas;
        private readonly IService<SchemaFile, int> _schemaFiles;
        private readonly IService<LicenseAgreement, int> _licenseAgreements;
        private readonly IService<ApplicationToken, int> _applicationTokens;
        private readonly INotificationService _notificationService;
        private readonly IService<ConsumerProviderRegistration, int> _consumerRegistrationRequests;
        private readonly IService<ConsumerRequest, int> _consumerRequests;
        private readonly ISecurityService _security;
        private readonly IService<ProviderEndpoint, int> _providerEndpointService;
        private readonly IUrlProvider _urls;
        private readonly string _host;
        private readonly string _linkToDownloadAgreement;

        public SoftwareSoftwareStatementService(IService<SoftwareStatement, int> service,
            IService<Application, int> applications,
            IService<OrganizationLicense, int> orgLicenses,
            IService<DataSchema, int> dataSchemas,
            IService<LicenseAgreement, int> licenseAgreements,
            ISecurityService security,
            INotificationService notifications,
            IService<ConsumerProviderRegistration, int> consumerRequests,
            IService<ConsumerRequest, int> requests,
            IService<ProviderEndpoint, int> endpoints,
            IService<SchemaFile, int> files,
            IUrlProvider urls,
            IService<ApplicationToken, int> appTokens
            )
        {
            _security = security;
            _softwareStatements = service;
            _applications = applications;
            _organisationLicenses = orgLicenses;
            _dataSchemas = dataSchemas;
            _licenseAgreements = licenseAgreements;
            _applicationTokens = appTokens;
            _notificationService = notifications;
            _consumerRegistrationRequests = consumerRequests;
            _consumerRequests = requests;
            _providerEndpointService = endpoints;
            _schemaFiles = files;
            _urls = urls;

            _host = ConfigurationManager.AppSettings["DataLinkerHost"];
            _linkToDownloadAgreement = ConfigurationManager.AppSettings["PathToDownloadAgreement"];
        }

        private DateTime GetDate => DateTime.UtcNow;

        public SoftwareStatement GetValidStatement(int applicationId, LoggedInUserDetails user, int orgId)
        {
            // Check whether organisation is not active
            if(!user.Organization.IsActive)
            {
                throw new BaseException("Access denied");
            }

            // Check whether applicaiton belongs to user
            _security.CheckAccessToApplication(user, applicationId);

            var result = _softwareStatements.Where(i => i.ApplicationID == applicationId).FirstOrDefault(i => i.ExpiredBy == null);
            // if not valid statements - create a new one
            if(result == null)
            {
                result = GetNewStatement(applicationId, user.ID.Value, orgId);
                _softwareStatements.Add(result);
            }

            return result;
        }

        public string Get(int applicationId, LoggedInApplication app, int orgId)
        {
            if(app.IsProvider)
            {
                throw new BaseApiException("Access denied");
            }

            // Check whether organisation is not active
            if (!app.Organization.IsActive)
            {
                throw new BaseException("Access denied");
            }

            // Check whether applicaiton belongs to user
            if(app.ID != applicationId)
            {
                throw new BaseException("Accees denied");
            }

            var result = _softwareStatements.Where(i => i.ApplicationID == applicationId).FirstOrDefault(i => i.ExpiredBy == null);

            // if not valid statements - generate a new one
            if (result == null)
            {
                throw new BaseApiException("No valid statements found");
            }

            return result.Content;
        }

        public SoftwareStatement UpdateSoftwareStatement(int applicationId, LoggedInUserDetails user, int organisationId)
        {
            // Check whether organisation is not active
            if (!user.Organization.IsActive)
            {
                throw new BaseException("Access denied");
            }

            // Check whether applicaiton belongs to user
            _security.CheckAccessToApplication(user, applicationId);

            var validStatement = GetValidStatement(applicationId, user,organisationId);
            if (validStatement != null)
            {
                validStatement.ExpiredAt = GetDate;
                validStatement.ExpiredBy = user.ID;
                _softwareStatements.Update(validStatement);
            }
            var statement = GetNewStatement(applicationId, user.ID.Value,organisationId);
            _softwareStatements.Add(statement);

            return statement;
        }
                
        public string GetSignedAndEncodedToken(LoggedInApplication loggedInApp)
        {
            // Get application
            var consumerApplication = _applications.FirstOrDefault(i=>i.ID == loggedInApp.ID);

            // Get applciation token
            var applicationToken = _applicationTokens.FirstOrDefault(i => i.Token == loggedInApp.TokenUsedToAuthorize);
            
            // Get schemas from approved
            var providerLicenseIds = _consumerRegistrationRequests.Where(i => i.ConsumerApplicationID == consumerApplication.ID)
                .Where(i => i.Status == (int)ConsumerProviderRegistrationStatus.Approved)
                .Select(i => i.OrganizationLicenseID);

            var schemasFromProviderLicenses = new List<int>();
            foreach (var providerLicenseId in providerLicenseIds)
            {
                var providerLicense = _organisationLicenses.GetById(providerLicenseId);
                schemasFromProviderLicenses.Add(providerLicense.DataSchemaID);
            }

            // Get schemas for each agreements
            var schemasFromAgreements = schemasFromProviderLicenses.Distinct().ToList();

            // Filter schemas
            var schemaIds = schemasFromAgreements.Distinct();

            // Setup software statement schemas
            var schemas = new List<SoftwareStatementSchema>();
            foreach (var schemaId in schemaIds)
            {
                var schema = _dataSchemas.FirstOrDefault(i=>i.ID == schemaId);
                var schemaDetails = schema.ToStmtSchema();
                schemas.Add(schemaDetails);
            }

            // Software statement will expire in 5 years for now.
            var time = GetDate.AddYears(5) - new DateTime(1970, 1, 1);
            var expireEpoch = (int)time.TotalSeconds;

            var signingCredentials = new SigningCredentials(GetSigningKey(),
                SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);

            // Create Software Statement.
            var header = new JwtHeader(signingCredentials);
            var payload = new JwtPayload();
            payload.AddClaim(new Claim("software_id", consumerApplication.PublicID.ToString()));
            payload.AddClaim(new Claim("client_name", consumerApplication.Name));
            payload.AddClaim(new Claim("client_uri", applicationToken.OriginHost));
            payload.AddClaim(new Claim("iss", "42e01f09-0dea-42b2-b5e1-0f1e87547329"));
            payload.AddClaim(new Claim("sub", "10e5766e-aff1-4b2e-b926-1a1b4ccf566c"));
            payload.AddClaim(new Claim("aud", "urn:oauth:scim:reg:generic"));
            payload.AddClaim(new Claim("exp", expireEpoch.ToString()));
            payload.AddClaim(new Claim("schemas", JsonConvert.SerializeObject(schemas)));
            payload.Base64UrlEncode();
            var jwt = new JwtSecurityToken(header, payload);

            // Sign Software Statement.
            var tokenHandler = new JwtSecurityTokenHandler();
            var signedAndEncodedToken = tokenHandler.WriteToken(jwt);
            return signedAndEncodedToken;
        }

        public SoftwareStatement GetNewStatement(int applicationId,int loggedInUserId, int organizationId)
        {
            var app = _applications.FirstOrDefault(i=>i.ID == applicationId);

            // NOTE: ClientUri would be OriginHost from first token. In API - from token, which used to authorize
            var firstAppToken = _applicationTokens.FirstOrDefault(i => i.ApplicationID == app.ID);
            if (firstAppToken == null)
            {
                throw new BaseException("Unable to find application token.");
            }
            var loggedInApp = new LoggedInApplication()
            {
                ID = app.ID,
                Name = app.Name,
                PublicID = app.PublicID,
                IsIndustryGood = app.IsIntroducedAsIndustryGood && app.IsVerifiedAsIndustryGood,
                Organization = new LoggedInOrganization { ID = organizationId },
                TokenUsedToAuthorize = firstAppToken.Token
            };

            var softwareStatementForHost = GetSignedAndEncodedToken(loggedInApp);
            return new SoftwareStatement
            {
                ApplicationID = applicationId,
                Content = softwareStatementForHost,
                CreatedAt = GetDate,
                CreatedBy = loggedInUserId
            };
        }

        private InMemorySymmetricSecurityKey GetSigningKey()
        {
            var plainTextSecurityKey = "a1cb0802-b821-4f6e-a7d8-a1351e888c52";
            var signingKey = new InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(plainTextSecurityKey));
            return signingKey;
        }

        public TokenValidationParameters TokenValidationParameters => new TokenValidationParameters()
        {
            ValidAudiences = new string[]
            {
                "urn:oauth:scim:reg:generic"
            },
            ValidIssuers = new string[]
            {
                "42e01f09-0dea-42b2-b5e1-0f1e87547329"
            },
            IssuerSigningKey = GetSigningKey()
        };



        public StatementValidationResult GetValidationResult(string softwareStmt, string scope, LoggedInApplication loggedInApp)
        {
            if(!loggedInApp.IsProvider)
            {
                throw new BaseException("Access denied");
            }

            // Setup response model
            var result = new StatementValidationResult
            {
                isSuccessfull = true,
                schemas = new List<SchemaValidationResult>()
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;
            // Verify signature in jwt
            tokenHandler.ValidateToken(softwareStmt, TokenValidationParameters, out validatedToken);
            // Retrieve Jwt
            var jwt = (JwtSecurityToken)tokenHandler.ReadToken(softwareStmt);
            // Retrieve consumer application id
            var consumerPublicId = jwt.Payload["software_id"].ToString();
            // Retrieve consumer application name
            var consumerAppName = jwt.Payload["client_name"].ToString();
            var consumerAppPublicId = new Guid(consumerPublicId);
            // Get consumer application
            var consumerApp = _applications.FirstOrDefault(i => i.PublicID == consumerAppPublicId);
            // Return error if application was not found
            if (consumerApp == null)
            {
                var errMsg = $"Software details are not approved. Software '{consumerAppName}' with id '{consumerPublicId}' is not found";
                throw new BaseException(errMsg);
            }
            // Get schemas that need to validate
            var requestedScope = scope.Split(' ').ToList();
            // Get schemas from provided software statement
            var schemasFromStmt = JsonConvert.DeserializeObject<List<SoftwareStatementSchema>>(jwt.Payload["schemas"].ToString());
            foreach (var schema in requestedScope)
            {
                // Get requested schema from schemas provided in software statement
                var schemaDetails = schemasFromStmt.FirstOrDefault(i => i.public_id == schema);
                // Return error if requested schema is not present in software statement
                if (schemaDetails == null)
                {
                    var errorMessage = $" Provided schema '{schema}' does not present in software statement.";
                    result.schemas.Add(GetErrorValidationResult(schema, errorMessage));
                    continue;
                }
                // Add schema validation result to response model
                result.schemas.Add(GetValidationResult(schemaDetails, consumerApp, loggedInApp));
            }

            return result;
        }

        private SchemaValidationResult GetValidationResult(SoftwareStatementSchema schemaDetails, Application consumerApp, LoggedInApplication loggedInApp)
        {
            var providerApp = _applications.FirstOrDefault(i => i.ID == loggedInApp.ID);
            var schema = _dataSchemas.FirstOrDefault(i => i.PublicID == schemaDetails.public_id);
            // Return error if schema with such id was not found
            if (schema == null)
            {
                var errorMessage = $" Provided schema '{schemaDetails.public_id}' does not exist.";
                return GetErrorValidationResult(schemaDetails.public_id, errorMessage);
            }
            // Get consumer approved requests
            var consumerRequests = _consumerRegistrationRequests.Where(i => i.ConsumerApplicationID == consumerApp.ID)
                .Where(i=>i.Status == (int)ConsumerProviderRegistrationStatus.Approved);

            // Check whether there is a consumer request linked the provider & schema
            if (GetRequestThatForProvider(consumerRequests,schema.ID,providerApp.ID) != null)
            {
                return GetSuccessValidationResult(schema.PublicID);
            }
            // Both parties must have published data agreement which must match
            return ConsumerRequestHasToBeApproved(schemaDetails.licenseId, consumerApp, providerApp, schema);
        }

        private ConsumerProviderRegistration GetRequestThatForProvider(IEnumerable<ConsumerProviderRegistration> consumerRequests, int schemaId, int providerAppId)
        {
            foreach (var item in consumerRequests)
            {
                var providerLicense = _organisationLicenses.GetById(item.OrganizationLicenseID);
                var isLicenseBelongsToProvider = providerLicense.DataSchemaID == schemaId && providerLicense.ApplicationID == providerAppId;
                if (isLicenseBelongsToProvider)
                {
                    return item;
                }
            }

            return null;
        }

        private SchemaValidationResult ConsumerRequestHasToBeApproved(int consumerLicenseId,
            Application consumerApp, Application providerApp, DataSchema schema)
        {
            // Return error if provider does not have published license
            var publishedProviderLicense =
                _organisationLicenses.Where(i => i.ApplicationID == providerApp.ID).Where(i => i.DataSchemaID == schema.ID && i.Status == (int)PublishStatus.Published).FirstOrDefault();
            if (publishedProviderLicense == null)
            {
                var errorMessage =
                    $" '{providerApp.Name}' do not have published licenses for the schema '{schema.PublicID}'.";
                return GetErrorValidationResult(schema.PublicID, errorMessage);
            }
            // Return error if consumer provider request has not been approved by provider
            var approvedConsumerRequest = _consumerRegistrationRequests.Where(i => i.OrganizationLicenseID == publishedProviderLicense.ID)
                .FirstOrDefault(i => i.ConsumerApplicationID == consumerApp.ID && i.Status == (int)ConsumerProviderRegistrationStatus.Approved);
            if (approvedConsumerRequest == null)
            {
                var errorMessage = $" {consumerApp.Name} has not been approved by {providerApp.Name} to access {schema.PublicID}.";
                return GetErrorValidationResult(schema.PublicID, errorMessage);
            }
            // Return success if no errors occured during validation
            return GetSuccessValidationResult(schema.PublicID);
        }

        private ConsumerRequest GetConsumerRequest(Application consumerApp, Application providerApp, DataSchema schema)
        {
            var approvedRequest = _consumerRequests.Where(i => i.ProviderID == providerApp.ID).FirstOrDefault(i => i.DataSchemaID == schema.ID && i.ConsumerID == consumerApp.ID & i.Status == (int)RequestStatus.Approved);
            if (approvedRequest != null)
            {
                return approvedRequest;
            }
            var consumerRequest = _consumerRequests.Where(i => i.ProviderID == providerApp.ID).FirstOrDefault(i => i.ConsumerID == consumerApp.ID && i.DataSchemaID == schema.ID && i.Status == (int)RequestStatus.NotProcessed);

            // Create consumer request if not exists
            if (consumerRequest == null)
            {
                consumerRequest = new ConsumerRequest
                {
                    ProviderID = providerApp.ID,
                    ConsumerID = consumerApp.ID,
                    Status = (int)RequestStatus.NotProcessed,
                    DataSchemaID = schema.ID,
                    CreatedAt = GetDate
                };

                // Save request
                _consumerRequests.Add(consumerRequest);

                var urlToConsumerRequests = $"{_host}/ConsumerRequests/Index?applicationId={providerApp.ID}";
                _notificationService.LegalOfficer.NewConsumerRequestInBackground(providerApp.ID, urlToConsumerRequests);
            }
            return consumerRequest;
        }

        private SchemaValidationResult GetErrorValidationResult(string schemaPublicId, string errorMessage)
        {
            return new SchemaValidationResult
            {
                publicId = schemaPublicId,
                isValid = false,
                error_description = errorMessage
            };
        }

        private SchemaValidationResult GetSuccessValidationResult(string schemaPublicId)
        {
            return new SchemaValidationResult
            {
                publicId = schemaPublicId,
                isValid = true
            };
        }
        
        public void CreateLicenseAgreement(LicenseDetails licenseDetails, LoggedInApplication loggedInApp)
        {
            if(!loggedInApp.IsProvider)
            {
                throw new BaseException("Access denied");
            }

            var providedSchemas = licenseDetails.accepted_schemas.Split(' ');
            foreach (var schemaPublicId in providedSchemas)
            {
                // Get schema
                var schema = _dataSchemas.FirstOrDefault(i => i.PublicID == schemaPublicId);
                if (schema == null)
                {
                    // Return error is schema was not found
                    throw new BaseApiException($"Specified schema '{schemaPublicId}' was not found");
                }
                // Setup Jwt reader
                var tokenHandler = new JwtSecurityTokenHandler();
                // Read Jwt token
                var jwt = (JwtSecurityToken)tokenHandler.ReadToken(licenseDetails.software_statement);
                SecurityToken validatedToken;
                // Verify signature in jwt
                tokenHandler.ValidateToken(licenseDetails.software_statement, TokenValidationParameters, out validatedToken);
                // Retrieve from jwt public id for consumer application
                var consumerPublicId = jwt.Payload["software_id"].ToString();
                var consumerAppPublicId = new Guid(consumerPublicId);
                // Get provider service
                var providerApp = _applications.FirstOrDefault(i => i.ID == loggedInApp.ID);
                // Get consumer application
                var consumerApp = _applications.FirstOrDefault(i => i.PublicID == consumerAppPublicId);
                if (consumerApp == null)
                {
                    throw new BaseApiException($"Specified consumer '{consumerPublicId}' not found");
                }
                // Both parties should have published data agreement for schema which must match
                var agreement = CreateAgreementIfNotExists(licenseDetails, consumerApp, schema, schemaPublicId, providerApp);
                // Skip this scope if agreement already exists
                if (agreement == null)
                {
                    continue;
                }
                // Save new license agreement
                _licenseAgreements.Add(agreement);

                // TODO: Audit log
                //AuditLog.Log(AuditStream.LegalAgreements, "License agreement created",
                //    new {LoggedInApplication.Name, LoggedInApplication.TokenUsedToAuthorize, OrgId = LoggedInApplication.Organization.ID},
                //    new {agreement.ID, schemaName = schema.PublicID});

                // Get schema file
                var schemaFile = _schemaFiles.FirstOrDefault(i => i.DataSchemaID == schema.ID);
                // Setup url to schema
                var urlToSchema = _urls.ToDownloadSchema(schemaFile.ID);
                // Setup url to download license agreement
                var linkToDownloadLicense = $"{_host}{_linkToDownloadAgreement}{agreement.ID}";
                // Notify legal officers about new license agreement
                _notificationService.LegalOfficer.LicenseAgreementCreatedInBackground(agreement.ID,
                    linkToDownloadLicense, urlToSchema, _host);
            }
        }

        private LicenseAgreement CreateAgreementIfNotExists(LicenseDetails licenseDetails, Application consumerApp,
            DataSchema schema, string schemaPublicId, Application providerApp)
        {
            // Get published provider license
            var publishedProviderLicense =
                _organisationLicenses.Where(i => i.ApplicationID == providerApp.ID).Where(i => i.DataSchemaID == schema.ID && i.Status == (int)PublishStatus.Published).FirstOrDefault();
            if (publishedProviderLicense == null)
            {
                // Return error if provider does not have published license
                throw new BaseApiException($"Published data agreement from provider for schema '{schemaPublicId}' not found");
            }
            // Get published consumer license
            var consumerRequests = _consumerRegistrationRequests.Where(i => i.ConsumerApplicationID == consumerApp.ID)
                .Where(i => i.Status == (int)ConsumerProviderRegistrationStatus.Approved);

            var consumerProviderRequest = GetRequestThatForProvider(consumerRequests, schema.ID, providerApp.ID);
            if (consumerProviderRequest == null)
            {
                // Return error if consumer does not have approved request
                var errorMessage = $" {consumerApp.Name} has not been approved by {providerApp.Name} to access {schema.PublicID}.";
            }

            // Check if this record already exists - try to get license agreement
            var existingAgreement = _licenseAgreements.FirstOrDefault(i => i.ConsumerProviderRegistrationId == consumerProviderRequest.ID);

            // If agreement with these licenses already exists - do not create
            if (existingAgreement != null)
            {
                return null;
            }
            // Setup new details for license agreement
            var agreement = new LicenseAgreement
            {
                ConsumerProviderRegistrationId = consumerProviderRequest.ID,
                CreatedAt = GetDate,
                SoftwareStatement = licenseDetails.software_statement
            };
            return agreement;
        }
    }
}
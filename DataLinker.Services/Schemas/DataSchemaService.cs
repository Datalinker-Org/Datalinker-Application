using System.Collections.Generic;
using System.Linq;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Mappings;
using System.IO;
using System;
using System.Text;
using DataLinker.Models;
using DataLinker.Services.OrganizationLicenses;
using DataLinker.Services.Emails;
using Rezare.CommandBuilder.Services;
using DataLinker.Database.Models;

namespace DataLinker.Services.Schemas
{
    internal class DataSchemaService : IDataSchemaService
    {
        private IService<DataSchema, int> _schemas;
        private IService<SchemaFile, int> _schemaFiles;
        private ILicenseMatchesService _licenseMatches;
        private IService<OrganizationLicense, int> _licenses;
        private IOrganizationLicenseService _orgLicenses;
        private IService<Application, int> _applications;
        private IService<Organization, int> _organisations;
        private IService<ProviderEndpoint, int> _providerEndpoints;
        private IService<User, int> _users;
        private INotificationService _notifications;

        private readonly Encoding _encoding = Encoding.Default;

        private DateTime GetDate => DateTime.UtcNow;

        public DataSchemaService(IService<SchemaFile, int> schemaFiles,
            ILicenseMatchesService licenseMatches,
            IService<OrganizationLicense, int> licenses,
            IService<Application, int> apps,
            IService<Organization, int> orgs,
            IService<ProviderEndpoint, int> providerEndpoints,
            IService<User, int> users,
            IOrganizationLicenseService orgLicenses,
            INotificationService notifications,
            IService<DataSchema, int> schemas)
        {
            _schemas = schemas;
            _schemaFiles = schemaFiles;
            _licenseMatches = licenseMatches;
            _licenses = licenses;
            _applications = apps;
            _organisations = orgs;
            _providerEndpoints = providerEndpoints;
            _users = users;
            _notifications = notifications;
            _orgLicenses = orgLicenses;
        }
                
        public List<SchemaModel> GetSchemaModels(bool includeRetracted, LoggedInUserDetails user)
        {
            // Check whether user has appropriate access
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Define result
            var result = new List<SchemaModel>();

            // Get all schemas
            var metaList = _schemas.All();

            // Check whether retracted should be excluded
            if(!includeRetracted)
            {
                metaList = metaList.Where(i => i.RetractedAt == null).ToList();
            }

            // Order schemas
            metaList = metaList.OrderByDescending(i => i.CreatedAt);
            foreach (var item in metaList)
            {
                // Get schema file
                var file = _schemaFiles.FirstOrDefault(p => p.DataSchemaID == item.ID);

                // Setup schema model
                var schemaModel = item.ToModel();
                schemaModel.SchemaFileId = file.ID;

                // Add model to result
                result.Add(schemaModel);
            }

            // Return result
            return result;
        }

        public void Create(SchemaModel model, byte[] fileContent,string fileName, LoggedInUserDetails user)
        {
            // Check whether user has access
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Get file extension
            var fileExtension = Path.GetExtension(fileName);

            // Validate file extension
            if (string.Equals("json", fileExtension, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new BaseException($"Format '{fileExtension}' is not supported");
            }

            // Check whether name for schema already in use
            CheckSchemaName(model.Name, string.Empty);

            // Get file content
            var fileText = _encoding.GetString(fileContent);

            // Setup schema 
            var dataSchema = new DataSchema
            {
                Name = model == null ? "Schema" : model.Name,
                Description = model == null ? string.Empty : model.Description,
                IsIndustryGood = model?.IsIndustryGood ?? false,
                IsAggregate = model?.IsAggregate ?? false,
                UpdatedAt = GetDate,
                UpdatedBy = user.ID,
                Version = 1,
                CreatedAt = GetDate,
                CreatedBy = user.ID,
                Status = (int)TemplateStatus.Draft,
                PublicID = model == null ? string.Empty : model.PublicId
            };

            // Save new schema
            _schemas.Add(dataSchema);

            // Setup schema file
            var schemaFile = new SchemaFile
            {
                DataSchemaID = dataSchema.ID,
                SchemaText = fileText,
                CreatedAt = GetDate,
                IsCurrent = true,
                FileFormat = fileExtension,
                CreatedBy = user.ID
            };

            // Save schema file
            _schemaFiles.Add(schemaFile);
        }
        
        public CustomFileDetails GetReport(LoggedInUserDetails user)
        {
            // Check whether user has access
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get license matches for current month
            var consumerMatchesForMonth = _licenseMatches.GetAllMatchesForMonth(GetDate);

            // Setup string builder
            var fileContent = new StringBuilder();
            fileContent.AppendLine("Provider Name: Endpoint Name,Schema Name,Consumer Name");

            // Get all published licenses
            var providerLicenses = _orgLicenses.GetAllProviderLicensesForMonth(GetDate).ToList();

            // Check whether any provider license was published this month
            if (!providerLicenses.Any())
            {
                throw new BaseException($"No providers who published licenses in {GetDate.ToString("Y")}");
            }

            // Each license process - add to csv provider + schema
            foreach (var providerLicense in providerLicenses)
            {
                fileContent.AppendLine(GetRecordForLicense(consumerMatchesForMonth.ToList(), providerLicense));
            }

            // Setup result
            var result = new CustomFileDetails();
            var fileName = $"Schema_usage_{GetDate.ToString("Y")}.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(fileContent.ToString());
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            result.FileName = fileName;
            result.MimeType = "text/csv";
            result.Content = stream.ToArray();
            return result;
        }
        
        public DataSchema Publish(int dataSchemaId, LoggedInUserDetails user)
        {
            // Check whether user has access
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get data schema
            var result = _schemas.FirstOrDefault(i=>i.ID==dataSchemaId);

            // Check whethre schema is active or retracted
            if (result.Status==(int)TemplateStatus.Retracted || result.Status == (int)TemplateStatus.Active)
            {
                throw new BaseException("Only draft schema can be published");
            }

            // Check whethre schema is in draft
            if (result.Status == (int)TemplateStatus.Draft)
            {
                // Setup publish details
                result.PublishedAt = GetDate;
                result.PublishedBy = user.ID;
                result.Status = (int)TemplateStatus.Active;

                // Save changes
                _schemas.Update(result);
            }

            // Return result
            return result;
        }
        
        public void Retract(int dataSchemaId, LoggedInUserDetails user)
        {
            // Check whether user has access
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get data schema
            var dataSchema = _schemas.FirstOrDefault(i=>i.ID == dataSchemaId);

            // Check whether schema is retracted
            if (dataSchema.Status == (int)TemplateStatus.Retracted)
            {
                throw new BaseException("Schema is already retracted");
            }

            // Setup details about retraction
            dataSchema.RetractedAt = GetDate;
            dataSchema.RetractedBy = user.ID;
            dataSchema.Status = (int)TemplateStatus.Retracted;

            // Save changes
            _schemas.Update(dataSchema);

            // Notify all users about retraction
            NotifyUsersAboutRetraction(dataSchema);
        }

        public CustomFileDetails GetFileDetails(int fileId)
        {
            // Get schema file
            var schemaFile = _schemaFiles.FirstOrDefault(i=>i.ID == fileId);

            // Check whether file exists
            if (schemaFile == null)
            {
                throw new BaseException("File not found");
            }

            // Get metadata for schema file
            var schemaMeta = _schemas.FirstOrDefault(i => i.ID == schemaFile.DataSchemaID);

            // Setup file name
            var fileName = schemaMeta.Name + schemaFile.FileFormat;

            // Setup stream
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(schemaFile.SchemaText);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            // Setup result
            var result = new CustomFileDetails()
            {
                Content = stream.ToArray(),
                FileName = fileName,
                MimeType = "application/json"
            };

            // Return result
            return result;
        }

        public bool IsSchemaIdNotExists(string publicid, string initialId)
        {
            if (string.Equals(publicid, initialId, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            var result = _schemas.All().All(x => !string.Equals(x.PublicID, publicid, StringComparison.CurrentCultureIgnoreCase));

            return result;
        }

        public bool IsSchemaNameNotExists(string name, string InitialName)
        {
            if (string.Equals(name, InitialName, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            var result = _schemas.All().All(x => !string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));

            return result;
        }

        private void NotifyUsersAboutRetraction(DataSchema dataSchema)
        {
            // Get licenses for the schema
            var licenses = _licenses.Where(i => i.DataSchemaID == dataSchema.ID);

            // Get applciations for each license
            var applications = licenses.Select(license => _applications.FirstOrDefault(i=>i.ID==license.ApplicationID));

            // Get organisations for each application
            var organizations = applications.Select(application => _organisations.FirstOrDefault(i=>i.ID== application.OrganizationID)).Select(i => i.ID).Distinct();

            // Get users for each organisation
            var users = new List<User>();
            foreach (var organizationId in organizations)
            {
                // Get users
                var activeOrgMembers = _users.Where(i => i.OrganizationID == organizationId).Where(i => i.IsActive == true);

                // Add to list
                users.AddRange(activeOrgMembers);
            }

            // Each member of organization that uses this schema will be notified about retraction
            foreach (var member in users)
            {
                // Process notification for user
                _notifications.User.SchemaRetractedInBackground(member.ID, dataSchema.Name);
            }
        }

        private string GetRecordForLicense(List<LicenseMatch> consumerMatchesForMonth, OrganizationLicense providerLicense)
        {
            var licenseMatches = consumerMatchesForMonth.Where(i => i.ProviderLicenseID == providerLicense.ID).ToList();
            var endpoint = _providerEndpoints.FirstOrDefault(i=>i.ID==providerLicense.ProviderEndpointID);
            var schema = _schemas.FirstOrDefault(i=>i.ID==endpoint.DataSchemaID);
            var providerApp = _applications.FirstOrDefault(i=>i.ID==endpoint.ApplicationId);
            var provider = _organisations.FirstOrDefault(i=>i.ID == providerApp.OrganizationID);

            // Add to report if consumers have any license matches to provider's license
            if (licenseMatches.Any())
            {
                foreach (var licenseMatch in licenseMatches)
                {
                    var consumerLicense = _licenses.FirstOrDefault(i => i.ID == licenseMatch.ConsumerLicenseID);
                    var consumerApp = _applications.FirstOrDefault(i => i.ID == consumerLicense.ApplicationID);
                    var consumer = _organisations.FirstOrDefault(i => i.ID == consumerApp.OrganizationID);
                    return $"{provider.Name}:{endpoint.Name},{schema.Name},{consumer.Name}";
                }
            }

            return $"{provider.Name}:{endpoint.Name},{schema.Name}";
        }

        private void CheckSchemaName(string schemaName, string initialName)
        {
            if (schemaName == initialName)
            {
                return;
            }

            // Check whether name for schema already in use
            var isSchemaExists = _schemas.All().Any(x => string.Equals(x.Name, schemaName, StringComparison.CurrentCultureIgnoreCase) == true);
            if (isSchemaExists)
            {
                throw new BaseException("Schema name already in use");
            }
        }

        public SchemaModel GetModel(int schemaId, LoggedInUserDetails user)
        {
            // Check whether user has access
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Get schema
            var schema = _schemas.FirstOrDefault(i=>i.ID == schemaId);

            // Check whether schema is retracted
            if (schema.Status == (int)TemplateStatus.Retracted)
            {
                throw new BaseException("Can not edit retracted schema");
            }

            // Get schema file
            var file = _schemaFiles.FirstOrDefault(p => p.DataSchemaID == schema.ID);

            // Setup result model
            var result = schema.ToModel();

            // Setup schema file Id
            result.SchemaFileId = file.ID;

            // return result
            return result;
        }

        public void Update(SchemaModel model, MemoryStream stream, string fileName, LoggedInUserDetails user)
        {
            // Check whether user has access
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Get data schema
            var dataSchema = _schemas.FirstOrDefault(i => i.ID == model.DataSchemaID);

            // Check whether file needs to be updated
            if (stream != null)
            {
                // Get schema file
                var schemaFile = _schemaFiles.FirstOrDefault(p => p.DataSchemaID == dataSchema.ID);

                // Get file extension
                var fileExtension = Path.GetExtension(fileName);

                // Validate file extension
                if (string.Equals("json", fileExtension, StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new BaseException($"Format '{fileExtension}' is not supported");
                }

                // Udpate schema file details
                schemaFile.SchemaText = _encoding.GetString(stream.ToArray());
                schemaFile.FileFormat = fileExtension;

                // Save changes to schema file
                _schemaFiles.Update(schemaFile);
            }

            // Validate schema name
            CheckSchemaName(model.Name, dataSchema.Name);

            // Check whether data schema is retracted
            if (dataSchema.Status == (int)TemplateStatus.Retracted)
            {
                throw new BaseException("Can not edit retracted schema");
            }

            // Check whether data schema is active
            if (dataSchema.Status != (int)TemplateStatus.Active)
            {
                // Update data if schema is not active
                dataSchema.Name = string.IsNullOrEmpty(model.Name) ? "Schema" : model.Name;
                dataSchema.PublicID = model.PublicId;
                dataSchema.IsIndustryGood = model.IsIndustryGood;
                dataSchema.IsAggregate = model.IsAggregate;
            }

            // Update other data
            dataSchema.Description = model.Description;
            dataSchema.UpdatedBy = user.ID;
            dataSchema.UpdatedAt = GetDate;

            // Save changes
            _schemas.Update(dataSchema);
        }

        public IEnumerable<SchemaDetails> GetSchemas(bool isAggregateOnly)
        {
            List<SchemaDetails> result = _schemas.Where(i => i.Status == (int)TemplateStatus.Active).Select(i => i.ToDetails()).ToList();
            if (isAggregateOnly)
            {
                result = result.Where(i => i.is_aggregate == true).ToList();
            }
            return result;
        }
    }
}

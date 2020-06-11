using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Security;
using Rezare.CommandBuilder.Services;
using System;
using System.IO;
using System.Linq;

namespace DataLinker.Services.CustomLicenses
{
    internal class CustomLicenseService: ICustomLicenseService
    {
        private readonly IService<CustomLicense, int> _customLicenses;
        private readonly ISecurityService _security;
        private readonly IService<OrganizationLicense, int> _orgLicenses;
        private readonly IService<ProviderEndpoint, int> _endpoints;

        private DateTime GetDate => DateTime.UtcNow;

        public CustomLicenseService(IService<CustomLicense, int> customLicenses,
            IService<OrganizationLicense, int> orgLicenses,
            IService<ProviderEndpoint, int> endpoints,
            ISecurityService security)
        {
            _security = security;
            _orgLicenses = orgLicenses;
            _customLicenses = customLicenses;
            _endpoints = endpoints;
        }

        public CustomLicense Add(int applicationId, int schemaId, MemoryStream stream, string mimeType, string fileName, LoggedInUserDetails user)
        {
            // Check access
            _security.CheckAccessToApplication(user, applicationId);

            // Setup data
            var customLicense = new CustomLicense
            {
                Content = stream.ToArray(),
                CreatedBy = user.ID.Value,
                MimeType = mimeType,
                FileName = fileName,
                CreatedAt = GetDate
            };

            // Save custom license
            _customLicenses.Add(customLicense);

            // Create org license for custom license
            CreateOrganisationLicense(applicationId, schemaId, user, customLicense.ID);

            // Return result
            return customLicense;
        }
        
        public CustomLicense Get(int applicationId, int schemaId, int organisationLicenseId, LoggedInUserDetails user)
        {
            // Check access
            _security.CheckAccessToApplication(user, applicationId);

            // Get organisation license
            var orgLicense = _orgLicenses.FirstOrDefault(i => i.ID == organisationLicenseId);

            // Check whether org license is related to custom license
            if(orgLicense.CustomLicenseID == null)
            {
                throw new BaseException("Provided license is not related custom license.");
            }

            // Get data
            var result = _customLicenses.FirstOrDefault(i => i.ID == orgLicense.CustomLicenseID.Value);

            // Return result
            return result;
        }

        private void CreateOrganisationLicense(int applicationId, int schemaId, LoggedInUserDetails user, int customLicenseId)
        {
            // Get provider endpoint
            var endpoint = _endpoints.Where(i => i.ApplicationId == applicationId).FirstOrDefault(i => i.DataSchemaID == schemaId);

            // Setup organisation license for custom license
            var orgLicense = new OrganizationLicense
            {
                ApplicationID = applicationId,
                CreatedAt = GetDate,
                CreatedBy = user.ID.Value,
                ProviderEndpointID = endpoint.ID,
                DataSchemaID = schemaId,
                Status = (int)PublishStatus.Draft,
                CustomLicenseID = customLicenseId
            };

            // Save organisation license
            _orgLicenses.Add(orgLicense);
        }
    }
}

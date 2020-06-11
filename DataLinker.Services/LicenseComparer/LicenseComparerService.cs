using System;
using System.Collections.Generic;
using System.Linq;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Models;
using Rezare.CommandBuilder.Services;
using DataLinker.Database.Models;

namespace DataLinker.Services.OrganizationLicenses
{
    internal class LicenseComparerService : ILicenseComparerService
    {
        private IService<OrganizationLicense, int> _licenseService;
        private IService<Application, int> _applicationService;
        private IService<LicenseClauseTemplate, int> _clauseTemplateService;
        private IService<LicenseSection, int> _sectionService;
        private IService<LicenseClause, int> _clauseService;
        private IService<OrganizationLicenseClause, int> _licenseClauseService;
        private IOrganizationLicenseClauseService _licenseClauses;
        private IService<LicenseTemplate, int> _licenseTemplates;
        private IService<ProviderEndpoint, int> _providerEndpoints;
        private IService<Organization, int> _organisations;
        private IService<DataSchema, int> _schemas;

        public LicenseComparerService(
            IService<Application, int> applicationService,
            IService<LicenseClauseTemplate, int> clauseTemplates,
            IService<LicenseSection, int> sections,
            IService<LicenseClause, int> clauses,
            IService<DataSchema, int> schemas,
            IService<OrganizationLicenseClause, int> orgLicenseClauseService,
            IService<LicenseTemplate, int> licenseTemplates,
            IService<Organization, int> organisations,
            IOrganizationLicenseClauseService licenseClauses,
            IService<ProviderEndpoint, int> providerEndpoints,
            IService<OrganizationLicense, int> organisationLicenses
            )
        {
            _applicationService = applicationService;
            _clauseTemplateService = clauseTemplates;
            _sectionService = sections;
            _clauseService = clauses;
            _licenseClauseService = orgLicenseClauseService;
            _licenseTemplates = licenseTemplates;
            _organisations = organisations;
            _providerEndpoints = providerEndpoints;
            _licenseClauses = licenseClauses;
            _licenseService = organisationLicenses;
            _schemas = schemas;
        }
        
        // Similar to private method
        public ComparisonResult CompareClauses(OrganizationLicenseClause providerClause, OrganizationLicenseClause consumerClause)
        {
            var result = new ComparisonResult();

            // Check whether clause selected by consumer 
            if (providerClause.LicenseClauseID != consumerClause.LicenseClauseID)
            {
                // Setup result
                result.IsMatch = false;
                result.Message = "Not selected";

                // Return result
                return result;
            }
            // Get clasue template
            var clauseTemplate = _clauseTemplateService.FirstOrDefault(i => i.LicenseClauseID == providerClause.LicenseClauseID);
            var clauseType = (ClauseType)clauseTemplate.ClauseType;

            // Setup provider value
            var providerValue = new ClauseValue(providerClause.ClauseData, clauseType);

            // Setup consumer value
            var consumerValue = new ClauseValue(consumerClause.ClauseData, clauseType);

            // Compare values
            result = CompareClauseValues(providerValue, consumerValue);

            // Return result
            return result;
        }

        public ProviderComparisonSummary GetSchemaProviders(int schemaId, LoggedInUserDetails user)
        {
            // Setup result model
            var result = new ProviderComparisonSummary();

            // Get data schema
            var schema = _schemas.FirstOrDefault(i => i.ID == schemaId);

            // Setup schema name
            result.SchemaName = schema.Name;

            var providerLicenses = _licenseService.Where(i => i.DataSchemaID == schemaId).Where(i => i.Status == (int)PublishStatus.Published && i.ProviderEndpointID != 0);
            var providerMatches = providerLicenses.Select(i => new ProviderMatch { ProviderLicenseId = i.ID, OrganizationName = "Test Org" });
            // Get provider list for selection
            result.Endpoints = providerMatches.ToList();
            
            // Return result
            return result;
        }

        private List<ProviderMatch> GetAllSchemaProviders(int schemaId)
        {
            // Define result
            var result = new List<ProviderMatch>();

            var providerLicenses = _licenseService.Where(i => i.Status == (int)PublishStatus.Published)
                .Where(i => i.ProviderEndpointID != 0 && i.DataSchemaID == schemaId);

            if (!providerLicenses.Any())
            {
                // TODO: show notification message before user made license choices instead
                throw new BaseException("No providers who have published licenses for this schema based on current license template.");
            }

            // Process each published provider license
            foreach (var license in providerLicenses)
            {
                // Get provider endpoint
                var endpoint = _providerEndpoints.FirstOrDefault(i => i.ID == license.ProviderEndpointID);

                // Get provider application
                var app = _applicationService.FirstOrDefault(i => i.ID == endpoint.ApplicationId);

                // Get provider organization
                var org = _organisations.FirstOrDefault(i => i.ID == app.OrganizationID);

                // Setup provider details
                var match = new ProviderMatch();
                match.OrganizationName = org.Name;
                match.EndpointName = endpoint.Name;
                match.EndpointId = endpoint.ID;
                match.ProviderLicenseId = license.ID;
                match.IsMatch = true;

                // Add details to result
                result.Add(match);
            }

            // Return result
            return result;
        }

        private ProviderMatch SetupComparedProviderDetails(OrganizationLicense orgLicense, List<ClauseMatch> comparedClauses)
        {
            // Get provider endpoint
            var endpoint = _providerEndpoints.FirstOrDefault(i=>i.ID == orgLicense.ProviderEndpointID);

            // Get provider application
            var app = _applicationService.FirstOrDefault(i=>i.ID == endpoint.ApplicationId);

            // Get provider organization
            var org = _organisations.FirstOrDefault(i=>i.ID == app.OrganizationID);

            // Setup result
            var result = new ProviderMatch();
            result.Clauses = new List<ClauseMatch>();
            result.OrganizationName = org.Name;
            result.EndpointName = endpoint.Name;
            result.EndpointId = endpoint.ID;
            result.ProviderLicenseId = orgLicense.ID;

            // Setup compared clauses
            result.Clauses = comparedClauses;

            // Setup provider match flag
            result.IsMatch = !comparedClauses.Any(i => i.IsMatched == false);
            //result.IsMatch = true;

            // Return result
            return result;
        }

        private List<ClauseMatch> CompareWithConsumerSelection(List<SectionsWithClauses> consumerSelections, List<OrganizationLicenseClause> providerClauses)
        {
            // Define result
            var result = new List<ClauseMatch>();

            // Process each provider clause
            foreach (var clause in providerClauses)
            {
                // Get consumer clause
                var consumerClause = GetClauseFromSelection(clause.LicenseClauseID, consumerSelections);

                // Compare clauses
                var comparisonResult = CompareClauses(clause, consumerClause);

                // Setup clause model for provider
                var comparedClauseDetails = SetupComparedClauseDetails(clause, comparisonResult);

                // Setup result
                result.Add(comparedClauseDetails);
            }

            // Return result
            return result;
        }

        private ClauseMatch SetupComparedClauseDetails(OrganizationLicenseClause clause, ComparisonResult comparisonResult)
        {
            // Get license clause
            var licenseClause = _clauseService.FirstOrDefault(i => i.ID == clause.LicenseClauseID);

            // Get license template
            var licenseTemplate = _clauseTemplateService.FirstOrDefault(i => i.LicenseClauseID == licenseClause.ID);

            // Get section
            var section = _sectionService.FirstOrDefault(i=>i.ID == licenseClause.LicenseSectionID);

            // Setup result
            var result = new ClauseMatch
            {
                ClauseId = licenseClause.ID,
                SectionName = section.Title,
                Value = licenseTemplate.ShortText,
                Message = comparisonResult.Message,
                IsMatched = comparisonResult.IsMatch
            };

            // Return result
            return result;
        }

        private ClauseModel GetClauseFromSelection(int clauseId, List<SectionsWithClauses> model)
        {
            foreach (var section in model)
            {
                var clause = section.Clauses.FirstOrDefault(i => i.ClauseId == clauseId);
                if (clause != null)
                {
                    return clause;
                }
            }

            return null;
        }

        private ComparisonResult CompareClauses(OrganizationLicenseClause providerClause, ClauseModel consumerClause)
        {
            var result = new ComparisonResult();

            // Check whether clause selected by consumer 
            if (!consumerClause.IsSelectedByConsumer)
            {
                // Setup result
                result.IsMatch = false;
                result.Message = "Not selected";

                // Return result
                return result;
            }

            // Setup provider value
            var providerTemplate = _clauseTemplateService.FirstOrDefault(i => i.LicenseClauseID == providerClause.LicenseClauseID);
            var providerValue = new ClauseValue(providerClause.ClauseData, (ClauseType)providerTemplate.ClauseType);

            // Setup consumer value
            var consumerClauseData = _licenseClauses.GetClauseData(consumerClause);
            var consumerValue = new ClauseValue(consumerClauseData, (ClauseType)consumerClause.Type);

            // Compare values
            result = CompareClauseValues(providerValue, consumerValue);

            // Return result
            return result;
        }

        private ComparisonResult CompareClauseValues(ClauseValue providerSelection, ClauseValue consumerSelection)
        {
            // Define result
            var result = new ComparisonResult();

            // Setup default value for comparison result
            result.IsMatch = true;

            // Compare
            switch (providerSelection.Type)
            {
                case ClauseType.Text:
                    result.IsMatch = providerSelection.Type == consumerSelection.Type;
                    break;
                case ClauseType.Input:
                    // Check whether input values are not numbers
                    var isCustomProviderText = (providerSelection.Number == null && consumerSelection.Number == null);

                    // Compare numbers
                    if (!isCustomProviderText && providerSelection.Number > consumerSelection.Number)
                    {
                        result.IsMatch = false;
                        result.Message = $"To match you should specify {providerSelection.Number} or more. You specified {consumerSelection.Number}";
                    }
                    break;

                case ClauseType.InputAndDropDown:
                    // Compare numbers
                    if (providerSelection.Number > consumerSelection.Number)
                    {
                        result.IsMatch = false;
                        result.Message += $"To match you should specify {providerSelection.Number} or more. You specified {consumerSelection.Number}. ";
                    }

                    // Compare dropdown selection
                    if (!string.Equals(providerSelection.ListItem, consumerSelection.ListItem, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.IsMatch = false;
                        result.Message += $"To match you should select {providerSelection.ListItem}. You selected {consumerSelection.ListItem}. ";
                    }
                    break;
                default: throw new Exception("Unknown clause type.");
            }

            // Return result
            return result;
        }
    }
}
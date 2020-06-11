using System;
using System.Collections.Generic;
using System.Linq;
using DataLinker.Database.Models;
using DataLinker.Models.Enums;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Services.OrganizationLicenses
{
    public class LicenseMatchesService : ILicenseMatchesService
    {
        private readonly IService<LicenseMatch, int> _service;
        private readonly ILicenseComparerService _licenseComparer;
        private readonly IService<OrganizationLicenseClause, int> _organisationClauses;
        private readonly IService<OrganizationLicense, int> _organisationLicenses;

        public DateTime GetDate => DateTime.UtcNow;

        public LicenseMatchesService(
            IService<LicenseMatch, int> service,
            IService<OrganizationLicense, int> organisationLicenses,
            IService<OrganizationLicenseClause, int> organisationClauses,
            ILicenseComparerService licenseComparer
            )
        {
            _service = service;
            _licenseComparer = licenseComparer;
            _organisationLicenses = organisationLicenses;
            _organisationClauses = organisationClauses;
        }

        public IEnumerable<LicenseMatch> GetAllMatchesForMonth(DateTime day)
        {
            // Start of the month
            var startDay = new DateTime(day.Year, day.Month, 1);

            // End of the month
            var endDay = startDay.AddMonths(1).AddDays(-1);

            // Query data
            var matches = _service.Where(i => i.CreatedAt >= startDay).Where(i => i.CreatedAt <= endDay).ToList();

            // Return result
            return matches;
        }

        public List<LicenseMatch> GetForProvider(int providerId)
        {
            // Check whether user has access to licenses

            // Setup result
            var result = _service.Where(i => i.ProviderLicenseID == providerId).ToList();

            // Return result
            return result;
        }
        
        public List<LicenseMatch> GetForConsumer(int consumerId)
        {
            // Check whether user has access to licenses

            // Setup result
            var result = _service.Where(i => i.ConsumerLicenseID == consumerId).ToList();

            // Return result
            return result;
        }

        public void Add(LicenseMatch data)
        {
            // Validate data

            // Save data
            _service.Add(data);
        }

        public void UpdateWithNewProvider(int schemaId, int providerLicenseId, int userId)
        {
            // Get provider clauses for a given license
            var providerClauses = _organisationClauses.Where(i => i.OrganizationLicenseID == providerLicenseId).ToList();

            // Get published licenses
            var publishedLicenses = _organisationLicenses.Where(i => i.Status == (int)PublishStatus.Published);

            // Get consumer licenses for a given schema from published
            var consumerLicenses = publishedLicenses.Where(i => i.ProviderEndpointID == 0 && i.DataSchemaID == schemaId);

            // for each consumer get clause selection
            foreach(var consumerLicense in consumerLicenses)
            {
                // Compare provider selections with consumer selection
                var isMatch = IsClausesMatch(consumerLicense.ID, providerClauses);
                if(isMatch)
                {
                    // Add consumer matches for those who match provider selections
                    // Setup license match
                    var licenseMatch = new LicenseMatch
                    {
                        ConsumerLicenseID = consumerLicense.ID,
                        ProviderLicenseID = providerLicenseId,
                        CreatedAt = GetDate,
                        CreatedBy = userId
                    };

                    // Save license match
                    _service.Add(licenseMatch);
                }
            }
        }

        private bool IsClausesMatch(int consumerLicenseId, List<OrganizationLicenseClause> providerClauses)
        {
            // Get consumer clauses
            var consumerClauses = _organisationClauses.Where(i => i.OrganizationLicenseID == consumerLicenseId);
            foreach (var providerClause in providerClauses)
            {
                // Get consumer clause
                var consumerClause = consumerClauses.FirstOrDefault(i => i.LicenseClauseID == providerClause.LicenseClauseID);
                // Check whether consumer selected same clause
                if (consumerClause == null)
                {
                    return false;
                }

                // Compare clauses
                var comparisonResult = _licenseComparer.CompareClauses(providerClause, consumerClause);
                if (!comparisonResult.IsMatch)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}
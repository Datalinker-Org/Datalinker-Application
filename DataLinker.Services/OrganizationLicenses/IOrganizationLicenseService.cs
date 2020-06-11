using System;
using System.Collections.Generic;
using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.OrganizationLicenses
{
    public interface IOrganizationLicenseService
    {        
        IEnumerable<OrganizationLicense> GetAllProviderLicensesForMonth(DateTime today);
        
        IEnumerable<OrganizationLicense> GetForApplicationAndSchema(int appId, int schemaId, bool isPublished = false);

        ProviderLicensesModel SetupProviderLicensesModel(int applicationId, int schemaId, LoggedInUserDetails user);

        BuildLicenseModel SetupBuildLicenseModel(int applicationId, int schemaId, LoggedInUserDetails user);

        void RequestLicenseVerification(int id, int appId, int schemaId, LoggedInUserDetails user);

        void Publish(int id, int appId, int schemaId, LoggedInUserDetails user);

        void Retract(int licenseId, LoggedInUserDetails user);

        void Draft(int licenseId, LoggedInUserDetails user);

        void CreateProviderTemplatedLicense(int appId, int schemaId, BuildLicenseModel model, LoggedInUserDetails user);

        void CreateConsumerTemplatedLicense(int appId, int schemaId, int providerLicenseId, LoggedInUserDetails user);

        List<OrganizationLicense> CreateConsumerCustomLicense(int appId, int schemaId, List<DataProvider> providers, LoggedInUserDetails user);
    }
}
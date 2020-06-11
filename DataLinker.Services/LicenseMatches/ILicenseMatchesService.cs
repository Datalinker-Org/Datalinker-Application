using System;
using System.Collections.Generic;
using DataLinker.Database.Models;

namespace DataLinker.Services.OrganizationLicenses
{
    public interface ILicenseMatchesService
    {
        IEnumerable<LicenseMatch> GetAllMatchesForMonth(DateTime today);

        List<LicenseMatch> GetForProvider(int providerId);

        List<LicenseMatch> GetForConsumer(int consumerId);

        void Add(LicenseMatch data);

        void UpdateWithNewProvider(int schemaId, int providerLicenseId, int userId);
    }
}
using DataLinker.Database.Models;
using DataLinker.Models;
using System.IdentityModel.Tokens;

namespace DataLinker.Services.SoftwareStatements
{
    public interface ISoftwareStatementService
    {
        SoftwareStatement GetValidStatement(int applicationId, LoggedInUserDetails user, int orgId);

        string Get(int applicationId, LoggedInApplication app, int orgId);

        string GetSignedAndEncodedToken(LoggedInApplication loggedInApp);

        SoftwareStatement GetNewStatement(int applicationId, int loggedInUserId, int organizationId);

        SoftwareStatement UpdateSoftwareStatement(int applicationId, LoggedInUserDetails user, int organisationId);

        StatementValidationResult GetValidationResult(string softwareStmt, string scope, LoggedInApplication loggedInApp);

        void CreateLicenseAgreement(LicenseDetails licenseDetails, LoggedInApplication loggedInApp);
    }
}
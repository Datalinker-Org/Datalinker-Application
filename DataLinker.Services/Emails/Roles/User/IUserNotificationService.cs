using DataLinker.Models.Enums;

namespace DataLinker.Services.Emails.Roles.User
{
    public interface IUserNotificationService
    {
        void EmailVerification(int userId, string url);

        void EmailInvitation(int userId, string url, int inviterID);

        void EmailVerificationInBackground(int userId, string url);

        void EmailInvitationInBackground(int userId, string url, int inviterID);

        void OrganizationIsActive(int orgId, ActivationState state);
        void OrganizationIsActiveJob(string toEmail, string toName, string orgName, ActivationState state);

        void OrganizationIsActiveInBackground(int orgId, ActivationState state);

        void StatusForLicenseUpdated(int userId, string url, string schemaName, string organizationName, bool isProvider, int newStatus);

        void StatusForLicenseUpdatedInBackground(int userId, string url, string schemaName, string organizationName, bool isProvider, int newStatus);

        void SchemaRetracted(int userId, string schemaName);

        void SchemaRetractedInBackground(int userId, string schemaName);

        void NewClause(int clauseTemplateId);
        void NewClauseJob(string toEmail, string toName, string clauseDescr);

        void NewClauseInBackground(int clauseId);

        void NewProviderLicense(int licenseId, int applicationId, string linkToLicense);

        void NewProviderLicenseJob(string toEmail, string toName, string orgName, string schemaName,
            string linkToLicense);

        void NewProviderLicenseInBackground(int licenseId, int applicationId, string linkToLicense);

        void UpdatedAccountState(int userId, ActivationState state);

        void UpdatedAccountStateInBackground(int userId, ActivationState state);

        void SoftwareStatementUpdated(int orgId, string urlToApplicationDetails);
        void SoftwareStatementUpdatedJob(string toEmail, string toName, string orgName, string urlToApplicationDetails);

        void SoftwareStatementUpdatedInBackground(int orgId, string urlToApplicationDetails);
        void LegalOfficerRegisteredJob(int userId);
        void LegalOfficerRegisteredInBackground(int userId);
    }
}

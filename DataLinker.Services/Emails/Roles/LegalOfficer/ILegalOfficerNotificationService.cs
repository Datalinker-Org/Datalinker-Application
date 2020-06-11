namespace DataLinker.Services.Emails.Roles.LegalOfficer
{
    public interface ILegalOfficerNotificationService
    {
        void LicenseIsPendingApproval(int userId,
            string linkToConfirmScreen,
            string linkToSchema,
            string linkToDataLinker,
            string schemaName,
            int licenseId);

        void LicenseIsPendingApprovalInBackground(int userId,
            string linkToConfirmScreen,
            string linkToSchema,
            string linkToDataLinker,
            string schemaName,
            int licenseId);

        void LicenseVerificationRequired(int userId,
            string linkToConfirmScreen,
            string schemaName,
            int licenseId);

        void LicenseVerificationRequiredInBackground(int userId,
            string linkToConfirmScreen,
            string schemaName,
            int licenseId);

        void LicenseApprovedSuccessfully(int userId,
            string linkToSchema,
            string linkToDataLinker,
            string schemaName,
            int licenseId,
            bool isProvider);

        void LicenseApprovedSuccessfullyInBackground(int userId,
            string linkToSchema,
            string linkToDataLinker,
            string schemaName,
            int licenseId,
            bool isProvider);

        void LicenseAgreementCreated(int licenseId,
            string linkToLicense,
            string linkToSchema,
            string urlToDataLinker);

        void LicenseAgreementCreatedJob(int providderLicenseId,
            int agreementId,
            string linkToLicense,
            string linkToSchema,
            string urlToDataLinker,
            string toEmail,
            string toName,
            string consumerOrg,
            string providerOrg,
            string schemaName);

        void LicenseAgreementCreatedInBackground(int licenseId,
            string linkToLicense,
            string linkToSchema,
            string urlToDataLinker);

        void NewConsumerRequest(string linkToConsumerRequests,
            int userId);

        void NewConsumerRequestInBackground(int applicationId,
            string linkToConsumerRequests);


        void RejectedConsumerRequest(string schemaName,
            string providerName,
            int userId);

        void RejectedConsumerRequestInBackground(int applicationId,
            int schemaId);

        void ApprovedConsumerRequest(string schemaName,
            string providerName,
            int userId);

        void ApprovedConsumerRequestInBackground(int applicationId,
            int schemaId);
    }
}

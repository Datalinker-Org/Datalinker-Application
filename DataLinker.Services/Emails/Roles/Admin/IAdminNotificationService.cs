namespace DataLinker.Services.Emails.Roles.Admin
{
    public interface IAdminNotificationService
    {
        void UpdatedStatusForLicense(int userId, string url, int newStatus);

        void UpdatedStatusForLicenseInBackground(int userId, string url, int newStatus);

        /// <summary>
        /// Sends email to all admins about new legal officer
        /// </summary>
        /// <param name="url">Url to new legal officer details</param>
        /// <param name="organizationName"></param>
        void NewLegalOfficer(string url, string organizationName);

        /// <summary>
        /// Sends email to all admins about new legal officer in background
        /// </summary>
        /// <param name="url">Url to new legal officer details</param>
        /// <param name="organizationName"></param>
        void NewLegalOfficerInBackground(string url, string organizationName);

        void NewLegalOfficerJob(string url, string organizationName, string toEmail, string toName);

        void NewIndustryGoodApplication(string url, int orgId);

        void NewIndustryGoodApplicationInBackground(string url, int orgId);
        void NewIndustryGoodApplicationJob(string url, string orgName, string toName, string toEmail);
        /// <summary>
        /// Sends email to all admins
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="orgName"></param>
        /// <param name="userDetailsUrl"></param>
        /// <param name="orgDetailsUrl"></param>
        void NewOrganization(string userName, string orgName, string userDetailsUrl, string orgDetailsUrl);

        void NewOrganizationJob(string userName, string orgName, string userDetailsUrl, string orgDetailsUrl,
            string toName, string toEmail);
        /// <summary>
        /// Sends email to all admins in background
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="orgName"></param>
        /// <param name="userDetailsUrl"></param>
        /// <param name="orgDetailsUrl"></param>
        void NewOrganizationInBackground(string userName, string orgName, string userDetailsUrl, string orgDetailsUrl);
    }
}

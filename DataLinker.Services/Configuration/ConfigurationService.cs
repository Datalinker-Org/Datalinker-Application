namespace DataLinker.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        public string ConnectionString { get; set; }
        public string SmtpServer { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPort { get; set; }
        public bool SmtpUseDefault { get; set; }
        public bool SmtpUseSsl { get; set; }
        public string SmtpPassword { get; set; }
        public string SmtpSendEmail { get; set; }
        public string ConfirmNewEmailExpires { get; set; }
        public string ApprovalLinkExpiresIn { get; set; }
        public string PathToTempFiles { get; set; }
        public string DataLinkerHost { get; set; }

        // Page size for list views
        public int ManageUsersPageSize { get; set; }
        public int ManageSchemasPageSize { get; set; }
        public int ManageLicensesPageSize { get; set; }
        public int ManageApplicationsPageSize { get; set; }
        public int ManageOrganizationsPageSize { get; set; }
        public int ManageLicenseTemplatesPageSize { get; set; }
    }
}

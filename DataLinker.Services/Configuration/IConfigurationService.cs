namespace DataLinker.Services.Configuration
{
    public interface IConfigurationService
    {
        string ConnectionString { get; set; }
        string SmtpServer { get; set; }
        string SmtpUser { get; set; }
        string SmtpPort { get; set; }
        bool SmtpUseDefault { get; set; }
        bool SmtpUseSsl { get; set; }
        string SmtpPassword { get; set; }
        string SmtpSendEmail { get; set; }
        string ConfirmNewEmailExpires { get; set; }
        string ApprovalLinkExpiresIn { get; set; }
        string PathToTempFiles { get; set; }
        string DataLinkerHost { get; set; }

        // page size for list views
        int ManageUsersPageSize { get; set; }
        int ManageSchemasPageSize { get; set; }
        int ManageLicensesPageSize { get; set; }
        int ManageApplicationsPageSize { get; set; }
        int ManageOrganizationsPageSize { get; set; }
        int ManageLicenseTemplatesPageSize { get; set; }
    }
}
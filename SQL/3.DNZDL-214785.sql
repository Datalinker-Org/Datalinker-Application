DECLARE @issueNumber VARCHAR(50) = '3.DNZDL-214785'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION
		
			IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ConsumerLicenseRequests')
			BEGIN
				DROP TABLE [dbo].[ConsumerLicenseRequests];
			END
			
			IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProviderEndpointLicenseApprovalRequests')
			BEGIN
				DROP TABLE [dbo].[ProviderEndpointLicenseApprovalRequests];
			END
			
			IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProviderEndpointLicenseClauses')
			BEGIN
				DROP TABLE [dbo].[ProviderEndpointLicenseClauses];
			END
						
			IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProviderEndpointLicenses')
			BEGIN
				DROP TABLE [dbo].[ProviderEndpointLicenses];
			END
						
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrganizationLicenses')
			BEGIN
				CREATE TABLE [dbo].[OrganizationLicenses](
					[ID] [int] IDENTITY(1,1) NOT NULL,
					[CreatedAt] [datetime2](7) NOT NULL,
					[CreatedBy] [int] NOT NULL,
					[ApprovedAt] [datetime2](7) NULL,
					[ApprovedBy] [int] NULL,
					[PublishedAt] [datetime2](7) NULL,
					[PublishedBy] [int] NULL,				
					[UpdatedAt] [datetime2](7) NULL,
					[UpdatedBy] [int] NULL,
					[ProviderEndpointID] [int] NULL,
					[ApplicationID] [int] NOT NULL,
					[LicenseTemplateID] [int] NOT NULL,
					[Status] [int] NOT NULL,						
					[DataSchemaID] [int] NOT NULL,
					CONSTRAINT [PK_OrganizationLicenses] PRIMARY KEY CLUSTERED ([ID]),
					CONSTRAINT [FK_OrganizationLicenses_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([ID]),
					CONSTRAINT [FK_OrganizationLicenses_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[Users] ([ID]),
					CONSTRAINT [FK_OrganizationLicenses_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [dbo].[Users] ([ID]),
					CONSTRAINT [FK_OrganizationLicenses_PublishedBy] FOREIGN KEY ([PublishedBy]) REFERENCES [dbo].[Users] ([ID]),
					CONSTRAINT [FK_OrganizationLicenses_ApplicationID] FOREIGN KEY ([ApplicationID]) REFERENCES [dbo].[Applications] ([ID]),
					CONSTRAINT [FK_OrganizationLicenses_DataSchemaID] FOREIGN KEY ([DataSchemaID]) REFERENCES [dbo].[DataSchemas] ([ID]),
					CONSTRAINT [FK_OrganizationLicenses_LicenseTemplateID] FOREIGN KEY ([LicenseTemplateID]) REFERENCES [dbo].[LicenseTemplates] ([ID]));
			END
						
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LicenseApprovalRequests')
			BEGIN
				CREATE TABLE [dbo].[LicenseApprovalRequests](
					[ID] [int] IDENTITY(1,1) NOT NULL,
					[OrganizationLicenseID] int NOT NULL,
					[SentAt] datetime2 NOT NULL,
					[SentBy] int NOT NULL,
					[SentTo] int NOT NULL,
					[ExpiresAt] datetime2 NOT NULL,
					[accessToken] nvarchar(255) NOT NULL,
					[Token] nvarchar(255) NOT NULL,
					CONSTRAINT [PK_LicenseApprovalRequests] PRIMARY KEY CLUSTERED ([ID]),
					CONSTRAINT [FK_LicenseApprovalRequests_OrganizationLicenseID] FOREIGN KEY ([OrganizationLicenseID]) REFERENCES [dbo].[OrganizationLicenses] ([ID]),
					CONSTRAINT [FK_LicenseApprovalRequests_SentBy] FOREIGN KEY ([SentBy]) REFERENCES [dbo].[Users] ([ID]),
					CONSTRAINT [FK_LicenseApprovalRequests_SentTo] FOREIGN KEY ([SentTo]) REFERENCES [dbo].[Users] ([ID])
				);
			END
			
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrganizationLicenseClauses')
			BEGIN
				CREATE TABLE [dbo].[OrganizationLicenseClauses](
					[ID] [int] IDENTITY(1,1) NOT NULL,
					[OrganizationLicenseID] int NOT NULL,
					[LicenseClauseID] [int] NOT NULL,
					[ClauseData] [nvarchar](max) NULL,
					[CreatedAt] [datetime2](7) NOT NULL,
					[CreatedBy] [int] NOT NULL,
					[UpdatedAt] [datetime2](7) NULL,
					[UpdatedBy] [int] NULL,
					CONSTRAINT [PK_OrganizationLicenseClauses] PRIMARY KEY CLUSTERED ([ID]),
					CONSTRAINT [FK_OrganizationLicenseClauses_OrganizationLicenseID] FOREIGN KEY ([OrganizationLicenseID]) REFERENCES [dbo].[OrganizationLicenses] ([ID]),
					CONSTRAINT [FK_OrganizationLicenseClauses_LicenseClauseID] FOREIGN KEY ([LicenseClauseID]) REFERENCES [dbo].[LicenseClauses] ([ID]),
					CONSTRAINT [FK_OrganizationLicenseClauses_SentBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([ID]),
					CONSTRAINT [FK_OrganizationLicenseClauses_SentTo] FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[Users] ([ID])
				);
			END
		
		INSERT INTO [dbo].[UpdateHistory] ([Name],[Date]) VALUES (@issueNumber, GETDATE())		
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		SELECT
				ERROR_NUMBER() as ErrorNumber,
				ERROR_MESSAGE() as ErrorMessage
	END CATCH
END
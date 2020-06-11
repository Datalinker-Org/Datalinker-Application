DECLARE @issueNumber VARCHAR(50) = 'DNZDL-215237'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LicenseAgreements')
			BEGIN
				CREATE TABLE [dbo].[LicenseAgreements](
					[ID] [int] IDENTITY(1,1) NOT NULL,
					[ConsumerLicenseID] int NOT NULL,
					[ProviderLicenseID] int NOT NULL,
					[ProviderOrganizationID] int NOT NULL,
					[ConsumerOrganizationID] int NOT NULL,
					[DataSchemaID] int NOT NULL,
					[SoftwareStatement] [nvarchar](max) NOT NULL,
					[CreatedAt] [datetime2](7) NOT NULL,
					[ExpiresAt] [datetime2](7) NULL
					CONSTRAINT [PK_LicenseAgreements] PRIMARY KEY CLUSTERED ([ID]),
					CONSTRAINT [FK_LicenseAgreements_ConsumerLicenseID] FOREIGN KEY ([ConsumerLicenseID]) REFERENCES [dbo].[OrganizationLicenses] ([ID]),
					CONSTRAINT [FK_LicenseAgreements_ProviderLicenseID] FOREIGN KEY ([ProviderLicenseID]) REFERENCES [dbo].[OrganizationLicenses] ([ID]),
					CONSTRAINT [FK_LicenseAgreements_ProviderOrganizationID] FOREIGN KEY ([ProviderOrganizationID]) REFERENCES [dbo].[Organizations] ([ID]),
					CONSTRAINT [FK_LicenseAgreements_ConsumerOrganizationID] FOREIGN KEY ([ConsumerOrganizationID]) REFERENCES [dbo].[Organizations] ([ID]),
					CONSTRAINT [FK_LicenseAgreements_DataSchemaID] FOREIGN KEY ([DataSchemaID]) REFERENCES [dbo].[DataSchemas] ([ID])
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
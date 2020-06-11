DECLARE @issueNumber VARCHAR(50) = '4.DNZDL-214949'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION	
							
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LicenseMatches')
			BEGIN
				CREATE TABLE [dbo].[LicenseMatches](
					[ID] [int] IDENTITY(1,1) NOT NULL,
					[ConsumerLicenseID] int NOT NULL,
					[ProviderLicenseID] int NOT NULL,
					[CreatedAt] [datetime2](7) NOT NULL,
					[CreatedBy] [int] NOT NULL
					CONSTRAINT [PK_LicenseMatches] PRIMARY KEY CLUSTERED ([ID]),
					CONSTRAINT [FK_LicenseMatches_ConsumerLicenseID] FOREIGN KEY ([ConsumerLicenseID]) REFERENCES [dbo].[OrganizationLicenses] ([ID]),
					CONSTRAINT [FK_LicenseMatches_ProviderLicenseID] FOREIGN KEY ([ProviderLicenseID]) REFERENCES [dbo].[OrganizationLicenses] ([ID]),
					CONSTRAINT [FK_LicenseMatches_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([ID])
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
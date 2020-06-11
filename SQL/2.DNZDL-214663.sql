DECLARE @issueNumber VARCHAR(50) = '2.DNZDL-214663'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION
			IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Applications' AND COLUMN_NAME = N'IsIntroducedAsIndustryod')
			BEGIN
				EXECUTE sp_rename N'dbo.Applications.[IsIntroducedAsIndustryod]', N'IsIntroducedAsIndustryGood', 'COLUMN'
			END
			
			IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Applications' AND COLUMN_NAME = N'IsVerifiedAsIndustryod')
			BEGIN
				EXECUTE sp_rename N'dbo.Applications.[IsVerifiedAsIndustryod]', N'IsVerifiedAsIndustryGood', 'COLUMN'
			END
						
			IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'DataSchemas' AND COLUMN_NAME = N'IsIndustryod')
			BEGIN
				EXECUTE sp_rename N'dbo.DataSchemas.[IsIndustryod]', N'IsIndustryGood', 'COLUMN'
			END
			
			IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'ProviderEndpoints' AND COLUMN_NAME = N'IsIndustryod')
			BEGIN
				EXECUTE sp_rename N'dbo.ProviderEndpoints.[IsIndustryod]', N'IsIndustryGood', 'COLUMN'
			END
			
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'SchemaFiles' AND COLUMN_NAME = N'FileFormat')
			BEGIN
			ALTER TABLE dbo.SchemaFiles 
				ADD
				FileFormat nvarchar(50) NOT NULL
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
DECLARE @issueNumber VARCHAR(50) = '5.DNZDL-214970'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION	
							
			IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Organizations')
			BEGIN
				ALTER Table [dbo].[Organizations]
				Alter column [AdministrativeContact] [nvarchar](255) NULL
				
				ALTER Table [dbo].[Organizations]
				Alter column [Address] [nvarchar](255) NULL
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
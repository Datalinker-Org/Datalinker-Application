DECLARE @issueNumber VARCHAR(50) = 'DNZDL-215092'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'ApplicationAuthentication' AND COLUMN_NAME = N'RegistrationEndpoint')
			BEGIN
				ALTER TABLE ApplicationAuthentication  
					ADD [RegistrationEndpoint] [nvarchar](255) NOT NULL DEFAULT('');
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
DECLARE @issueNumber VARCHAR(50) = 'DNZDL-228411'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'ProviderEndpoints' AND COLUMN_NAME = N'NeedPersonalApproval')
			BEGIN
				ALTER TABLE ProviderEndpoints  
					ADD [NeedPersonalApproval] [bit] NOT NULL DEFAULT(0);
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
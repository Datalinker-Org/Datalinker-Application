-- TODO:  fill in the Gemini issue number here
DECLARE @issueNumber VARCHAR(50) = ''

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION

		-- TODO:  Put sql script code here
		
		-- End SQL script code
		
		-- Add's this script so it won't run again on successfull transaction commit
		INSERT INTO [dbo].[UpdateHistory] ([Name],[Date]) VALUES (@issueNumber, GETDATE())		
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		SELECT
				ERROR_NUMBER() as ErrorNumber,
				ERROR_MESSAGE() as ErrorMessage
	END CATCH
END
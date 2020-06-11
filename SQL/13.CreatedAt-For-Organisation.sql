DECLARE @issueNumber VARCHAR(50) = 'CreatedAt-For-Organisation'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Organizations' AND COLUMN_NAME = N'CreatedAt')
			BEGIN
				ALTER TABLE Organizations  
					ADD [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETDATE());
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
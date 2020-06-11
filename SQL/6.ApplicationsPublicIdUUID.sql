DECLARE @issueNumber VARCHAR(50) = '6.ApplicationsPublicIdUUID'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION

		ALTER TABLE dbo.Applications
		ALTER COLUMN PublicID nvarchar(50) NULL

		UPDATE dbo.Applications 
		SET PublicID=NULL

		ALTER TABLE dbo.Applications
		DROP COLUMN PublicID

		ALTER TABLE dbo.Applications
		ADD PublicID uniqueidentifier NOT NULL DEFAULT NEWID()
		
		INSERT INTO [dbo].[UpdateHistory] ([Name],[Date]) VALUES (@issueNumber, GETDATE())		
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		SELECT
				ERROR_NUMBER() as ErrorNumber,
				ERROR_MESSAGE() as ErrorMessage
	END CATCH
END
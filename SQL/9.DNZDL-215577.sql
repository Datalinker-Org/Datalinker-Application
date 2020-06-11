DECLARE @issueNumber VARCHAR(50) = 'DNZDL-215577'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SoftwareStatements')
			BEGIN
				CREATE TABLE [dbo].[SoftwareStatements](
					[ID] [int] IDENTITY(1,1) NOT NULL,
					[ApplicationID] int NOT NULL,
					[Content] [nvarchar](max) NOT NULL,
					[CreatedAt] [datetime2](7) NOT NULL,
					[CreatedBy] int NOT NULL,
					[ExpiredAt] [datetime2](7) NULL,
					[ExpiredBy] int NULL,
					CONSTRAINT [PK_SoftwareStatements] PRIMARY KEY CLUSTERED ([ID]),
					CONSTRAINT [FK_SoftwareStatements_ApplicationID] FOREIGN KEY ([ApplicationID]) REFERENCES [dbo].[Applications] ([ID]),
					CONSTRAINT [FK_SoftwareStatements_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([ID]),
					CONSTRAINT [FK_SoftwareStatements_ExpiredBy] FOREIGN KEY ([ExpiredBy]) REFERENCES [dbo].[Users] ([ID])
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
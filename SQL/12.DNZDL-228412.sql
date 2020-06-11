DECLARE @issueNumber VARCHAR(50) = 'DNZDL-228412'

IF NOT EXISTS (SELECT 1 FROM UpdateHistory WHERE Name = @issueNumber)
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		BEGIN TRANSACTION
			IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ConsumerRequests')
			BEGIN
				CREATE TABLE [dbo].[ConsumerRequests](
					[ID] [int] IDENTITY(1,1) NOT NULL,
					[ProviderID] int NOT NULL,
					[ConsumerID] int NOT NULL,
					[DataSchemaID] int NOT NULL,
					[Status] int NOT NULL,
					[CreatedAt] [datetime2](7) NOT NULL,
					[ProcessedAt] [datetime2](7) NULL,
					[ProcessedBy] int NULL
					CONSTRAINT [PK_ConsumerRequests] PRIMARY KEY CLUSTERED ([ID]),
					CONSTRAINT [FK_ConsumerRequests_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [dbo].[Applications] ([ID]),
					CONSTRAINT [FK_ConsumerRequests_ConsumerID] FOREIGN KEY ([ConsumerID]) REFERENCES [dbo].[Applications] ([ID]),
					CONSTRAINT [FK_ConsumerRequests_ProcessedBy] FOREIGN KEY ([ProcessedBy]) REFERENCES [dbo].[Users] ([ID]),
					CONSTRAINT [FK_ConsumerRequests_DataSchemaID] FOREIGN KEY ([DataSchemaID]) REFERENCES [dbo].[DataSchemas] ([ID])
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
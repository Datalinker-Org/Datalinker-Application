-- Ensure the version number tracking table exists
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'VersionInformation')
BEGIN
	CREATE TABLE [dbo].[VersionInformation] (
		[VersionNumber] int NOT NULL,
		[UpgradedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
		CONSTRAINT [PK_VersionInformation] PRIMARY KEY CLUSTERED ([VersionNumber])
	);
END;

-- If there is no version information then this is the initial empty installation
IF NOT EXISTS (SELECT 1 FROM [dbo].[VersionInformation])
BEGIN
	CREATE TABLE [dbo].[AuditLogMessages] (
		[AuditStream] nvarchar(40) NOT NULL,
		[Sequence] bigint NOT NULL,
		[CreatedAt] datetime2 NOT NULL,
		[Action] nvarchar(max) NOT NULL,
		[UserDetails] nvarchar(max) NOT NULL,
		[DataPayload] nvarchar(max) NOT NULL,
		[Signature] nvarchar(max) NOT NULL
	);

	CREATE CLUSTERED INDEX [IX_AuditLogMessages_CreatedAt]
		ON [dbo].[AuditLogMessages] ([CreatedAt] ASC);

	ALTER TABLE [dbo].[AuditLogMessages]
		ADD CONSTRAINT [PK_AuditLogMessages] PRIMARY KEY NONCLUSTERED ([AuditStream], [Sequence]);

	INSERT INTO [dbo].[VersionInformation] ([VersionNumber]) VALUES (1);
END;

/* Next update:
IF (1 = (SELECT MAX([VersionNumber]) FROM [dbo].[VersionInformation]))
BEGIN
	-- Update statements
	INSERT INTO [dbo].[VersionInformation] ([VersionNumber]) VALUES (2);
END;
*/
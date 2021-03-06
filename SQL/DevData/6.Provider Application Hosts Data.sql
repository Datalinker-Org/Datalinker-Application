IF EXISTS (SELECT 1 FROM Organizations WHERE Name = 'Provider Org') AND EXISTS (SELECT 1 FROM Users WHERE Email = 'provrep@provorg.co.nz')
BEGIN

	DECLARE @provOrg int, @provApp1Id int, @provApp2Id int, @provApp3Id int, @provRep int;
	
	/** Select Ids from provider applications **/
	SELECT @provApp1Id = id FROM Applications WHERE Name = 'Provider App 1';
	SELECT @provApp2Id = id FROM Applications WHERE Name = 'Provider App 2';
	SELECT @provApp3Id = id FROM Applications WHERE Name = 'Provider App 3';
	
	SELECT @provRep = id FROM Users WHERE Email = 'provrep@provorg.co.nz';
	/** Add host data **/
	MERGE ApplicationTokens AS target
	USING( VALUES
		(@provApp1Id, N'providerApp.com', N'O9vZtXu1nUic6LkD6SZHGA==', CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), @provRep),
		(@provApp2Id, N'providerApp.com', N'O9vZtXu1nUic6LkD6SZHGA==', CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), @provRep),
		(@provApp3Id, N'providerApp.com', N'O9vZtXu1nUic6LkD6SZHGA==', CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), @provRep)
	) AS source (ApplicationID, OriginHost, Token, CreatedAt, CreatedBy)
	ON target.ApplicationID = source.ApplicationID
	WHEN NOT MATCHED THEN
		INSERT (ApplicationID, OriginHost, Token, CreatedAt, CreatedBy)
		VALUES (source.ApplicationID, source.OriginHost, source.Token, source.CreatedAt, source.CreatedBy)
	WHEN MATCHED THEN
		UPDATE SET
		ApplicationID = source.ApplicationID,
		OriginHost = source.OriginHost,
		Token = source.Token,
		CreatedAt =	source.CreatedAt,
		CreatedBy = source.CreatedBy;
END
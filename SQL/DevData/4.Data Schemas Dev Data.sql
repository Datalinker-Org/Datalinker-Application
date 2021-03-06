/** Add schema data **/
MERGE DataSchemas AS target
USING( VALUES
	(N'pubID1', N'Default schema v1.0', N'blah blah blah', CAST(N'2015-12-23 10:23:54.0000000' AS DateTime2), 1, CAST(N'2015-12-23 10:23:55.0000000' AS DateTime2), 1, 1, CAST(N'2015-12-23 10:24:40.0000000' AS DateTime2), 1, NULL, NULL),
	(N'pubID2', N'Custom schema v1.2', N'blah blah blah', CAST(N'2015-12-23 10:24:16.0000000' AS DateTime2), 1, CAST(N'2015-12-23 10:24:19.0000000' AS DateTime2), 1, 1, NULL, NULL, CAST(N'2015-12-23 10:24:49.0000000' AS DateTime2), 1),
	(N'pubID3', N'New schema v3.0', N'blah blah blah', CAST(N'2015-12-23 10:24:33.0000000' AS DateTime2), 1, CAST(N'2015-12-23 10:24:33.0000000' AS DateTime2), 1, 1, NULL, NULL, NULL, NULL)
) AS source (PublicID, Name, Description, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Version, PublishedAt, PublishedBy, RetractedAt, RetractedBy)
ON target.Name = source.Name
WHEN NOT MATCHED THEN
	INSERT (PublicID, Name, Description, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Version, PublishedAt, PublishedBy, RetractedAt, RetractedBy)
	VALUES (source.PublicID, source.Name, source.Description, source.CreatedAt, source.CreatedBy, source.UpdatedAt, source.UpdatedBy, source.Version, source.PublishedAt, source.PublishedBy, source.RetractedAt, source.RetractedBy)
WHEN MATCHED THEN
	UPDATE SET
	PublicID = source.PublicID,
	Name = source.Name,
	Description = source.Description,
	CreatedAt =	source.CreatedAt,
	CreatedBy = source.CreatedBy,
	UpdatedAt = source.UpdatedAt,
	UpdatedBy = source.UpdatedBy,
	Version = source.Version,
	PublishedAt = source.PublishedAt,
	PublishedBy = source.PublishedBy, 
	RetractedAt = source.RetractedAt,
	RetractedBy = source.RetractedBy;
/** Add schema data **/
MERGE SchemaFiles AS target
USING( VALUES
	(1, N'blah blah blah', 1, CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), 1),
	(2, N'blah blah', 1, CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), 2),
	(3, N'blah blah blah', 1, CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), 3)
) AS source (DataSchemaID, SchemaText, IsCurrent, CreatedAt, CreatedBy)
ON target.DataSchemaID = source.DataSchemaID
WHEN NOT MATCHED THEN
	INSERT (DataSchemaID, SchemaText, IsCurrent, CreatedAt, CreatedBy)
	VALUES (source.DataSchemaID, source.SchemaText, source.IsCurrent, source.CreatedAt, source.CreatedBy)
WHEN MATCHED THEN
	UPDATE SET
	DataSchemaID = source.DataSchemaID,
	SchemaText = source.SchemaText,
	IsCurrent = source.IsCurrent,
	CreatedAt =	source.CreatedAt,
	CreatedBy = source.CreatedBy;
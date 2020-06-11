/** Add organisations **/
MERGE Organizations AS target
USING ( VALUES
    ('Provider Org'),
    ('Consumer Org')
) AS source (Name)
ON target.Name = source.Name
WHEN NOT MATCHED THEN
	INSERT (Name)
	VALUES (source.Name)
WHEN MATCHED THEN
	UPDATE SET
		Name = source.Name;

DECLARE @provOrg int, @consOrg int;
SELECT @provOrg = id FROM Organizations WHERE Name = 'Provider Org';
SELECT @consOrg = id FROM Organizations WHERE Name = 'Consumer Org';

/** Add user accounts **/
MERGE Users AS target
USING ( VALUES
    ('provrep@provorg.co.nz', 'provrep', 0, @provOrg, 0, 1),
    ('provlegal@provorg.co.nz', 'provlegal', 1, @provOrg, 0, 1),
    ('consrep@consorg.co.nz', 'consrep', 0, @consOrg, 0, 1),
    ('conslegal@consorg.co.nz', 'conslegal', 1, @consOrg, 0, 1)
) AS source (Email, Username, IsIntroducedAsLegalOfficer, OrganizationID, IsSysAdmin, IsActive)
ON target.Email = source.Email
WHEN NOT MATCHED THEN
	INSERT (Email, Username, IsIntroducedAsLegalOfficer, OrganizationID, IsSysAdmin, IsActive)
	VALUES (source.Email, source.Username, source.IsIntroducedAsLegalOfficer, source.OrganizationID, source.IsSysAdmin, source.IsActive)
WHEN MATCHED THEN
	UPDATE SET
		Email = source.Email,
		Username = source.Username,
		IsIntroducedAsLegalOfficer = source.IsIntroducedAsLegalOfficer,
		OrganizationID = source.OrganizationID,
		IsSysAdmin = source.IsSysAdmin,
		IsActive = source.IsActive;

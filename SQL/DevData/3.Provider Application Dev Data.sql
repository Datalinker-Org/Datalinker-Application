IF EXISTS (SELECT 1 FROM Organizations WHERE Name = 'Provider Org') AND EXISTS (SELECT 1 FROM Users WHERE Email = 'provrep@provorg.co.nz')
BEGIN

	DECLARE @provOrg int, @provRep int;
	SELECT @provOrg = id FROM Organizations WHERE Name = 'Provider Org';
	SELECT @provRep = id FROM Users WHERE Email = 'provrep@provorg.co.nz';

	/** Add license templates **/
	MERGE Applications AS target
	USING ( VALUES
	    (@provOrg,'Provider App 1','The first provider service.','pa1',1,'2015-12-18 02:27:31.3000000',@provRep,NULL,NULL,1,0),
	    (@provOrg,'Provider App 2','The second and more popular provider service.','pa2',1,'2015-12-18 03:27:31.3000000',@provRep,NULL,NULL,1,0),
	    (@provOrg,'Provider App 3','The third provider service, used mostly by universities and libraries.','pa3',1,'2015-12-18 04:27:31.3000000',@provRep,'2015-12-18 04:37:02.3000000',@provRep,0,0)
	) AS source (OrganizationID,Name,Description,PublicID,IsProvider,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,IsActive,IsIntroducedAsIndustryGood)
	ON target.Name = source.Name
	WHEN NOT MATCHED THEN
		INSERT (OrganizationID,Name,Description,PublicID,IsProvider,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,IsActive,IsIntroducedAsIndustryGood)
		VALUES (source.OrganizationID,source.Name,source.Description,source.PublicID,source.IsProvider,source.CreatedAt,source.CreatedBy,source.UpdatedAt,source.UpdatedBy,source.IsActive,source.IsIntroducedAsIndustryGood)
	WHEN MATCHED THEN
		UPDATE SET
			OrganizationID = source.OrganizationID,
			Name = source.Name,
			Description = source.Description,
			PublicID = source.PublicID,
			IsProvider = source.IsProvider,
			CreatedAt = source.CreatedAt,
			CreatedBy = source.CreatedBy,
			UpdatedAt = source.UpdatedAt,
			IsActive = source.IsActive,
			IsIntroducedAsIndustryGood = source.IsIntroducedAsIndustryGood,
			UpdatedBy = source.UpdatedBy;

END

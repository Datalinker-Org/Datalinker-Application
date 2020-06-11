USE Development;
DECLARE @Cursor CURSOR;
DECLARE @LegalText nvarchar(max);
DECLARE @LicenseClauseID int;
BEGIN
    SET @Cursor = CURSOR FOR
    select LegalText, LicenseClauseID from [dbo].[LicenseClauseTemplates] where [Status] = 3;

    OPEN @Cursor 
    FETCH NEXT FROM @Cursor 
    INTO @LegalText, @LicenseClauseID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
		update [dbo].[OrganizationLicenseClauses] set ClauseData = @LegalText where LicenseClauseId = @LicenseClauseId
	  
		FETCH NEXT FROM @Cursor 
		INTO @LegalText, @LicenseClauseID;
    END; 

    CLOSE @Cursor;
    DEALLOCATE @Cursor;
END;
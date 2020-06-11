USE DataLinkerIDS
DECLARE @EmailsCursor CURSOR;
DECLARE @Email varchar(100);
BEGIN
    SET @EmailsCursor = CURSOR FOR
    select Email from [DataLinkerIDS].[dbo].[AspNetUsers]

    OPEN @EmailsCursor 
    FETCH NEXT FROM @EmailsCursor 
    INTO @Email

    WHILE @@FETCH_STATUS = 0
    BEGIN

	  update [DataLinkerIDS].[dbo].[AspNetUsers] set Username = @Email where Email = @Email

      FETCH NEXT FROM @EmailsCursor 
      INTO @Email 
    END; 

    CLOSE @EmailsCursor;
    DEALLOCATE @EmailsCursor;
END;
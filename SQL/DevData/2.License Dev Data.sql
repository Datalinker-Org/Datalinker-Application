/** Add global licenses **/
MERGE Licenses AS target
USING( VALUES
(CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), 1)
) AS source (CreatedAt, CreatedBy)
ON target.CreatedBy = source.CreatedBy
WHEN NOT MATCHED THEN
	INSERT (CreatedAt, CreatedBy)
	VALUES (source.CreatedAt, source.CreatedBy)
WHEN MATCHED THEN
	UPDATE SET
	CreatedAt =	source.CreatedAt,
	CreatedBy = source.CreatedBy;
	
/** Add license templates **/
DECLARE @licenseText nvarchar(max) = '<html>
	<body>
		<h1>Global license</h1>
		<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec quam 
			urna, hendrerit eget facilisis tempor, sodales bibendum nulla. Class
			aptent taciti sociosqu ad litora torquent per conubia nostra, per
			inceptos himenaeos. Integer porta auctor tellus, vel facilisis quam
			porta ornare. Vivamus convallis nisl eget lectus laoreet posuere.
			Integer non augue pretium, fringilla est in, rutrum dui. Nulla vitae
			enim lobortis risus vehicula consequat. Aliquam odio massa, molestie
			vitae ornare et, dignissim accumsan orci. Vestibulum facilisis orci
			vel nisl porta lobortis. Interdum et malesuada fames ac ante ipsum
			primis in faucibus. Praesent placerat, elit non gravida tincidunt,
			turpis orci malesuada arcu, vel vehicula sem sapien eu ipsum.
			Pellentesque quis felis tortor. Aliquam in massa ut urna accumsan
			varius. Morbi blandit neque eu magna convallis, commodo tristique
			lectus dictum.</p>
		<h2>Some common section</h2>
		<p>Duis malesuada leo vitae pretium vehicula. Nunc eu nisl ultrices mauris
			posuere varius. Sed erat justo, interdum maximus nibh et, sagittis
			malesuada augue. Nunc odio lacus, volutpat eget venenatis id, ultricies
			id justo. Sed iaculis metus in turpis dignissim lobortis. Donec
			venenatis turpis tempus mauris vehicula, in maximus orci molestie.
			Nullam vestibulum erat laoreet, ullamcorper nunc in, tempus nisi. Proin
			at luctus ligula. Suspendisse vestibulum lacus nec dolor malesuada
			porta. Nulla aliquet finibus ipsum, nec suscipit urna ullamcorper
			vitae. Nam diam nunc, convallis eget vulputate non, malesuada nec nunc.
			Sed a sem dapibus, consequat elit eget, interdum sem.</p>
		<section title="Attribution"></section>
		<h2>Another common section</h2>
		<p>Morbi metus purus, ultricies et augue feugiat, feugiat feugiat ipsum.
			Praesent et lacinia dolor, porttitor finibus elit. Nullam vitae
			pulvinar justo. Praesent eu leo finibus, pulvinar massa a, ornare
			mauris. Integer scelerisque lectus eget lectus interdum, eget placerat
			ligula lobortis. Praesent et mi vestibulum, sodales justo et, dignissim
			libero. Praesent rhoncus quam erat. Aliquam odio dui, facilisis
			elementum lectus quis, finibus pulvinar sapien. Pellentesque habitant
			morbi tristique senectus et netus et malesuada fames ac turpis egestas.
			Pellentesque facilisis semper gravida. Mauris mattis dictum sem, eget
			consectetur nunc dapibus et. Ut mollis nibh metus, in scelerisque nisl
			accumsan ac. Duis ac sollicitudin mauris.</p>
		<section title="Payment"></section>
		<h2>A third common section</h2>
		<p>Pellentesque semper leo non consequat elementum. Sed ut semper dolor.
			Nullam a sem pretium, bibendum felis ac, blandit urna. Duis quis orci
			vitae neque feugiat iaculis. Maecenas tempus odio quam, ut ultrices
			sapien aliquam vel. Suspendisse facilisis nisl ut magna iaculis, quis
			pellentesque nulla mattis. Cum sociis natoque penatibus et magnis dis
			parturient montes, nascetur ridiculus mus. Nunc felis tellus, posuere
			et consectetur eu, semper sit amet nisl. Ut mattis enim at justo
			faucibus, ut faucibus magna vestibulum. Phasellus varius erat ante,
			vel mollis velit convallis quis. Quisque risus nisi, hendrerit eu
			libero vel, ornare consequat mi. Curabitur aliquam nisi et tincidunt
			consequat.</p>
		<p>Vivamus varius facilisis nunc, tincidunt vehicula tortor finibus in. In
			consectetur et nulla eget tincidunt. Morbi sollicitudin ex quis
			tristique pharetra. Ut gravida placerat ultrices. Suspendisse potenti.
			Donec porta sapien fermentum pellentesque cursus. Donec lacinia in
			massa luctus sollicitudin.</p>
		<section title="Disclosure"></section>
	</body>
</html>';
MERGE LicenseTemplates AS target
USING ( VALUES
    ('Uber License v1.0','An awesome license.','2015-12-16 03:27:31.3000000',1,'2015-12-16 03:27:31.3000000',1,1,1,@licenseText,1),
	('Another v2.1','An aweful license that didn''t last long.','2015-12-16 02:27:31.3000000',1,'2015-12-16 03:20:31.3000000',1,1,1,@licenseText,0),
	('Uber License v1.1','An awesome license with some tweaks.','2015-12-16 03:27:31.3000000',1,'2015-12-16 03:27:31.3000000',1,1,1,@licenseText,2)
) AS source (Name,Description,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,Version,LicenseID,LicenseText,Status)
ON target.Name = source.Name
WHEN NOT MATCHED THEN
	INSERT (Name,Description,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,Version,LicenseID,LicenseText,Status)
	VALUES (source.Name,source.Description,source.CreatedAt,source.CreatedBy,source.UpdatedAt,source.UpdatedBy,source.Version,source.LicenseID,source.LicenseText,source.Status)
WHEN MATCHED THEN
	UPDATE SET
		Name = source.Name,
		Description = source.Description,
		CreatedAt = source.CreatedAt,
		CreatedBy = source.CreatedBy,
		UpdatedAt = source.UpdatedAt,
		UpdatedBy = source.UpdatedBy,
		Version = source.Version,
		Status = source.Status,
		LicenseText = source.LicenseText,
		LicenseID = source.LicenseID;

/** Add license sections for global template **/
DECLARE @provRep int, @Email nvarchar(max) = 'provrep@provorg.co.nz';

IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
BEGIN
	SELECT @provRep = id FROM Users WHERE Email = @Email;
	MERGE LicenseSections AS target
	USING( VALUES
		(N'Attribution', CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), @provRep),
		(N'Payment', CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), @provRep),
		(N'Disclosure', CAST(N'2015-12-23 00:00:00.0000000' AS DateTime2), @provRep)
	) AS source (Title, CreatedAt, CreatedBy)
	ON target.Title = source.Title
	WHEN NOT MATCHED THEN
		INSERT (Title, CreatedAt, CreatedBy)
		VALUES (source.Title, source.CreatedAt, source.CreatedBy)
	WHEN MATCHED THEN
		UPDATE SET
		Title = source.Title,
		CreatedAt =	source.CreatedAt,
		CreatedBy = source.CreatedBy;
END
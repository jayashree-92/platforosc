IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsSwiftGroupStatusLkp')
BEGIN
	CREATE TABLE [dbo].[hmsSwiftGroupStatusLkp]
	(
		[hmsSwiftGroupStatusLkpId] INT NOT NULL IDENTITY(1,1),
		[Status] VARCHAR(30),

		CONSTRAINT hmsSwiftGroupStatusLkp_PK PRIMARY KEY NONCLUSTERED (hmsSwiftGroupStatusLkpId)
	);
	
	INSERT INTO hmsSwiftGroupStatusLkp ([Status]) VALUES ('Requested');
	INSERT INTO hmsSwiftGroupStatusLkp ([Status]) VALUES ('Testing');
	INSERT INTO hmsSwiftGroupStatusLkp ([Status]) VALUES ('Live');
	
END
GO
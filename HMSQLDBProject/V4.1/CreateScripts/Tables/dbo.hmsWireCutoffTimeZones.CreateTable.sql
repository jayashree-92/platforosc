IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireCutoffTimeZones')
BEGIN
	CREATE TABLE [dbo].[hmsWireCutoffTimeZones]
	(
		[hmsWireCutoffTimeZoneId] BIGINT NOT NULL IDENTITY(1,1),
		[TimeZone] VARCHAR(20) NOT NULL,
		[TimeZoneStandardName] VARCHAR(200) NOT NULL,
		CONSTRAINT hmsWireCutoffTimeZones_PK PRIMARY KEY NONCLUSTERED ([hmsWireCutoffTimeZoneId])
	);
	
END
GO

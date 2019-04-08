IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsNotificationStaging')
BEGIN
	CREATE TABLE [dbo].[hmsNotificationStaging]
	(
		hmsNotificationStagingId BIGINT NOT NULL IDENTITY(1,1),
		[FromUserId] INT NOT NULL,
		[ToUserId] INT NOT NULL,
		[Title] VARCHAR(100) NOT NULL,	
		[Message] VARCHAR(2000) NOT NULL,	
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		CONSTRAINT hmsNotificationStaging_PK PRIMARY KEY NONCLUSTERED (hmsNotificationStagingId)
	)	
END
GO
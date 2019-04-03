IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsUserAuditLog')
BEGIN
	CREATE TABLE [dbo].[hmsUserAuditLog]
	(
		hmsUserAuditLogId BIGINT NOT NULL IDENTITY(1,1),	
		[Module] VARCHAR(100),	
		[Action] VARCHAR(100), 
		[UserName] VARCHAR(100),
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[Log] VARCHAR(8000),
		[AssociationId] BIGINT,
		CONSTRAINT hmsUserAuditLog_PK PRIMARY KEY NONCLUSTERED (hmsUserAuditLogId)
	)	
END
GO
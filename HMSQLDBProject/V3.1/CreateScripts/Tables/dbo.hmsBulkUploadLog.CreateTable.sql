IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsBulkUploadLog')
BEGIN
	CREATE TABLE [dbo].[hmsBulkUploadLog]
	(
		[hmsBulkUploadLogId] BIGINT NOT NULL IDENTITY(1,1),	
		[FileName] VARCHAR(200) NOT NULL,
		[IsFundAccountLog] BIT NOT NULL,
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[UserName] VARCHAR(100) NOT NULL,
		CONSTRAINT hmsBulkUploadLog_PK PRIMARY KEY NONCLUSTERED ([hmsBulkUploadLogId])
	)	
END
GO


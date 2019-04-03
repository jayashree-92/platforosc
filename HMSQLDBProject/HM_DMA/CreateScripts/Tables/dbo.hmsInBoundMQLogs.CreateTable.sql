IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsInBoundMQLogs')
BEGIN
	CREATE TABLE [dbo].[hmsInBoundMQLogs]
	(
		[hmsInBoundMQLogsId] BIGINT NOT NULL IDENTITY(1,1),
		[InBoundMessage] VARCHAR(8000) NOT NULL,
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE())
	)	

END
GO 
IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsMQLogs')
BEGIN
	CREATE TABLE [dbo].[hmsMQLogs]
	(
		[hmsMQLogsId] BIGINT NOT NULL IDENTITY(1,1),
		[IsOutBound] BIT NOT NULL,
		[QueueManager] VARCHAR(50) NOT NULL,
		[QueueName] VARCHAR(50) NOT NULL,
		[Message] VARCHAR(8000) NOT NULL,
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		CONSTRAINT hmsMQLogs_PK PRIMARY KEY NONCLUSTERED (hmsMQLogsId)
	)
END
GO 

IF EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsInBoundMQLogs')
BEGIN

INSERT INTO [hmsMQLogs] ([IsOutBound],[QueueManager],[QueueName],[Message],[CreatedAt])
SELECT 0,'LQAL','DMO.EMX.EMX2DMO.ACK.U1.F',InBoundMessage,CreatedAt from hmsInBoundMQLogs;

DROP TABLE hmsInBoundMQLogs;

END
IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireJobSchedules')
BEGIN
	CREATE TABLE [dbo].[hmsWireJobSchedules]
	(
		[hmsWireJobSchedulerId] BIGINT NOT NULL IDENTITY(1,1),	
		[hmsWireId] BIGINT NOT NULL,
		[ScheduledDate] DATETIME NOT NULL,
		[IsJobCreated] BIT NOT NULL,
		[IsDeleted] BIT NOT NULL,
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[CreatedBy] INT NOT NULL,
		[UpdatedBy] INT NOT NULL,
		[LastModifiedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		CONSTRAINT hmsWireJobScheduler_PK PRIMARY KEY NONCLUSTERED (hmsWireJobSchedulerId),
		CONSTRAINT FK_hmsWireJobScheduler_hmsWires_hmsWireId FOREIGN KEY ([hmsWireId]) REFERENCES hmsWires(hmsWireId)
)	
END
GO 



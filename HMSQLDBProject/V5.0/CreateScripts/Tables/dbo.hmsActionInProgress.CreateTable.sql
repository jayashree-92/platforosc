IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsActionInProgress')
BEGIN

	CREATE TABLE [dbo].[hmsActionInProgress](
		[hmsActionInProgressId] [int] IDENTITY(1,1) NOT NULL,
		[hmsWireId] BIGINT NOT NULL,
		[UserName] VARCHAR(200) NOT NULL,
		[RecCreatedDt] DATETIME NOT NULL DEFAULT(GETDATE()),

	PRIMARY KEY CLUSTERED 
	(
		[hmsActionInProgressId] ASC
	) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[hmsActionInProgress]  WITH CHECK ADD  CONSTRAINT [FK_hmsActionInProgress_hmsWireId] FOREIGN KEY([hmsWireId])
	REFERENCES [dbo].[hmsWires] ([hmsWireId])

	ALTER TABLE [dbo].[hmsActionInProgress] WITH CHECK ADD CONSTRAINT  [UQ01_hmsActionInProgress_hmsWireId_UserName] UNIQUE (hmsWireId,UserName) 

END
GO 
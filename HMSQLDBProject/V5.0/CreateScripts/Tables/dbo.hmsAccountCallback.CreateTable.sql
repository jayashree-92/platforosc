IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsAccountCallback')
BEGIN
	CREATE TABLE [dbo].[hmsAccountCallback]
	(
		[hmsAccountCallbackId] BIGINT NOT NULL IDENTITY(1,1),
		[onBoardingAccountId] BIGINT NOT NULL,
		[ContactName] VARCHAR(100) NOT NULL,
		[ContactNumber] VARCHAR(30) NULL,
		[Title] VARCHAR(200) NULL, 
		[IsCallbackConfirmed] BIT NOT NULL,
		[RecCreatedBy] VARCHAR(50) NOT NULL,
		[RecCreatedDt] DATETIME NOT NULL,
		[ConfirmedBy] VARCHAR(50) NULL,
		[ConfirmedAt] DATETIME NULL,

		CONSTRAINT [hmsAccountCallback_PK] PRIMARY KEY NONCLUSTERED ([hmsAccountCallbackId]),
	    CONSTRAINT [FK_hmsAccountCallback_onBoardingAccountId] FOREIGN KEY([onBoardingAccountId])
		REFERENCES [dbo].[onBoardingAccount] ([onBoardingAccountId])
	);
	
END
GO

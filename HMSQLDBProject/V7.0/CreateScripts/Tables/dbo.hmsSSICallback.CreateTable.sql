IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsSSICallback')
BEGIN
	CREATE TABLE [dbo].[hmsSSICallback]
	(
		[hmsSSICallbackId] BIGINT NOT NULL IDENTITY(1,1),
		[onBoardingSSITemplateId] BIGINT NOT NULL,
		[ContactName] VARCHAR(100) NOT NULL,
		[ContactNumber] VARCHAR(30) NULL,
		[Title] VARCHAR(200) NULL, 
		[IsCallbackConfirmed] BIT NOT NULL,
		[RecCreatedBy] VARCHAR(50) NOT NULL,
		[RecCreatedDt] DATETIME NOT NULL,
		[ConfirmedBy] VARCHAR(50) NULL,
		[ConfirmedAt] DATETIME NULL,

		CONSTRAINT [hmsSSICallback_PK] PRIMARY KEY NONCLUSTERED ([hmsSSICallbackId]),
	    CONSTRAINT [FK_hmsSSICallback_onBoardingSSITemplateId] FOREIGN KEY([onBoardingSSITemplateId])
		REFERENCES [dbo].[onBoardingSSITemplate] ([onBoardingSSITemplateId])
	);
	
END
GO
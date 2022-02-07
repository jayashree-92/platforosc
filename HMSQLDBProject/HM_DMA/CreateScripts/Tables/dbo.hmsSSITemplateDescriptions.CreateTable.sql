IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsSSIDescriptions')
BEGIN
	CREATE TABLE [dbo].[hmsSSIDescriptions](
	[hmsSSIDescriptionId] INT IDENTITY(1,1) NOT NULL,
	[Description] VARCHAR(100) NOT NULL,	
	[RecCreatedBy] VARCHAR(100) NOT NULL,
	[RecCreatedDt] DATETIME NOT NULL DEFAULT(GETDATE())

 PRIMARY KEY  CLUSTERED  
(
	[hmsSSIDescriptionId] ASC
)
) ON [PRIMARY]

END
GO 

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'hmsSSIDescriptionId' AND TABLE_NAME = 'onBoardingSSITemplate')
BEGIN
	ALTER TABLE onBoardingSSITemplate ADD hmsSSIDescriptionId INT NULL;
		
	ALTER TABLE [dbo].[onBoardingSSITemplate]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingSSITemplate_hmsSSIDescriptionId] 
	FOREIGN KEY([hmsSSIDescriptionId]) REFERENCES [dbo].[hmsSSIDescriptions] ([hmsSSIDescriptionId])
END
GO



IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsSSIPaymentReasonForDescription')
BEGIN
	CREATE TABLE [dbo].[hmsSSIPaymentReasonForDescription](
	[hmsSSIPaymentReasonForDescriptionId] INT IDENTITY(1,1) NOT NULL,
	[hmsSSIDescriptionId] INT NOT NULL,
	[PaymentReason] VARCHAR(100) NOT NULL,	
	[RecCreatedBy] VARCHAR(100) NOT NULL,
	[RecCreatedDt] DATETIME NOT NULL DEFAULT(GETDATE())

 PRIMARY KEY  CLUSTERED  
(
	[hmsSSIPaymentReasonForDescriptionId] ASC
)

) ON [PRIMARY]

	ALTER TABLE [dbo].[hmsSSIPaymentReasonForDescription]  WITH CHECK ADD  CONSTRAINT [FK_hmsSSIPaymentReasonForDescription_hmsSSIDescriptionId] 
	FOREIGN KEY([hmsSSIDescriptionId]) REFERENCES [dbo].[hmsSSIDescriptions] ([hmsSSIDescriptionId])

END
GO 
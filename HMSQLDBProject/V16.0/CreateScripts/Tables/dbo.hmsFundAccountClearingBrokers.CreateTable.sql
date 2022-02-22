
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME= 'hmsFundAccountClearingBrokers')
BEGIN
	CREATE TABLE [dbo].[hmsFundAccountClearingBrokers](
	[hmsFundAccountClearingBrokerId] [BIGINT] IDENTITY(1,1) NOT NULL,
	[onBoardingAccountId] BIGINT NOT NULL,
	[ClearingBrokerName] VARCHAR(300) NOT NULL,
	[RecCreatedAt] [datetime] NOT NULL DEFAULT (getdate()),
	[RecCreatedById] [int] NOT NULL,
	
	PRIMARY KEY CLUSTERED 
	(
		[hmsFundAccountClearingBrokerId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	
	ALTER TABLE [dbo].[hmsFundAccountClearingBrokers]  WITH CHECK ADD  CONSTRAINT [FK_hmsFundAccountClearingBrokers_onBoardingAccountId] 
	FOREIGN KEY([onBoardingAccountId]) REFERENCES [dbo].[onBoardingAccount] ([onBoardingAccountId])
	
END
GO

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'MarginExposureType' AND TABLE_NAME = 'hmsFundAccountClearingBrokers')
BEGIN
	ALTER TABLE hmsFundAccountClearingBrokers DROP COLUMN MarginExposureType;
END
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'MarginExposureTypeID' AND TABLE_NAME = 'onboardingAccount')
BEGIN
	ALTER TABLE onboardingAccount ADD MarginExposureTypeID INT NOT NULL DEFAULT(0);
END
GO
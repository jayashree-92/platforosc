IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='onboardingAccountModule')
BEGIN
	CREATE TABLE [dbo].[onboardingAccountModule]
	(
		[onboardingAccountModuleId] BIGINT NOT NULL IDENTITY(1,1),
		[Module] VARCHAR(100) NOT NULL,
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[CreatedBy] VARCHAR(100) NOT NULL,
		CONSTRAINT onboardingAccountModule_PK PRIMARY KEY NONCLUSTERED (onboardingAccountModuleId)
	)
END
GO 

IF NOT EXISTS(SELECT 8 FROM onboardingAccountModule)
BEGIN

INSERT INTO [onboardingAccountModule] ([Module],[CreatedAt],[CreatedBy]) VALUES('Collateral', GETDATE(), 'Retrofit')
INSERT INTO [onboardingAccountModule] ([Module],[CreatedAt],[CreatedBy]) VALUES('Invoice', GETDATE(), 'Retrofit')
INSERT INTO [onboardingAccountModule] ([Module],[CreatedAt],[CreatedBy]) VALUES('Expenses', GETDATE(), 'Retrofit')
INSERT INTO [onboardingAccountModule] ([Module],[CreatedAt],[CreatedBy]) VALUES('Margin', GETDATE(), 'Retrofit')
INSERT INTO [onboardingAccountModule] ([Module],[CreatedAt],[CreatedBy]) VALUES('Cash', GETDATE(), 'Retrofit')
INSERT INTO [onboardingAccountModule] ([Module],[CreatedAt],[CreatedBy]) VALUES('Repo Collateral', GETDATE(), 'Retrofit')
INSERT INTO [onboardingAccountModule] ([Module],[CreatedAt],[CreatedBy]) VALUES('Interest Rate', GETDATE(), 'Retrofit')

END
GO

IF EXISTS(SELECT * FROM sys.objects WHERE TYPE = 'U' AND NAME ='onboardingAccountModule') AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'Module' AND TABLE_NAME ='onboardingAccountModule')
BEGIN
DROP TABLE onboardingAccountModule
END

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='onBoardingModule')
BEGIN
	CREATE TABLE [dbo].[onBoardingModule]
	(
		[onBoardingModuleId] BIGINT NOT NULL IDENTITY(1,1),
		[dmaReportsId] BIGINT NOT NULL,
		[ModuleName] VARCHAR(100) NOT NULL,
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[CreatedBy] VARCHAR(100) NOT NULL,
		CONSTRAINT onBoardingModule_PK PRIMARY KEY NONCLUSTERED (onBoardingModuleId)
	)
END
GO

IF NOT EXISTS(SELECT 8 FROM onBoardingModule)
BEGIN

INSERT INTO [onBoardingModule] ([dmaReportsId],[ModuleName],[CreatedAt],[CreatedBy]) VALUES(4, 'Collateral', GETDATE(), 'Retrofit')
INSERT INTO [onBoardingModule] ([dmaReportsId],[ModuleName],[CreatedAt],[CreatedBy]) VALUES(11, 'Invoice', GETDATE(), 'Retrofit')
END
GO 

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='onBoardingAccountModuleAssociation')
BEGIN
	CREATE TABLE [dbo].[onBoardingAccountModuleAssociation]
	(
		[onBoardingAccountModuleAssociationId] BIGINT NOT NULL IDENTITY(1,1),
		[onBoardingAccountId] BIGINT NOT NULL,
		[onBoardingModuleId] BIGINT NOT NULL,
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[CreatedBy] VARCHAR(100) NOT NULL,
		CONSTRAINT onBoardingAccountModuleAssociation_PK PRIMARY KEY NONCLUSTERED (onBoardingAccountModuleAssociationId),
		CONSTRAINT FK_onBoardingAccountModuleAssociation_onBoardingAccount_onBoardingAccountId FOREIGN KEY ([onBoardingAccountId]) REFERENCES onBoardingAccount(onBoardingAccountId),
		CONSTRAINT FK_onBoardingAccountModuleAssociation_onBoardingModule_onBoardingModuleId FOREIGN KEY ([onBoardingModuleId]) REFERENCES onBoardingModule(onBoardingModuleId)
	)
END
GO


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


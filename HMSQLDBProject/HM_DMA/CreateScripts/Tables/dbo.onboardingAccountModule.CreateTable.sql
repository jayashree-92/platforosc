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
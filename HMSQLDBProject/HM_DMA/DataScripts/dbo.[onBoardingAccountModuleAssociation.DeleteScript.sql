
DELETE FROM [dbo].[onBoardingAccountModuleAssociation] WHERE onBoardingModuleId IN (SELECT onBoardingModuleId FROM onBoardingModule WHERE ModuleName = 'Collateral')
GO

DELETE FROM [dbo].[onBoardingModule] WHERE ModuleName = 'Collateral'
GO

DELETE FROM [dbo].[onBoardingAccountModuleAssociation] WHERE onBoardingModuleId IN (SELECT onBoardingModuleId FROM onBoardingModule WHERE ModuleName = 'Invoice')
GO

DELETE FROM [dbo].[onBoardingModule] WHERE ModuleName = 'Invoice'
GO
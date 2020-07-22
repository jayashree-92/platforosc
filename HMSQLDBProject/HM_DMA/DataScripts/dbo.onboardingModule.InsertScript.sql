
IF NOT EXISTS(SELECT * FROM onBoardingModule WHERE dmaReportsId=17)
BEGIN

INSERT INTO [onBoardingModule] ([dmaReportsId],[ModuleName],[CreatedAt],[CreatedBy]) VALUES(17, 'Repo Collateral Pledge', GETDATE(), 'system')
INSERT INTO [onBoardingModule] ([dmaReportsId],[ModuleName],[CreatedAt],[CreatedBy]) VALUES(17, 'Repo Collateral Return', GETDATE(), 'system')
INSERT INTO [onBoardingModule] ([dmaReportsId],[ModuleName],[CreatedAt],[CreatedBy]) VALUES(17, 'Repo Collateral Payment', GETDATE(), 'system')
END
GO 





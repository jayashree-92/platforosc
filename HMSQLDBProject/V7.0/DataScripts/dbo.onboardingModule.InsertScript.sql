
IF NOT EXISTS(SELECT * FROM onBoardingModule WHERE dmaReportsId=17)
BEGIN

INSERT INTO [onBoardingModule] ([dmaReportsId],[ModuleName],[CreatedAt],[CreatedBy]) VALUES(17, 'Repo Collateral Pledge', GETDATE(), 'system')
INSERT INTO [onBoardingModule] ([dmaReportsId],[ModuleName],[CreatedAt],[CreatedBy]) VALUES(17, 'Repo Collateral Return', GETDATE(), 'system')

END
GO 

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Repo Collateral' AND Purpose = 'Respond to Broker Call')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Repo Collateral', 'Respond to Broker Call',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Repo Collateral' AND Purpose = 'Send Call')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Repo Collateral', 'Send Call',GETDATE(),-1, NULL, NULL, 1)
END 
GO




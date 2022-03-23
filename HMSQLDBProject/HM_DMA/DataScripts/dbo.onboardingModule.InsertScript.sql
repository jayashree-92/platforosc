
USE HM_WIRES;

IF NOT EXISTS(SELECT * FROM onBoardingModule WHERE dmaReportsId=19)
BEGIN
	INSERT INTO [onBoardingModule] ([dmaReportsId],[ModuleName],[CreatedAt],[CreatedBy]) VALUES(19, 'Treasury', GETDATE(), 'system')
END
GO 

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Treasury' AND Purpose = 'Cover Margin/Cash')
BEGIN
	INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Treasury', 'Cover Margin/Cash',GETDATE(),-1, NULL, NULL, 1)
END 
GO
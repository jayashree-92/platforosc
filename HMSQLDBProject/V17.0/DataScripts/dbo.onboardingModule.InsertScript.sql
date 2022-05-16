
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

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Treasury' AND Purpose = 'Custodian Transfer')
BEGIN
	INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Treasury', 'Custodian Transfer',GETDATE(),-1, NULL, NULL, 1)
END 

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Treasury' AND Purpose = 'Distribution Payment')
BEGIN
	INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Treasury', 'Distribution Payment',GETDATE(),-1, NULL, NULL, 1)
END 

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Treasury' AND Purpose = 'Excess Cash Payment')
BEGIN
	INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Treasury', 'Excess Cash Payment',GETDATE(),-1, NULL, NULL, 1)
END 

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Treasury' AND Purpose = 'FX')
BEGIN
	INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Treasury', 'FX',GETDATE(),-1, NULL, NULL, 1)
END

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Treasury' AND Purpose = 'Margin Payment')
BEGIN
	INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Treasury', 'Margin Payment',GETDATE(),-1, NULL, NULL, 1)
END

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Treasury' AND Purpose = 'Money Market Transfer')
BEGIN
	INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Treasury', 'Money Market Transfer',GETDATE(),-1, NULL, NULL, 1)
END

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Treasury' AND Purpose = 'PB to PB Transfer')
BEGIN
	INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Treasury', 'PB to PB Transfer',GETDATE(),-1, NULL, NULL, 1)
END 

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Treasury' AND Purpose = 'Redemption Payment')
BEGIN
	INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Treasury', 'Redemption Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

select * from hmsWirePurposeLkup
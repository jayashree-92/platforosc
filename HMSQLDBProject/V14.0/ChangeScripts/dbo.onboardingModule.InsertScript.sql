
IF NOT EXISTS(SELECT * FROM onBoardingModule WHERE dmaReportsId=16)
BEGIN
INSERT INTO [onBoardingModule] ([dmaReportsId],[ModuleName],[CreatedAt],[CreatedBy]) VALUES(16, 'Interest Payment', GETDATE(), 'system')
END
GO 

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Interest Report' AND Purpose = 'Interest Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Interest Report', 'Interest Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO






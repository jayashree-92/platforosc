IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Admin Fees')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Admin Fees',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Audit Fees')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Audit Fees',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'CDS Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'CDS Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Collateral Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Collateral Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Custodian Transfer')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Custodian Transfer',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Director Fees')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Director Fees',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Distribution Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Distribution Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Excess Cash Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Excess Cash Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'FX')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'FX',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Fund Expenses')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Fund Expenses',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Futures')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Futures',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'IPO Subscription')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'IPO Subscription',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'ISDA Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'ISDA Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Interest Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Interest Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Legal Fees')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Legal Fees',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Loan Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Loan Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Management Fees')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Management Fees',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Margin Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Margin Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Money Market Transfer')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Money Market Transfer',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'OTC Derivatives')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'OTC Derivatives',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'OTC Option Premiums')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'OTC Option Premiums',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'PB to PB Transfer')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'PB to PB Transfer',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Private Debt Loan Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Private Debt Loan Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Private Investment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Private Investment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Redemption Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Redemption Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Repo Margin')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Repo Margin',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Rights Offer')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Rights Offer',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Swap Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Swap Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Tax Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Tax Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Trade Related')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Trade Related',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Tri Party')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Tri Party',GETDATE(),-1, NULL, NULL, 1)
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWirePurposeLkup WHERE ReportName = 'Adhoc Report' AND Purpose = 'Vendor Payment')
BEGIN
INSERT INTO [hmsWirePurposeLkup] ([ReportName], [Purpose],[CreatedAt],[CreatedBy],[ModifiedBy],[ModifiedAt],[IsApproved]) VALUES('Adhoc Report', 'Vendor Payment',GETDATE(),-1, NULL, NULL, 1)
END 
GO


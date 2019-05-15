IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Collateral' and Purpose = 'Send Call')
BEGIN
INSERT INTO hmsWirePurposeLkup (ReportName,Purpose) VALUES ('Collateral','Send Call')
END
GO

IF NOT EXISTS(SELECT * FROM hmsWireTransferTypeLKup WHERE TransferType = 'Notice')
BEGIN
INSERT INTO hmsWireTransferTypeLKup ([TransferType]) VALUES ('Notice');
END
GO




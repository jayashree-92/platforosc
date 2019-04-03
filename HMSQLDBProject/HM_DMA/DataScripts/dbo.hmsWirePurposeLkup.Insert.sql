IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup WHERE ReportName = 'Collateral' and Purpose = 'Send Call')
BEGIN
INSERT INTO hmsWirePurposeLkup (ReportName,Purpose) VALUES ('Collateral','Send Call')
END
GO
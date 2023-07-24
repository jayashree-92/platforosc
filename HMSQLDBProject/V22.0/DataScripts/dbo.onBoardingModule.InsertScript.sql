use HM_WIRES

IF NOT EXISTS( SELECT * FROM onBoardingModule where dmaReportsId = 22 and ModuleName = 'OTC Settlements')
BEGIN
INSERT INTO onBoardingModule VALUES (22, 'OTC Settlements', GETDATE(), 'SYSTEM')
END
GO

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup where ReportName = 'OTC Trade Lifecycle')
BEGIN
INSERT INTO hmsWirePurposeLkup VALUES ('OTC Trade Lifecycle', 'OTC Settlements', -1, GETDATE(), NULL,NULL,1)
END
GO

IF NOT EXISTS(SELECT * FROM hmsWirePurposeLkup where ReportName = 'Treasury')
BEGIN
INSERT INTO hmsWirePurposeLkup VALUES ('Treasury', 'Cash Optimization', -1, GETDATE(), NULL,NULL,1)
END
GO

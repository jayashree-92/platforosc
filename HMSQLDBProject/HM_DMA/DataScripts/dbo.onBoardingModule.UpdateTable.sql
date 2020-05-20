
USE HM_WIRES
IF NOT EXISTS(SELECT * FROM onBoardingModule WHERE ModuleName = 'Treasury')
BEGIN
INSERT INTO onBoardingModule VALUES (3, 'Treasury', GETDATE(), 'System')
END
GO
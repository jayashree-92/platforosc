
USE HM_WIRES
IF EXISTS(SELECT * FROM onBoardingModule WHERE ModuleName = 'Treasury')
BEGIN
delete from onBoardingModule where ModuleName = 'Treasury'
END
GO
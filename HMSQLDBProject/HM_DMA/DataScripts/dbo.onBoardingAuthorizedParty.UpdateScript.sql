USE HM_WIRES
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingAuthorizedParty_bkup_V21_0' AND TABLE_SCHEMA = 'DmaBackup')
BEGIN
	SELECT * INTO DmaBackup.onBoardingAuthorizedParty_bkup_V21_0 FROM onBoardingAuthorizedParty
END
GO

IF EXISTS(SELECT * FROM onBoardingAuthorizedParty WHERE AuthorizedParty='Hedgemark')
BEGIN
	UPDATE onBoardingAuthorizedParty SET  AuthorizedParty='Innocap' WHERE AuthorizedParty='Hedgemark'
END
GO


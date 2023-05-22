USE HM_WIRES
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingAccount_bkup_V21_1' AND TABLE_SCHEMA = 'DmaBackup')
BEGIN
	SELECT * INTO DmaBackup.onBoardingAccount_bkup_V21_1 FROM onBoardingAccount
	UPDATE onBoardingAccount SET IsDeleted=1 WHERE AccountType in ('Custody','DDA') and onboardingAccountStatus in ('Created') and isdeleted=0 and updatedat<'2023-01-01'
END
GO


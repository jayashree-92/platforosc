IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingAccount' AND COLUMN_NAME = 'dmaCounterpartyFamilyId')
BEGIN
	EXEC sp_rename 'dbo.onBoardingAccount.BrokerId', 'dmaCounterpartyFamilyId', 'COLUMN';
END
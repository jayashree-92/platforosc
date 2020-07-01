IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingAccount' AND COLUMN_NAME = 'dmaCounterpartyFamilyId')
BEGIN
	EXEC sp_rename 'dbo.onBoardingAccount.BrokerId', 'dmaCounterpartyFamilyId', 'COLUMN';
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingAccount' AND COLUMN_NAME = 'dmaCounterpartyId')
BEGIN
	ALTER TABLE onBoardingAccount ADD dmaCounterpartyId BIGINT
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IsExcludedFromTreasuryMarginCheck' AND TABLE_NAME = 'onBoardingAccount')
BEGIN
	ALTER TABLE onBoardingAccount ADD IsExcludedFromTreasuryMarginCheck BIT NOT NULL DEFAULT (0);
END
GO
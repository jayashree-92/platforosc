IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingCurrency' AND COLUMN_NAME = 'onBoardingCurrencyId')
BEGIN
	EXEC sp_rename 'dbo.onBoardingCurrency.onBoardingCurrencyId', 'hmsCurrencyId', 'COLUMN';
END
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingCurrency')
BEGIN
	EXEC sp_rename 'dbo.onBoardingCurrency', 'hmsCurrency';
END


IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingWirePortalCutoff' AND COLUMN_NAME = 'onBoardingWirePortalCutoffId')
BEGIN
	EXEC sp_rename 'dbo.onBoardingWirePortalCutoff.onBoardingWirePortalCutoffId', 'hmsWirePortalCutoffId', 'COLUMN';
END
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingWirePortalCutoff')
BEGIN
	EXEC sp_rename 'dbo.onBoardingWirePortalCutoff', 'hmsWirePortalCutoff';
END	
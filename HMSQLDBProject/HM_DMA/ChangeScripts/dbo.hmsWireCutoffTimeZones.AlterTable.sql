IF NOT EXISTS(SELECT 8 FROM SYS.OBJECTS WHERE NAME = 'UQ1_hmsWireCutoffTimeZones_TimeZone_TimeZoneStandardName')
BEGIN
	ALTER TABLE [dbo].[hmsWireCutoffTimeZones] WITH CHECK ADD CONSTRAINT [UQ1_hmsWireCutoffTimeZones_TimeZone_TimeZoneStandardName] UNIQUE (TimeZone,TimeZoneStandardName)
END
GO

IF NOT EXISTS(SELECT 8 FROM SYS.OBJECTS WHERE NAME = 'UQ1_onBoardingCurrency_Currency')
BEGIN
	ALTER TABLE [dbo].[onBoardingCurrency] WITH CHECK ADD CONSTRAINT [UQ1_onBoardingCurrency_Currency] UNIQUE (Currency)
END
GO

IF NOT EXISTS(SELECT 8 FROM SYS.OBJECTS WHERE NAME = 'UQ1_onBoardingCashInstruction_CashInstruction')
BEGIN
	ALTER TABLE [dbo].[onBoardingCashInstruction] WITH CHECK ADD CONSTRAINT [UQ1_onBoardingCashInstruction_CashInstruction] UNIQUE (CashInstruction)
END
GO
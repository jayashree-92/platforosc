
IF NOT EXISTS (SELECT 8 FROM onBoardingWirePortalCutoff WHERE CashInstruction = 'BNP Neolink' AND Country = 'United Kingdom' AND Currency = 'GBP')
BEGIN
INSERT INTO onBoardingWirePortalCutoff (CashInstruction, Country, Currency, CutOffTime, DaystoWire) VALUES ('BNP Neolink', 'United Kingdom', 'GBP', '02:10:00', 0)
END
GO

IF NOT EXISTS (SELECT 8 FROM onBoardingWirePortalCutoff WHERE CashInstruction = 'BNP Neolink' AND Country = 'EU' AND Currency = 'EUR')
BEGIN
INSERT INTO onBoardingWirePortalCutoff (CashInstruction, Country, Currency, CutOffTime, DaystoWire) VALUES ('BNP Neolink', 'EU', 'EUR', '10:20:00', 0)
END
GO

IF NOT EXISTS (SELECT 8 FROM onBoardingWirePortalCutoff WHERE CashInstruction = 'BNP Neolink' AND Country = 'Japan' AND Currency = 'JPY')
BEGIN
INSERT INTO onBoardingWirePortalCutoff (CashInstruction, Country, Currency, CutOffTime, DaystoWire) VALUES ('BNP Neolink', 'Japan', 'JPY', '11:45:00', 1)
END
GO

IF NOT EXISTS (SELECT 8 FROM onBoardingWirePortalCutoff WHERE CashInstruction = 'BNP Neolink' AND Country = 'USA' AND Currency = 'USD')
BEGIN
INSERT INTO onBoardingWirePortalCutoff (CashInstruction, Country, Currency, CutOffTime, DaystoWire) VALUES ('BNP Neolink', 'USA', 'USD', '04:00:00', 0)
END
GO

IF NOT EXISTS (SELECT 8 FROM onBoardingWirePortalCutoff WHERE CashInstruction = 'BBH WorldView' AND Country = 'United Kingdom' AND Currency = 'GBP')
BEGIN
INSERT INTO onBoardingWirePortalCutoff (CashInstruction, Country, Currency, CutOffTime, DaystoWire) VALUES ('BBH WorldView', 'United Kingdom', 'GBP', '10:00:00', 0)
END
GO

IF NOT EXISTS (SELECT 8 FROM onBoardingWirePortalCutoff WHERE CashInstruction = 'BBH WorldView' AND Country = 'EU' AND Currency = 'EUR')
BEGIN
INSERT INTO onBoardingWirePortalCutoff (CashInstruction, Country, Currency, CutOffTime, DaystoWire) VALUES ('BBH WorldView', 'EU', 'EUR', '10:00:00', 0)
END
GO

IF NOT EXISTS (SELECT 8 FROM onBoardingWirePortalCutoff WHERE CashInstruction = 'BBH WorldView' AND Country = 'Japan' AND Currency = 'JPY')
BEGIN
INSERT INTO onBoardingWirePortalCutoff (CashInstruction, Country, Currency, CutOffTime, DaystoWire) VALUES ('BBH WorldView', 'Japan', 'JPY', '22:00:00', 0)
END
GO

IF NOT EXISTS (SELECT 8 FROM onBoardingWirePortalCutoff WHERE CashInstruction = 'BBH WorldView' AND Country = 'USA' AND Currency = 'USD')
BEGIN
INSERT INTO onBoardingWirePortalCutoff (CashInstruction, Country, Currency, CutOffTime, DaystoWire) VALUES ('BBH WorldView', 'USA', 'USD', '17:30:00', 0)
END
GO

IF NOT EXISTS (SELECT 8 FROM onBoardingCashInstruction WHERE CashInstruction = 'BNP Neolink')
BEGIN
INSERT INTO onBoardingCashInstruction (CashInstruction) VALUES ('BNP Neolink')
END
GO

IF NOT EXISTS (SELECT 8 FROM onBoardingCashInstruction WHERE CashInstruction = 'BBH WorldView')
BEGIN
INSERT INTO onBoardingCashInstruction (CashInstruction) VALUES ('BBH WorldView')
END
GO

UPDATE onBoardingWirePortalCutoff  SET CutOffTimeZone = 'CET', CutOffTime = '16:00:00', DaysToWire = 0 WHERE CashInstruction = 'BBH WorldView' AND Currency = 'EUR'
GO

UPDATE onBoardingWirePortalCutoff  SET CutOffTimeZone = 'GMT', CutOffTime = '15:00:00', DaysToWire = 0 WHERE CashInstruction = 'BBH WorldView' AND Currency = 'GBP'
GO

UPDATE onBoardingWirePortalCutoff  SET CutOffTimeZone = 'JST', CutOffTime = '12:00:00', DaysToWire = 0 WHERE CashInstruction = 'BBH WorldView' AND Currency = 'JPY'
GO

UPDATE onBoardingWirePortalCutoff  SET CutOffTimeZone = 'EST', CutOffTime = '17:30:00', DaysToWire = 0 WHERE CashInstruction = 'BBH WorldView' AND Currency = 'USD'
GO

UPDATE onBoardingWirePortalCutoff  SET CutOffTimeZone = 'CET', CutOffTime = '16:20:00', DaysToWire = 0 WHERE CashInstruction = 'BNP Neolink' AND Currency = 'EUR'
GO

UPDATE onBoardingWirePortalCutoff  SET CutOffTimeZone = 'CET', CutOffTime = '18:10:00', DaysToWire = 0 WHERE CashInstruction = 'BNP Neolink' AND Currency = 'GBP'
GO

UPDATE onBoardingWirePortalCutoff  SET CutOffTimeZone = 'CET', CutOffTime = '17:45:00', DaysToWire = 1 WHERE CashInstruction = 'BNP Neolink' AND Currency = 'JPY'
GO

UPDATE onBoardingWirePortalCutoff  SET CutOffTimeZone = 'CET', CutOffTime = '22:00:00', DaysToWire = 0 WHERE CashInstruction = 'BNP Neolink' AND Currency = 'USD'
GO

USE HM_WIRES
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsSystemPreferences_bkup_V45_0' AND TABLE_SCHEMA = 'DmaBackup')
BEGIN
SELECT * INTO DmaBackup.hmsSystemPreferences_bkup_V45_0 FROM hmsSystemPreferences;
END
GO

IF EXISTS(SELECT * FROM sys.tables where [name]='hmsSystemPreferences' )
BEGIN
	DROP TABLE hmsSystemPreferences;
END
GO







IF NOT EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'Field' and TABLE_NAME = 'hmsUserAuditLog')
BEGIN
	ALTER TABLE hmsUserAuditLog ADD Field VARCHAR(30) NULL
END
GO

IF NOT EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'PreviousStateValue' and TABLE_NAME = 'hmsUserAuditLog')
BEGIN
	ALTER TABLE hmsUserAuditLog ADD PreviousStateValue VARCHAR(50) NULL
END
GO

IF NOT EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'ModifiedStateValue' and TABLE_NAME = 'hmsUserAuditLog')
BEGIN
	ALTER TABLE hmsUserAuditLog ADD ModifiedStateValue VARCHAR(50) NULL
END
GO

IF NOT EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'IsLogFromOps' and TABLE_NAME = 'hmsUserAuditLog')
BEGIN
	ALTER TABLE hmsUserAuditLog ADD IsLogFromOps BIT NOT NULL DEFAULT 0
END
GO

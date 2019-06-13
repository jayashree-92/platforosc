IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'Field' AND TABLE_NAME = 'hmsUserAuditLog' AND CHARACTER_MAXIMUM_LENGTH='30')
BEGIN
	ALTER TABLE hmsUserAuditLog ALTER COLUMN [Field] VARCHAR(300) NULL ;
END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'PreviousStateValue' AND TABLE_NAME = 'hmsUserAuditLog' AND CHARACTER_MAXIMUM_LENGTH='50')
BEGIN
	ALTER TABLE hmsUserAuditLog ALTER COLUMN [PreviousStateValue] VARCHAR(300) NULL ;
END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'ModifiedStateValue' AND TABLE_NAME = 'hmsUserAuditLog' AND CHARACTER_MAXIMUM_LENGTH='50')
BEGIN
	ALTER TABLE hmsUserAuditLog ALTER COLUMN [ModifiedStateValue] VARCHAR(300) NULL ;
END
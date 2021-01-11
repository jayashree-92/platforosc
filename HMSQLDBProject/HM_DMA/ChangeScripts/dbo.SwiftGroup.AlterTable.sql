
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsWirePortalCutoff' AND COLUMN_NAME = 'IsApproved')
BEGIN
	ALTER TABLE hmsWirePortalCutoff ADD IsApproved BIT NOT NULL DEFAULT(0)
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsWirePortalCutoff' AND COLUMN_NAME = 'ApprovedBy')
BEGIN
	ALTER TABLE hmsWirePortalCutoff ADD ApprovedBy INT
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsWirePortalCutoff' AND COLUMN_NAME = 'ApprovedAt')
BEGIN
	ALTER TABLE hmsWirePortalCutoff ADD ApprovedAt DATETIME DEFAULT(GETDATE())
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'hmsSwiftGroup_bkup_V10_0' AND TABLE_SCHEMA ='DmaBackup')
BEGIN
   SELECT * INTO DMABackup.hmsSwiftGroup_bkup_V10_0 FROM hmsSwiftGroup;
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsSwiftGroup' AND COLUMN_NAME = 'RequestedBy')
BEGIN
	ALTER TABLE hmsSwiftGroup ADD RequestedBy INT
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsSwiftGroup' AND COLUMN_NAME = 'RequestedAt')
BEGIN
	ALTER TABLE hmsSwiftGroup ADD RequestedAt DATETIME DEFAULT(GETDATE())
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsSwiftGroup' AND COLUMN_NAME = 'ApprovedBy')
BEGIN
	ALTER TABLE hmsSwiftGroup ADD ApprovedBy INT
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsSwiftGroup' AND COLUMN_NAME = 'ApprovedAt')
BEGIN
	ALTER TABLE hmsSwiftGroup ADD ApprovedAt DATETIME DEFAULT(GETDATE())
END
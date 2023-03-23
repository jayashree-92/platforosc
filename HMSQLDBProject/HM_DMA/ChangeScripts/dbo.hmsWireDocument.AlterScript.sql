USE HM_Wires


IF  EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'FileName' AND TABLE_NAME = 'hmsWireDocument' and CHARACTER_MAXIMUM_LENGTH=100)
BEGIN
	ALTER TABLE hmsWireDocument ALTER COLUMN [FileName] VARCHAR(256) NOT NULL
END
GO
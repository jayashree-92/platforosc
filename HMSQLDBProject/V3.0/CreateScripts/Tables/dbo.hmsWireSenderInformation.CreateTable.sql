IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireSenderInformation')
BEGIN
	CREATE TABLE [dbo].[hmsWireSenderInformation]
	(
		hmsWireSenderInformationId INT NOT NULL IDENTITY(1,1),
		[SenderInformation] VARCHAR(30),
		[Description] VARCHAR(500)

		CONSTRAINT hmsWireSenderInformation_PK PRIMARY KEY NONCLUSTERED (hmsWireSenderInformationId)
	);
	
	INSERT INTO hmsWireSenderInformation ([SenderInformation],[Description]) VALUES ('ACC', 'Instructions following are for the account with institution');
	INSERT INTO hmsWireSenderInformation ([SenderInformation],[Description]) VALUES ('BNF', 'Information following is for the Beneficiary');
	INSERT INTO hmsWireSenderInformation ([SenderInformation],[Description]) VALUES ('INS', 'The instructing institution which instructed the Sender to execute the transaction');
	INSERT INTO hmsWireSenderInformation ([SenderInformation],[Description]) VALUES ('REC', 'Instructions following are for the Receiver of the message');
	INSERT INTO hmsWireSenderInformation ([SenderInformation],[Description]) VALUES ('TSU', 'Invoice');
END
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'SenderInformationId' AND TABLE_NAME = 'hmsWires')
BEGIN
	ALTER TABLE hmsWires ADD [SenderInformationId] INT NULL;
	
	DECLARE @Command  NVARCHAR(1000)
	SELECT @Command ='UPDATE dbo.hmsWires 
			SET SenderInformationId = CASE 
			WHEN WireMessageTypeId = 1 THEN 1
			WHEN WireMessageTypeId = 3 OR WireMessageTypeId = 4 THEN 2
			ELSE NULL
	      END'
	EXECUTE (@Command);
	
	ALTER TABLE hmsWires ADD CONSTRAINT FK_hmsWires_hmsWireSenderInformation_SenderInformationId FOREIGN KEY ([SenderInformationId]) REFERENCES hmsWireSenderInformation(hmsWireSenderInformationId);
END
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'SenderDescription' AND TABLE_NAME = 'hmsWires')
BEGIN
	ALTER TABLE hmsWires ADD [SenderDescription] VARCHAR(500) NULL;
END
GO
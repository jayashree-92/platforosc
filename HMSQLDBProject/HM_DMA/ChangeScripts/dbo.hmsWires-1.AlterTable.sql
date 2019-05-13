IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'Status' AND TABLE_NAME = 'hmsSwiftStatusLkup')
BEGIN
	ALTER TABLE hmsSwiftStatusLkup DROP CONSTRAINT UK_hmsSwiftStatusLkup_Status;

	ALTER TABLE hmsSwiftStatusLkup ALTER COLUMN [Status] VARCHAR(30) NOT NULL ;

	ALTER TABLE [dbo].[hmsSwiftStatusLkup]  WITH CHECK ADD CONSTRAINT [UK_hmsSwiftStatusLkup_Status] UNIQUE ([Status])
END


IF NOT EXISTS(SELECT * FROM SYS.OBJECTS WHERE NAME = 'UK_hmsSwiftStatusLkup_Status')
BEGIN
	ALTER TABLE [dbo].[hmsSwiftStatusLkup]  WITH CHECK ADD CONSTRAINT [UK_hmsSwiftStatusLkup_Status] UNIQUE ([Status])
END


--THis needs to be executed by the DBA team IN PROD
--IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'Amount' AND TABLE_NAME = 'hmsWires')
--BEGIN
--	ALTER TABLE hmsWires ALTER COLUMN [Amount] DECIMAL(18,2) NOT NULL ;
--END


IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireLogTypeLkup')
BEGIN
	CREATE TABLE [dbo].[hmsWireLogTypeLkup]
	(
		hmsWireLogTypeId INT NOT NULL IDENTITY(1,1),
		[LogType] VARCHAR(30),

		CONSTRAINT hmsWireLogTypeLkup_PK PRIMARY KEY NONCLUSTERED (hmsWireLogTypeId)
	);
	
	INSERT INTO hmsWireLogTypeLkup ([LogType]) VALUES ('Outbound');	
	INSERT INTO hmsWireLogTypeLkup ([LogType]) VALUES ('Acknowledged');
	INSERT INTO hmsWireLogTypeLkup ([LogType]) VALUES ('N-Acknowledged');
	INSERT INTO hmsWireLogTypeLkup ([LogType]) VALUES ('Confirmation');
END
GO


IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireLogs')
BEGIN
	CREATE TABLE [dbo].[hmsWireLogs](
	[hmsWireLogId] BIGINT NOT NULL IDENTITY(1,1),
	[hmsWireId] BIGINT NOT NULL,
	[hmsWireWorkflowLogId] BIGINT NOT NULL,
	[hmsWireLogTypeId] INT NOT NULL,
	[WireMessageTypeId] INT NOT NULL,
	[SwiftMessage] VARCHAR(MAX) NULL,
	[AdditionalDetails] VARCHAR(1000) NULL,
	[RecCreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),

	CONSTRAINT hmsWireLogs_PK PRIMARY KEY NONCLUSTERED (hmsWireLogId)
	);

	ALTER TABLE hmsWireLogs ADD CONSTRAINT FK_hmsWireLogs_hmsWires_hmsWireId FOREIGN KEY ([hmsWireId]) REFERENCES hmsWires(hmsWireId);
	ALTER TABLE hmsWireLogs ADD CONSTRAINT FK_hmsWireLogs_hmsWireMessageType_WireMessageTypeId FOREIGN KEY ([WireMessageTypeId]) REFERENCES hmsWireMessageType(hmsWireMessageTypeId);
	ALTER TABLE hmsWireLogs ADD CONSTRAINT FK_hmsWireLogs_hmsWireWorkflowLog_hmsWireWorkflowLogId FOREIGN KEY ([hmsWireWorkflowLogId]) REFERENCES hmsWireWorkflowLog(hmsWireWorkflowLogId);
	ALTER TABLE hmsWireLogs ADD CONSTRAINT FK_hmsWireLogs_hmsWireLogTypeLkup_hmsWireLogTypeId FOREIGN KEY ([hmsWireLogTypeId]) REFERENCES hmsWireLogTypeLkup(hmsWireLogTypeId);

	/*NOT REQUIRED TO BE EXECUTED IN PRODUCTION*/
	
	--INSERT INTO  [hmsWireLogs] (hmsWireId,hmsWireWorkflowLogId,hmsWireLogTypeId,WireMessageTypeId,SwiftMessage,AdditionalDetails,RecCreatedAt)
	--SELECT  wl.hmsWireId,wfl.hmsWireWorkflowLogId,1 as hmsWireLogTypeId,WireMessageTypeId,OutBoundSwiftMessage,ExceptionDetails,RecCreatedAt FROM hmsWireLog wl
	--inner join hmsWireWorkflowLog wfl on wl.WireStatusId = wfl.WireStatusId and wl.hmsWireId= wfl.hmsWireId and wfl.SwiftStatusId =2 and OutBoundSwiftMessage is not null;

	
	--INSERT INTO  [hmsWireLogs] (hmsWireId,hmsWireWorkflowLogId,hmsWireLogTypeId,WireMessageTypeId,SwiftMessage,AdditionalDetails,RecCreatedAt)
	--SELECT  wl.hmsWireId,wfl.hmsWireWorkflowLogId,3 as hmsWireLogTypeId,WireMessageTypeId,ServiceSwiftMessage,ExceptionDetails,RecCreatedAt FROM hmsWireLog wl
	--inner join hmsWireWorkflowLog wfl on wl.WireStatusId = wfl.WireStatusId and wl.hmsWireId= wfl.hmsWireId  and wfl.SwiftStatusId in (4) and ServiceSwiftMessage is not null;


	--INSERT INTO  [hmsWireLogs] (hmsWireId,hmsWireWorkflowLogId,hmsWireLogTypeId,WireMessageTypeId,SwiftMessage,AdditionalDetails,RecCreatedAt)
	--SELECT  wl.hmsWireId,wfl.hmsWireWorkflowLogId,2 as hmsWireLogTypeId,WireMessageTypeId,ServiceSwiftMessage,ExceptionDetails,RecCreatedAt FROM hmsWireLog wl
	--inner join hmsWireWorkflowLog wfl on wl.WireStatusId = wfl.WireStatusId and wl.hmsWireId= wfl.hmsWireId  and wfl.SwiftStatusId in (3) and ServiceSwiftMessage is not null;

END


IF EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireLog')
BEGIN
DROP TABLE hmsWireLog;
END

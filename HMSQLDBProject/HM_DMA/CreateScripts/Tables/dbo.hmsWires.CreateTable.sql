IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWirePurposeLkup')
BEGIN
	CREATE TABLE [dbo].[hmsWirePurposeLkup]
	(
		hmsWirePurposeId INT NOT NULL IDENTITY(1,1),
		ReportName VARCHAR(20),
		Purpose VARCHAR(100),

		CONSTRAINT hmsWirePurposeLkup_PK PRIMARY KEY NONCLUSTERED (hmsWirePurposeId)
	);

	INSERT INTO hmsWirePurposeLkup (ReportName,Purpose) VALUES ('Collateral','Respond to Broker Call')
END
GO

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsSwiftStatusLkup')
BEGIN
	CREATE TABLE [dbo].[hmsSwiftStatusLkup]
	(
		hmsSwiftStatusId INT NOT NULL IDENTITY(1,1),
		[Status] VARCHAR(20),

		CONSTRAINT hmsSwiftStatusLkup_PK PRIMARY KEY NONCLUSTERED (hmsSwiftStatusId)
	);
	
	INSERT INTO hmsSwiftStatusLkup ([Status]) VALUES ('Not Initiated');
	INSERT INTO hmsSwiftStatusLkup ([Status]) VALUES ('Processing');
	INSERT INTO hmsSwiftStatusLkup ([Status]) VALUES ('Acknowledged');
	INSERT INTO hmsSwiftStatusLkup ([Status]) VALUES ('Completed');
	INSERT INTO hmsSwiftStatusLkup ([Status]) VALUES ('Failed');
END
GO

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireStatusLkup')
BEGIN
	CREATE TABLE [dbo].[hmsWireStatusLkup]
	(
		hmsWireStatusId INT NOT NULL IDENTITY(1,1),
		[Status] VARCHAR(20),

		CONSTRAINT hmsWireStatusLkup_PK PRIMARY KEY NONCLUSTERED (hmsWireStatusId)
	);

	INSERT INTO hmsWireStatusLkup ([Status]) VALUES ('Drafted');
	INSERT INTO hmsWireStatusLkup ([Status]) VALUES ('Initiated');
	INSERT INTO hmsWireStatusLkup ([Status]) VALUES ('Approved');
	INSERT INTO hmsWireStatusLkup ([Status]) VALUES ('Cancelled');
	INSERT INTO hmsWireStatusLkup ([Status]) VALUES ('Failed');
END
GO

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireMessageType')
BEGIN
	CREATE TABLE [dbo].[hmsWireMessageType]
	(
		hmsWireMessageTypeId INT NOT NULL IDENTITY(1,1),
		[MessageType] VARCHAR(20) NOT NULL,
		[Category] INT NOT NULL,
		[IsOutbound] BIT NOT NULL DEFAULT(1),
		[Description] VARCHAR(300) NOT NULL,

		CONSTRAINT hmsWireMessageType_PK PRIMARY KEY NONCLUSTERED (hmsWireMessageTypeId)
	);

	 -- OUT BOUND Messages
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT103',1,1,'Single customer credit transfer');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT192',1,1,'Request for Cancellation');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT202',2,1,'General Financial Institution Transfer');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT202 COV',2,1,'General Financial Institution Transfer');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT210',2,1,'Notice to Receive');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT292',2,1,'Request for Cancellation');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT540',5,1,'Receive Free');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT542',5,1,'Deliver Free');

	-- IN BOUND Messages
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT548',5,0,'Settlement Status and Processing Advice');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT900',9,0,'Confirmation of Debit');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT910',9,0,'Confirmation of Credit');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT196',1,0,'Answers - Confirmation of Cancellation');
	INSERT INTO hmsWireMessageType ([MessageType],[Category],[IsOutbound],[Description]) VALUES ('MT296',2,0,'Answers - Confirmation of Cancellation');

END
GO

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWires')
BEGIN
	CREATE TABLE [dbo].[hmsWires]
	(
		hmsWireId BIGINT NOT NULL IDENTITY(1,1),	
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[CreatedBy] INT NOT NULL,
		[WirePurposeId] INT NOT NULL,
		[ContextDate] DATE NOT NULL,
		[OnBoardAccountId] BIGINT NOT NULL,
		[OnBoardSSITemplateId] BIGINT NOT NULL,
		[OnBoardAgreementId] BIGINT NOT NULL,
		[hmFundId] BIGINT NOT NULL,
		[ValueDate] DATE NOT NULL DEFAULT(GETDATE()),
		[PaymentOrReceipt] VARCHAR(200) NOT NULL,
		[SendingAccountNumber] VARCHAR(200) NOT NULL,
		[SendingPlatform] VARCHAR(200) NOT NULL,
		[ReceivingAccountNumber] VARCHAR(200) NOT NULL,
		[Currency] VARCHAR(200) NOT NULL,
		[Amount] DECIMAL NOT NULL,
		[DeliveryCharges] VARCHAR(20) NULL,
		[IsBookTransfer] BIT NOT NULL DEFAULT (0),
		[WireMessageTypeId] INT NOT NULL,
		[WireStatusId] INT NOT NULL,
		[SwiftStatusId] INT NOT NULL,
		[LastModifiedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[LastUpdatedBy] INT NOT NULL
		CONSTRAINT hmsWires_PK PRIMARY KEY NONCLUSTERED (hmsWireId)
	);


	ALTER TABLE hmsWires ADD CONSTRAINT FK_hmsWires_hmsWirePurposeLkup_WirePurposeID FOREIGN KEY ([WirePurposeID]) REFERENCES hmsWirePurposeLkup(hmsWirePurposeId);
	ALTER TABLE hmsWires ADD CONSTRAINT FK_hmsWires_hmsWireStatusLkup_WireStatusId FOREIGN KEY ([WireStatusId]) REFERENCES hmsWireStatusLkup(hmsWireStatusId);
	ALTER TABLE hmsWires ADD CONSTRAINT FK_hmsWires_hmsSwiftStatusLkup_WireStatusId FOREIGN KEY ([SwiftStatusId]) REFERENCES hmsSwiftStatusLkup(hmsSwiftStatusId);
	ALTER TABLE hmsWires ADD CONSTRAINT FK_hmsWires_hmsWireMessageType_WireMessageTypeId FOREIGN KEY ([WireMessageTypeId]) REFERENCES hmsWireMessageType(hmsWireMessageTypeId);
END
GO

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireLog')
BEGIN
	CREATE TABLE [dbo].[hmsWireLog](
	hmsWireLogId BIGINT NOT NULL IDENTITY(1,1),
	hmsWireId BIGINT NOT NULL,
	[WireStatusId] INT NOT NULL,
	[WireMessageTypeId] INT NOT NULL,
	[OutBoundSwiftMessage] VARCHAR(MAX) NULL,
	[IsFrontEndAcknowleged] BIT NOT NULL DEFAULT(0),
	[ServiceSwiftMessage] VARCHAR(MAX) NULL,
	[InBoundSwiftMessage] VARCHAR(MAX) NULL,
	[ConfirmationMessageDetails] VARCHAR(1000) NULL,
	[ExceptionDetails] VARCHAR(1000) NULL,
	[RecCreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),

	CONSTRAINT hmsWireLog_PK PRIMARY KEY NONCLUSTERED (hmsWireLogId)
	);

	ALTER TABLE hmsWireLog ADD CONSTRAINT FK_hmsWireLog_hmsWires_hmsWireId FOREIGN KEY ([hmsWireId]) REFERENCES hmsWires(hmsWireId);
	ALTER TABLE hmsWireLog ADD CONSTRAINT FK_hmsWireLog_hmsWireStatusLkup_WireStatusId FOREIGN KEY ([WireStatusId]) REFERENCES hmsWireStatusLkup(hmsWireStatusId);
	ALTER TABLE hmsWireLog ADD CONSTRAINT FK_hmsWireLog_hmsWireMessageType_WireMessageTypeId FOREIGN KEY ([WireMessageTypeId]) REFERENCES hmsWireMessageType(hmsWireMessageTypeId);
END

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireDocument')
BEGIN
	CREATE TABLE [dbo].[hmsWireDocument]
	(
		[hmsWireDocumentId] BIGINT NOT NULL IDENTITY(1,1),	
		[hmsWireId] BIGINT NOT NULL,
		[FileName] VARCHAR(100) NOT NULL,
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[CreatedBy] INT NOT NULL,
		CONSTRAINT hmsWireDocument_PK PRIMARY KEY NONCLUSTERED (hmsWireDocumentId),
		CONSTRAINT FK_hmsWireDocument_hmsWires_hmsWireId FOREIGN KEY ([hmsWireId]) REFERENCES hmsWires(hmsWireId)
	)	
END
GO

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireWorkflowLog')
BEGIN
	CREATE TABLE [dbo].[hmsWireWorkflowLog]
	(
		[hmsWireWorkflowLogId] BIGINT NOT NULL IDENTITY(1,1),	
		[hmsWireId] BIGINT NOT NULL,
		[WireStatusId] INT NOT NULL,
		[Comment] VARCHAR(8000) NOT NULL,
		[CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),
		[CreatedBy] INT NOT NULL,
		CONSTRAINT hmsWireWorkflowLog_PK PRIMARY KEY NONCLUSTERED (hmsWireWorkflowLogId),
		CONSTRAINT FK_hmsWireWorkflowLog_hmsWires_hmsWireId FOREIGN KEY ([hmsWireId]) REFERENCES hmsWires(hmsWireId),
		CONSTRAINT FK_hmsWireWorkflowLog_hmsWireStatusLkup_WireStatusId FOREIGN KEY ([WireStatusId]) REFERENCES hmsWireStatusLkup(hmsWireStatusId),
		CONSTRAINT UQ_hmsWireWorkflowLog_hmsWireId_WireStatusId UNIQUE ([hmsWireId],[WireStatusId])
)	

END
GO 

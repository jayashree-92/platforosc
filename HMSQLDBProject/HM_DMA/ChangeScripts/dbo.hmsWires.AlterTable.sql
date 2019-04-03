IF EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWires')
BEGIN
	IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'Comments' AND TABLE_NAME = 'hmsWires')
	BEGIN
		ALTER TABLE hmsWires DROP COLUMN [Comments] ;
	END
END

IF NOT EXISTS(SELECT * FROM SYS.OBJECTS WHERE NAME = 'UK_hmsWirePurposeLkup_ReportName_Purpose')
BEGIN
	ALTER TABLE [dbo].[hmsWirePurposeLkup]  WITH CHECK ADD CONSTRAINT [UK_hmsWirePurposeLkup_ReportName_Purpose] UNIQUE (ReportName,Purpose)
END

IF NOT EXISTS(SELECT * FROM SYS.OBJECTS WHERE NAME = 'UK_hmsWireStatusLkup_Status')
BEGIN
	ALTER TABLE [dbo].[hmsWireStatusLkup]  WITH CHECK ADD CONSTRAINT [UK_hmsWireStatusLkup_Status] UNIQUE ([Status])
END

IF NOT EXISTS(SELECT * FROM SYS.OBJECTS WHERE NAME = 'UK_hmsSwiftStatusLkup_Status')
BEGIN
	ALTER TABLE [dbo].[hmsSwiftStatusLkup]  WITH CHECK ADD CONSTRAINT [UK_hmsSwiftStatusLkup_Status] UNIQUE ([Status])
END


IF NOT EXISTS(SELECT * FROM SYS.OBJECTS WHERE NAME = 'UK_hmsWireMessageType_MessageType')
BEGIN
	ALTER TABLE [dbo].[hmsWireMessageType]  WITH CHECK ADD CONSTRAINT [UK_hmsWireMessageType_MessageType] UNIQUE (MessageType)
END


IF NOT EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'DeliveryCharges' and TABLE_NAME = 'hmsWires')
BEGIN
	ALTER TABLE hmsWires ADD DeliveryCharges VARCHAR(20) NULL
END
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE NAME = 'UQ_hmsWireWorkflowLog_hmsWireId_WireStatusId')
BEGIN
	ALTER TABLE hmsWireWorkflowLog DROP CONSTRAINT UQ_hmsWireWorkflowLog_hmsWireId_WireStatusId
END
GO 

IF((Select count(*) from hmsWireStatusLkup) =7)
BEGIN

	UPDATE hmsWires SET WireStatusId =5 WHERE WireStatusId >5;
	UPDATE hmsWireWorkflowLog SET WireStatusId =5 WHERE WireStatusId >5;
	DELETE FROM hmsWireStatusLkup WHERE hmsWireStatusId >5;

	UPDATE hmsWireStatusLkup SET [Status] = 'Drafted' WHERE hmsWireStatusId =1
	UPDATE hmsWireStatusLkup SET [Status] = 'Initiated' WHERE hmsWireStatusId =2
	UPDATE hmsWireStatusLkup SET [Status] = 'Approved' WHERE hmsWireStatusId =3
	UPDATE hmsWireStatusLkup SET [Status] = 'Cancelled' WHERE hmsWireStatusId =4
	UPDATE hmsWireStatusLkup SET [Status] = 'Failed' WHERE hmsWireStatusId =5

END


IF NOT EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'IsBookTransfer' and TABLE_NAME = 'hmsWires')
BEGIN
	ALTER TABLE hmsWires ADD IsBookTransfer BIT NOT NULL DEFAULT 0
END
GO


IF NOT EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'SwiftStatusId' and TABLE_NAME = 'hmsWires')
BEGIN
	ALTER TABLE hmsWires ADD [SwiftStatusId] INT NOT NULL DEFAULT(1);
	ALTER TABLE hmsWires ADD CONSTRAINT FK_hmsWires_hmsSwiftStatusLkup_WireStatusId FOREIGN KEY ([SwiftStatusId]) REFERENCES hmsSwiftStatusLkup(hmsSwiftStatusId);
END
GO


IF NOT EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'SwiftStatusId' and TABLE_NAME = 'hmsWireWorkflowLog')
BEGIN
	ALTER TABLE hmsWireWorkflowLog ADD [SwiftStatusId] INT NOT NULL DEFAULT(1);
	ALTER TABLE hmsWireWorkflowLog ADD CONSTRAINT FK_hmsWireWorkflowLog_hmsSwiftStatusLkup_WireStatusId FOREIGN KEY ([SwiftStatusId]) REFERENCES hmsSwiftStatusLkup(hmsSwiftStatusId);
END
GO

IF NOT EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'IsFrontEndAcknowleged' and TABLE_NAME = 'hmsWireLog')
BEGIN
	ALTER TABLE hmsWireLog ADD IsFrontEndAcknowleged BIT NOT NULL DEFAULT 0
END
GO


IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'DeadlineToApprove' AND TABLE_NAME = 'hmsWires')
BEGIN
	ALTER TABLE hmsWires DROP COLUMN [DeadlineToApprove] ;
END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IsAcknowledged' AND TABLE_NAME = 'hmsWires')
BEGIN

DECLARE @Command1 NVARCHAR(MAX)
  select @Command1 ='Alter Table hmsWires Drop Constraint [' + (
		select d.name
     from 
         sys.tables t
         join sys.default_constraints d on d.parent_object_id = t.object_id
         join sys.columns c on c.object_id = t.object_id
                               and c.column_id = d.parent_column_id
     where 
         t.name = 'hmsWires'
         and c.name = 'IsAcknowledged') + ']'
		
    print @Command1
    exec sp_executesql @Command1;


	ALTER TABLE hmsWires DROP COLUMN [IsAcknowledged] ;
END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IsCreditOrDebitConfirmed' AND TABLE_NAME = 'hmsWires')
BEGIN


DECLARE @Command2 NVARCHAR(MAX)
  select @Command2 ='Alter Table hmsWires Drop Constraint [' + (
		select d.name
     from 
         sys.tables t
         join sys.default_constraints d on d.parent_object_id = t.object_id
         join sys.columns c on c.object_id = t.object_id
                               and c.column_id = d.parent_column_id
     where 
         t.name = 'hmsWires'
         and c.name = 'IsCreditOrDebitConfirmed') + ']'
		
    print @Command2
    exec sp_executesql @Command2;

	ALTER TABLE hmsWires DROP COLUMN [IsCreditOrDebitConfirmed] ;
END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'CreditOrDebitConfirmedAt' AND TABLE_NAME = 'hmsWires')
BEGIN
	ALTER TABLE hmsWires DROP COLUMN [CreditOrDebitConfirmedAt] ;
END


IF NOT EXISTS(SELECT * FROM SYS.OBJECTS WHERE NAME = 'UK_hmsWires_ValueDate_WirePurposeId_OnBoardAccountId_OnBoardSSITemplateId')
BEGIN

DELETE FROM hmsWireDocument
		WHERE hmsWireId NOT IN (SELECT MAX(hmsWireId)
						  FROM   hmsWires
						  GROUP  BY ValueDate,
									WirePurposeId,
									OnBoardAccountId,
									OnBoardSSITemplateId
						 HAVING MAX(hmsWireId) IS NOT NULL)

DELETE FROM hmsWireWorkflowLog
		WHERE hmsWireId NOT IN (SELECT MAX(hmsWireId)
						  FROM   hmsWires
						  GROUP  BY ValueDate,
									WirePurposeId,
									OnBoardAccountId,
									OnBoardSSITemplateId
						 HAVING MAX(hmsWireId) IS NOT NULL)
 DELETE FROM hmsWireLog
		WHERE hmsWireId NOT IN (SELECT MAX(hmsWireId)
						  FROM   hmsWires
						  GROUP  BY ValueDate,
									WirePurposeId,
									OnBoardAccountId,
									OnBoardSSITemplateId
						 HAVING MAX(hmsWireId) IS NOT NULL)

 DELETE FROM hmsWires
		WHERE hmsWireId NOT IN (SELECT MAX(hmsWireId)
						  FROM   hmsWires
						  GROUP  BY ValueDate,
									WirePurposeId,
									OnBoardAccountId,
									OnBoardSSITemplateId
						 HAVING MAX(hmsWireId) IS NOT NULL)

	ALTER TABLE [dbo].[hmsWires]  WITH CHECK ADD CONSTRAINT [UK_hmsWires_ValueDate_WirePurposeId_OnBoardAccountId_OnBoardSSITemplateId] UNIQUE (ValueDate,WirePurposeId,OnBoardAccountId,OnBoardSSITemplateId)
END
GO






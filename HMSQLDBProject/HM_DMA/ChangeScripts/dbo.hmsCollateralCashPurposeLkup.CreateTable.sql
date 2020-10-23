IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsCollateralCashPurposeLkup')
BEGIN
	CREATE TABLE [dbo].[hmsCollateralCashPurposeLkup](
	[hmsCollateralCashPurposeLkupId] INT IDENTITY(1,1) NOT NULL,
	[PurposeCode] VARCHAR(16) NOT NULL,
	[Description] VARCHAR(50) NOT NULL,
	
CONSTRAINT UK_hmsCollateralCashPurposeLkup_WorkflowStatusCode UNIQUE(PurposeCode),
PRIMARY KEY  CLUSTERED  
(
	[hmsCollateralCashPurposeLkupId] ASC
) ON [PRIMARY]
) ON [PRIMARY]

--INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('NONREF' ,'No Refereces')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('CCPC' ,'Cleared Swap Collateral (Initial Margin)')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('SWCC' ,'Swap Client Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('SWBC' ,'Swap Broker Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('FWCC' ,'Forward Client Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('FWBC' ,'Forward Broker Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('TBCC' ,'TBA Client Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('TBBC' ,'TBA Broker Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('OPCC' ,'Option Client Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('OPBC' ,'Option Broker Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('RPCC' ,'Repo Client Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('RPBC' ,'Repo Broker Collateral')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('COLLINT' ,'Cash Collateral Interest')
INSERT INTO [hmsCollateralCashPurposeLkup]([PurposeCode],[Description]) VALUES ('INTE' ,'Cash Collateral Interest')


END


IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireFields')
BEGIN
	CREATE TABLE [dbo].[hmsWireFields](
	[hmsWireFieldId] BIGINT IDENTITY(1,1) NOT NULL,
	[hmsCollateralCashPurposeLkupId] INT NOT NULL DEFAULT(1),
	
PRIMARY KEY CLUSTERED  
(
	[hmsWireFieldId] ASC
) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[hmsWireFields]  WITH CHECK ADD CONSTRAINT [FK_hmsWireFields_hmsCollateralCashPurposeLkupId] 
		FOREIGN KEY([hmsCollateralCashPurposeLkupId]) REFERENCES [dbo].[hmsCollateralCashPurposeLkup] (hmsCollateralCashPurposeLkupId)

END


IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'hmsWires' AND COLUMN_NAME = 'hmsWireFieldId')
BEGIN
	ALTER TABLE hmsWires ADD [hmsWireFieldId]  BIGINT	;

	ALTER TABLE [dbo].[hmsWires]  WITH CHECK ADD CONSTRAINT [FK_hmsWires_hmsWireFieldId] 
		FOREIGN KEY([hmsWireFieldId]) REFERENCES [dbo].[hmsWireFields] (hmsWireFieldId)
END
GO

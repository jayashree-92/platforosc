use HM_WIRES;

IF NOT EXISTS(SELECT * FROM hmsWireTransferTypeLKup WHERE TransferType = 'Notice to Fund')
BEGIN
SET IDENTITY_INSERT hmsWireTransferTypeLKup ON
	INSERT INTO hmsWireTransferTypeLKup ([WireTransferTypeId],[TransferType]) VALUES (6,'Notice to Fund');
SET IDENTITY_INSERT hmsWireTransferTypeLKup OFF
END
GO



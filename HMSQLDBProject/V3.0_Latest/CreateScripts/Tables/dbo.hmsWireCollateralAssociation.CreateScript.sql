IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireCollateralAssociation')
BEGIN
	CREATE TABLE [dbo].[hmsWireCollateralAssociation]
	(
		[hmsWireCollateralAssociationId] BIGINT NOT NULL IDENTITY(1,1),
		[hmsWireId] BIGINT NOT NULL,
		[dmaCashCollateralId] BIGINT NOT NULL,
		[AgreedMovementType] VARCHAR(30) NOT NULL,
		CONSTRAINT hmsWireCollateralAssociation_PK PRIMARY KEY NONCLUSTERED (hmsWireCollateralAssociationId),
		CONSTRAINT FK_hmsWireCollateralAssociation_hmsWires_hmsWireId FOREIGN KEY ([hmsWireId]) REFERENCES hmsWires(hmsWireId)
	);
	
END
GO

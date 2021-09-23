USE [HM_WIRES]
GO

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireInterestAssociation')
BEGIN
	CREATE TABLE [dbo].[hmsWireInterestAssociation]
	(
		[hmsWireInterestAssociationId] BIGINT NOT NULL IDENTITY(1,1),
		[hmsWireId] BIGINT NOT NULL,
		[dmaInterestReportEodDataId] BIGINT NOT NULL

		CONSTRAINT hmsWireInterestAssociation_PK PRIMARY KEY NONCLUSTERED (hmsWireInterestAssociationId),
		CONSTRAINT FK_hmsWireInterestAssociation_hmsWires_hmsWireId FOREIGN KEY ([hmsWireId]) REFERENCES hmsWires(hmsWireId)
	);
	
END
GO


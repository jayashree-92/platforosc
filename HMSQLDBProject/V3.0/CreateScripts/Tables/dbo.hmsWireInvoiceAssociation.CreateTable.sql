IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsWireInvoiceAssociation')
BEGIN
	CREATE TABLE [dbo].[hmsWireInvoiceAssociation]
	(
		[hmsWireInvoiceAssociationId] BIGINT NOT NULL IDENTITY(1,1),
		[hmsWireId] BIGINT NOT NULL,
		[InvoiceId] BIGINT NOT NULL

		CONSTRAINT hmsWireInvoiceAssociation_PK PRIMARY KEY NONCLUSTERED (hmsWireInvoiceAssociationId),
		CONSTRAINT FK_hmsWireInvoiceAssociation_hmsWires_hmsWireId FOREIGN KEY ([hmsWireId]) REFERENCES hmsWires(hmsWireId)
	);
	
END
GO

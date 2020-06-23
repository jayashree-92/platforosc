use HM_WIRES

IF NOT EXISTS(SELECT * FROM SYS.objects WHERE NAME = 'UQ_hmsWireInvoiceAssociation_InvoiceId')
BEGIN
ALTER TABLE hmsWireInvoiceAssociation ADD CONSTRAINT UQ_hmsWireInvoiceAssociation_InvoiceId UNIQUE(InvoiceId)
END
GO


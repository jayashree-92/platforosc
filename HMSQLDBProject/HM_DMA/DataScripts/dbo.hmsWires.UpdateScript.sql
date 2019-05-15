IF EXISTS(SELECT 5 FROM hmsWires WHERE WireTransferTypeId > 3)
BEGIN
UPDATE hmsWires SET WireTransferTypeId = 3 WHERE WireTransferTypeId > 3
END
GO

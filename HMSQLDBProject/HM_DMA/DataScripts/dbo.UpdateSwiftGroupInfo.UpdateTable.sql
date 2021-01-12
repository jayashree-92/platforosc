IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsSwiftGroup' AND COLUMN_NAME = 'RequestedBy')
BEGIN
	UPDATE TGT SET TGT.RequestedBy  = SRC.intLoginId FROM hmsSwiftGroup TGT INNER JOIN HM.DBO.hLoginRegistration SRC ON TGT.RecCreatedBy = SRC.varLoginID
END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsSwiftGroup' AND COLUMN_NAME = 'RequestedAt')
BEGIN
	UPDATE hmsSwiftGroup SET RequestedAt  = RecCreatedAt
END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsWirePortalCutoff' AND COLUMN_NAME = 'IsApproved')
BEGIN
	IF NOT EXISTS(SELECT * FROM hmsWirePortalCutoff WHERE IsApproved =1)
	BEGIN
		UPDATE hmsWirePortalCutoff SET IsApproved  = 1
	END
END

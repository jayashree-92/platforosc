
IF  EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'hmsWires' AND COLUMN_NAME = 'ReceivingOnBoardAccountId')
	BEGIN
	IF NOT EXISTS(SELECT * FROM hmsWires Where ReceivingOnBoardAccountId IS NULL) 
	BEGIN
		UPDATE hmsWires SET ReceivingOnBoardAccountId = OnBoardSSITemplateId WHERE WireTransferTypeId =2;
	END
END

--**** the below list has to be deleted AS its violating the foreign-key relationship with SSI Template***--
--select * from hmsWires Where WireTransferTypeId =2 and OnBoardSSITemplateId is not null
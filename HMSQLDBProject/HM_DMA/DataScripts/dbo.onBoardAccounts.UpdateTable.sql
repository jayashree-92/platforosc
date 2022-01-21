USE HM_WIRES
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingAccount_bkup_V45_0' AND TABLE_SCHEMA = 'DmaBackup')
BEGIN
SELECT * INTO DmaBackup.onBoardingAccount_bkup_V45_0 FROM onBoardingAccount
END
GO

IF EXISTS(SELECT * FROM onBoardingAccount where dmaCounterpartyId =0 AND dmaAgreementOnBoardingId >0)
BEGIN
	UPDATE TGT SET TGT.dmaCounterpartyId =SRC.dmaCounterPartyOnBoardId FROM onBoardingAccount TGT
	INNER JOIN HM.HMADMIN.vw_CounterpartyAgreements SRC on TGT.dmaAgreementOnBoardingId = SRC.dmaAgreementOnBoardingId
	WHERE TGT.dmaCounterpartyId =0 AND TGT.dmaAgreementOnBoardingId >0
END
GO







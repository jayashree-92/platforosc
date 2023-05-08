USE dbmod_data_backup_db;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'onboardingAccount_Bkup_V21_0_20_May_2023' AND TABLE_SCHEMA='dbo')
BEGIN
	SELECT * INTO dbmod_data_backup_db.DBO.onboardingAccount_Bkup_V21_0_20_May_2023 FROM HM_WIRES.DBO.onboardingAccount
END

USE HM_WIRES;


UPDATE TGT SET TGT.dmaAgreementOnBoardingId = SRC.dmaAgreementOnboardingId ,TGT.AccountType = 'Agreement'
FROM onboardingAccount TGT , HM.HMADMIN.vw_CounterpartyAgreements SRC 
WHERE TGT.dmaCounterpartyId = SRC.dmaCounterpartyOnboardId AND TGT.hmFundId = SRC.FundMapId
AND TGT.AccountType = SRC.AgreementType AND TGT.IsDeleted=0 AND TGT.AccountType In ('DDA' ,'Custody') 
AND hmFundId !=0 AND dmaCounterpartyId is not null;


--UPDATE TGT SET TGT.AccountType = 'Agreement'
--FROM onboardingAccount TGT , HM.HMADMIN.vw_CounterpartyAgreements SRC 
--WHERE TGT.dmaCounterpartyId = SRC.dmaCounterpartyOnboardId AND TGT.hmFundId = SRC.FundMapId
--AND TGT.AccountType = SRC.AgreementType AND TGT.IsDeleted=0 AND TGT.AccountType In ('DDA' ,'Custody') 
--AND hmFundId !=0 AND dmaCounterpartyId is not null;



--select SRC.AgreementType,TGT.AccountType FROM onboardingAccount TGT , HM.HMADMIN.vw_CounterpartyAgreements SRC 
--WHERE TGT.dmaCounterpartyId = SRC.dmaCounterpartyOnboardId AND TGT.hmFundId = SRC.FundMapId
--AND TGT.AccountType = SRC.AgreementType AND TGT.IsDeleted=0 AND TGT.AccountType In ('DDA' ,'Custody') 
--AND hmFundId !=0 AND dmaCounterpartyId is not null
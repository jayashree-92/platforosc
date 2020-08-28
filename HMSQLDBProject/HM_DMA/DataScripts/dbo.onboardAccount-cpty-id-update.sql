
	UPDATE tgt set tgt.dmaCounterpartyId = src.dmaCounterPartyOnBoardId
	FROM HM_WIRES..onBoardingAccount tgt 
	INNER JOIN HM.HMADMIN.dmaAgreementOnBoarding src on tgt.dmaAgreementOnBoardingId = src.dmaAgreementOnBoardingId
	--WHERE tgt.AccountType='Agreement'

	--UPDATE tgt set tgt.dmaCounterpartyId = src.dmaCounterPartyOnBoardId
	--FROM HM_WIRES..onBoardingAccount tgt 
	--INNER JOIN HM.HMADMIN.dmaCounterPartyOnBoarding src on tgt.dmaCounterpartyFamilyId = src.dmaCounterpartyFamilyId
	--WHERE tgt.AccountType!='Agreement' and CounterpartyName='The Bank of New York Mellon'

	UPDATE tgt set tgt.dmaCounterpartyId = src.dmaCounterPartyOnBoardId
	FROM HM_WIRES..onBoardingAccount tgt 
	INNER JOIN HM.HMADMIN.dmaCounterPartyOnBoarding src on tgt.dmaCounterpartyFamilyId = src.dmaCounterpartyFamilyId
	WHERE tgt.AccountType!='Agreement' and tgt.dmaCounterpartyFamilyId in (select dmaCounterpartyFamilyId from HM.HMADMIN.dmaCounterPartyOnBoarding 
	group by dmaCounterpartyFamilyId 
	having count(dmaCounterpartyFamilyId )=1)

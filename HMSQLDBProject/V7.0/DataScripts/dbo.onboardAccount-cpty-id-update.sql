
	UPDATE tgt set tgt.dmaCounterpartyId = src.dmaCounterPartyOnBoardId
	FROM HM_WIRES..onBoardingAccount tgt 
	INNER JOIN HM.HMADMIN.dmaAgreementOnBoarding src on tgt.dmaAgreementOnBoardingId = src.dmaAgreementOnBoardingId
	WHERE tgt.AccountType='Agreement' and tgt.dmaAgreementOnBoardingId is not null

	UPDATE tgt set tgt.dmaCounterpartyId = src.dmaCounterPartyOnBoardId
	FROM HM_WIRES..onBoardingAccount tgt 
	INNER JOIN HM.HMADMIN.dmaCounterPartyOnBoarding src on tgt.dmaCounterpartyFamilyId = src.dmaCounterpartyFamilyId
	WHERE tgt.AccountType!='Agreement' and tgt.dmaCounterpartyFamilyId in (select dmaCounterpartyFamilyId from HM.HMADMIN.dmaCounterPartyOnBoarding 
	group by dmaCounterpartyFamilyId 
	having count(dmaCounterpartyFamilyId )=1)
	
	UPDATE tgt set tgt.dmaCounterpartyId = src.dmaCounterPartyOnBoardId
	FROM HM_WIRES..onBoardingAccount tgt 
	INNER JOIN HM.HMADMIN.dmaCounterPartyOnBoarding src on tgt.dmaCounterpartyFamilyId = src.dmaCounterpartyFamilyId
	WHERE tgt.AccountType!='Agreement' and CounterpartyName in ('BNP Paribas','Merrill Lynch Professional Clearing Corp.') 
	
	
	--///*********CHECK FOR MISSING COUNTERPARTY**********////
	
select tgt.dmaCounterpartyId,src.dmaCounterPartyOnBoardId,cptf.CounterpartyFamily,tgt.*
	FROM HM_WIRES..onBoardingAccount tgt 
	INNER JOIN HM.HMADMIN.dmaCounterPartyOnBoarding src on tgt.dmaCounterpartyFamilyId = src.dmaCounterpartyFamilyId
	INNER JOIN HM.HMADMIN.dmaCounterpartyFamily cptf on tgt.dmaCounterpartyFamilyId = cptf.dmaCounterpartyFamilyId
	WHERE tgt.dmaCounterpartyId is null and tgt.IsDeleted=0


	

USE HM_WIRES;

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'onBoardingAccount' AND COLUMN_NAME = 'TopLevelManagerAccountNumber')
BEGIN
	ALTER TABLE onBoardingAccount ADD TopLevelManagerAccountNumber VARCHAR(100)
END

IF EXISTS (SELECT * FROM SYSOBJECTS WHERE type = 'V' AND name = 'vw_FundAccounts')
BEGIN
DROP VIEW [dbo].[vw_FundAccounts]
END

IF NOT EXISTS (SELECT * FROM SYSOBJECTS WHERE type = 'V' AND name = 'vw_FundAccounts')
BEGIN
                DECLARE @query NVARCHAR(MAX);

                SET @query  = 'CREATE VIEW [dbo].[vw_FundAccounts]
				AS
					SELECT onBoardingAccountId,acc.dmaAgreementOnBoardingId,dmaAgreementTypeId,typ.AgreementType,
			CASE WHEN acc.dmaAgreementOnBoardingId >0 THEN	CONCAT(typ.AgreementType,'' between '',LFund.LegalFundName,'' and '',cp.CounterpartyName) ELSE NULL END AS AgreementLongName,  
			CASE WHEN acc.dmaAgreementOnBoardingId >0 THEN	CONCAT(typ.AgreementType,'' - '',LFund.LegalFundName,'' - '',cp.CounterpartyShortCode) ELSE NULL END AS AgreementShortName,  
				acc.AccountType,acc.onBoardingAccountStatus AS ApprovalStatus,
				acc.hmFundId,
				CASE WHEN(F.ShortFundName IS NOT NULL) THEN F.ShortFundName ELSE LFund.LegalFundName END AS [ShortFundName],
				LFund.LegalFundName,
				acc.dmaCounterpartyId, cp.CounterpartyName,cpf.CounterpartyFamily,acc.AccountName, 
			CASE WHEN LEN(acc.FFCNumber) >0 THEN acc.FFCNumber ELSE acc.UltimateBeneficiaryAccountNumber  END AS AccountNumber,
				acc.FFCNumber,acc.UltimateBeneficiaryAccountNumber,acc.MarginAccountNumber,acc.AssociatedCustodyAcctNumber,acc.TopLevelManagerAccountNumber,acc.Currency,acc.AccountPurpose,acc.AccountStatus,acc.AuthorizedParty,
				acc.[Description],acc.TickerorISIN,acc.CashSweep,acc.CashSweepTime,acc.CashSweepTimeZone
				FROM onBoardingAccount acc
				LEFT JOIN HM.DBO.Fund F  WITH(NOLOCK) ON F.FundID = acc.hmFundId
				LEFT JOIN HM.DBO.LegalFund LFund  WITH(NOLOCK) ON LFund.LegalFundID = F.LegalFundID  
				LEFT JOIN HM.HMADMIN.dmaAgreementOnBoarding agrmt  WITH(NOLOCK)on agrmt.dmaAgreementOnBoardingId = acc.dmaAgreementOnBoardingId
				LEFT JOIN HM.HMADMIN.dmaAgreementTypes typ  WITH(NOLOCK) on agrmt.AgreementTypeId = typ.dmaAgreementTypeId 
				LEFT JOIN HM.HMADMIN.dmaCounterPartyOnBoarding cp WITH(NOLOCK) ON cp.dmaCounterPartyOnBoardId = acc.dmaCounterpartyId
				LEFT JOIN HM.HMADMIN.dmaCounterpartyFamily cpf WITH(NOLOCK) ON cpf.dmaCounterpartyFamilyId = cp.dmaCounterPartyFamilyId  
				WHERE acc.IsDeleted =0 AND acc.hmFundId !=0'
				
	EXEC sp_executesql @query ;
END
GO

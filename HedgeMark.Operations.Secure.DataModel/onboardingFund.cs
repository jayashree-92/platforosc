//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HedgeMark.Operations.Secure.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class onboardingFund
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public onboardingFund()
        {
            this.dmaFundOnBoardPermissions = new HashSet<dmaFundOnBoardPermission>();
            this.onboardingContactFundMaps = new HashSet<onboardingContactFundMap>();
            this.onboardingSubAdvisorFundMaps = new HashSet<onboardingSubAdvisorFundMap>();
            this.onboardingSubAdvisorFundMaps1 = new HashSet<onboardingSubAdvisorFundMap>();
        }
    
        public long dmaFundOnBoardId { get; set; }
        public long dmaClientOnBoardId { get; set; }
        public string LegalFundName { get; set; }
        public string FundShortName { get; set; }
        public Nullable<System.DateTime> DateMandated { get; set; }
        public Nullable<System.DateTime> OriginalTargetLaunchDate { get; set; }
        public Nullable<System.DateTime> RevisedLaunchedDate { get; set; }
        public Nullable<System.DateTime> ActualLaunchDate { get; set; }
        public Nullable<System.DateTime> TradeStartDate { get; set; }
        public Nullable<System.DateTime> CanceledDate { get; set; }
        public Nullable<System.DateTime> LastNAVDate { get; set; }
        public string ReasonForDelay { get; set; }
        public string LaunchStatus { get; set; }
        public string LaunchType { get; set; }
        public string EstimatedLaunchAmount { get; set; }
        public string EstimatedRevenue { get; set; }
        public string ActualLaunchAmount { get; set; }
        public string RevenueBasedLaunchedAssets { get; set; }
        public string Currency { get; set; }
        public string TypeofConversion { get; set; }
        public string ScheduleCallDiscussConversionManager { get; set; }
        public string DiscussManagerHighwatermark { get; set; }
        public string DiscussManagerCrystalization { get; set; }
        public string CashGoBacktoInvestors { get; set; }
        public string TransferingPostions { get; set; }
        public string AdditionalNewInvestments { get; set; }
        public string InKindSubscriptionorRedemptionFormOptions { get; set; }
        public string InKindSubscriptionorRedemptionForm { get; set; }
        public string BoardResolutiontoApproveOptions { get; set; }
        public string BoardResolutiontoApprove { get; set; }
        public Nullable<long> ManagerId { get; set; }
        public Nullable<long> StrategyId { get; set; }
        public string FundCID { get; set; }
        public string TaxID { get; set; }
        public string GIIN { get; set; }
        public string LEI { get; set; }
        public string NFAStatus { get; set; }
        public string NFAPoolID { get; set; }
        public string ERISAStatus { get; set; }
        public string ERISAFunds { get; set; }
        public string CIMARegistration { get; set; }
        public string SubRedFrequency { get; set; }
        public string HedgeMarkAccessPlatform { get; set; }
        public string JoindertoAccessPaltformAgreement { get; set; }
        public string CDDClassificationReason { get; set; }
        public string Notes { get; set; }
        public string FormDResponsibility { get; set; }
        public string FATCAResponsibility { get; set; }
        public string FATCAResponsibilityNotes { get; set; }
        public string FBARApplicability { get; set; }
        public string FBARApplicabilityNotes { get; set; }
        public string HedgeMarkVolckerSponsorshipStatus { get; set; }
        public string CoveredFundStatus { get; set; }
        public string NonBankAffiliateStatus { get; set; }
        public string AMLROorMLROorDeputy { get; set; }
        public string PPOCorPPOCChangeNotifier { get; set; }
        public string FundAdministratorsforPlatform { get; set; }
        public string CustodianorBankforSubscriptions { get; set; }
        public string Auditor { get; set; }
        public Nullable<long> FundStructureId { get; set; }
        public Nullable<long> DomicileId { get; set; }
        public string FundCorporateStructure { get; set; }
        public string GeneralPartner { get; set; }
        public string DirectorsoftheGP { get; set; }
        public string ManagingMemberorManager { get; set; }
        public string Directors { get; set; }
        public string AIFM { get; set; }
        public string Depositary { get; set; }
        public string ShareClasses { get; set; }
        public string ResponsiblePartyforCashManagement { get; set; }
        public string FundlevelDiscussionWithManager { get; set; }
        public string PlatformAgreementsforCashManagementAgreement { get; set; }
        public string CashManagementApproachforExcessCashBrokers { get; set; }
        public string CashManagementApproachLevel { get; set; }
        public string CashManagementApproachMarginRule { get; set; }
        public string CashManagementApproachforExcessCashCustodian { get; set; }
        public string EstablishMethodNotificationProxyVoting { get; set; }
        public string ProxyVotingThirdPartyCompanyName { get; set; }
        public string EstablishMethodNotificationClassAction { get; set; }
        public string ClassActionThirdPartyCompanyName { get; set; }
        public string IsTaxlotAccountingMethodPlatformorFundSpecific { get; set; }
        public Nullable<long> TaxlotAccountingMethodId { get; set; }
        public Nullable<long> FundHolidayScheduleId { get; set; }
        public string ResponsiblePartySendingNAV { get; set; }
        public string ResponsiblePartySendingNAVAdmin { get; set; }
        public string NAVFrequency { get; set; }
        public string TimingofNAVRelease { get; set; }
        public string ClientSignoffNAV { get; set; }
        public string ManagerSignoffNAV { get; set; }
        public string AccountingStandard { get; set; }
        public string AuditedFinancialsRequired { get; set; }
        public string AuditedFinancialsRequiredPlatform { get; set; }
        public string AuditedFinancialsRequiredFund { get; set; }
        public string FinanicalYearEndDate { get; set; }
        public Nullable<System.DateTime> FinanicalYearEndDatePlatform { get; set; }
        public string FinanicalYearEndDateFund { get; set; }
        public string BookInterestIncome { get; set; }
        public string AdminGroupEmail { get; set; }
        public string ManagementFee { get; set; }
        public string PerformanceFees { get; set; }
        public Nullable<long> AccountingMethodologyId { get; set; }
        public string InvestmentManagerEligibleReimbursement { get; set; }
        public string InvestmentManagerEligibleReimbursementNotes { get; set; }
        public string AdminServiceLevelDescription { get; set; }
        public string EstimatesRequiredInvestors { get; set; }
        public string EstimatesRequiredInvestorsFrequency { get; set; }
        public string EstimatesRequiredInvestorsType { get; set; }
        public Nullable<System.DateTime> EstimatesRequiredInvestorsDeliveryDate { get; set; }
        public string EstimatesRequiredInvestorsNotes { get; set; }
        public string AdminMethodforDeliveryofNAV { get; set; }
        public string ClientNAVDistributionList { get; set; }
        public Nullable<int> NumberofDecimalPlacesforNAV { get; set; }
        public Nullable<int> NumberofDecimalPlacesforShares { get; set; }
        public string EstablishCPOReportingProtocols { get; set; }
        public string ConfirmProtocolApprovingSubscriptionsRedemptions { get; set; }
        public string EstablishTemplateCapitalActivityTrackerandTiming { get; set; }
        public string ProtocolNotifyingManagersCapitalActivityorTradingLevel { get; set; }
        public string RequirementTrackERISAInvestors { get; set; }
        public string TalkaboutProcessFlowofInformationonHedging { get; set; }
        public string SetUpComplianceandSurveillanceModule { get; set; }
        public string SetClientupwithAccesstoHMcom { get; set; }
        public string SetupMonitoring { get; set; }
        public string FileNameofRiskFileHM { get; set; }
        public string GenevaCode { get; set; }
        public string Status { get; set; }
        public string StatusComments { get; set; }
        public Nullable<long> FundMapId { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string FundChecklistView { get; set; }
        public string AIFStatus { get; set; }
        public string CPO { get; set; }
        public string RegisterAddress { get; set; }
        public string StructureChart { get; set; }
        public string BusinessDay { get; set; }
        public Nullable<long> FundAdministrator { get; set; }
        public string EstimatedLaunchCurrency { get; set; }
        public Nullable<System.DateTime> TerminationDate { get; set; }
        public string CDDClassification { get; set; }
        public Nullable<long> HMDescriptionId { get; set; }
        public string ClientRiskCSPrimary { get; set; }
        public string ClientRiskCSSecondaryA { get; set; }
        public string ClientRiskCSSecondaryB { get; set; }
        public string MonthEndValuation { get; set; }
        public string CPONameText { get; set; }
        public string ConfirmwithOpsTeamthesweepisworkingproperly { get; set; }
        public string AMLDueDiligenceforHMSAccessPlatform { get; set; }
        public string PerformanceAllocation { get; set; }
        public string SpecialLimitedPartner { get; set; }
        public string FileForm1099MISC { get; set; }
        public string DealingDay { get; set; }
        public Nullable<int> onboardHolidayCalendarId { get; set; }
        public string IntroductoryCall { get; set; }
        public string RegisteredwithSEC { get; set; }
        public string RegisteredwithNFA { get; set; }
        public string ManagerERISAStatus { get; set; }
        public string ManagerJurisdiction { get; set; }
        public string ManagerPrinicpalPlaceofBusiness { get; set; }
        public string b2letter { get; set; }
        public string ManagerNDAwithHedgeMark { get; set; }
        public string AuthorizedSignatoryList { get; set; }
        public string TaxDoc { get; set; }
        public string SamplePortfolio { get; set; }
        public string ReferenceFundPPM { get; set; }
        public string AllocationPolicy { get; set; }
        public string Other { get; set; }
        public string DoddFrankReport { get; set; }
        public string TradeFileforAdmin { get; set; }
        public string SFTPSetupwithAdmin { get; set; }
        public string HoldingsReportincludingestimatedmarketvaluesFrequency { get; set; }
        public string HoldingsReportincludingestimatedmarketvaluesDeliveryMethod { get; set; }
        public string GROSSRateofReturnFrequency { get; set; }
        public string GROSSRateofReturnDeliveryMethod { get; set; }
        public string PLReportReflectingFrequency { get; set; }
        public string PLReportReflectingFrequencyDeliveryMethod { get; set; }
        public string RegisteredwithSECNumber { get; set; }
        public string RegisteredwithNFANumber { get; set; }
        public string AUMNotes { get; set; }
        public string DefaultHmOnboarding { get; set; }
        public string DefaultHmAccounting { get; set; }
        public string DefaultHmOps { get; set; }
        public string DefaultHmStructuring { get; set; }
        public string HmRiskClientServiceAndAnalytics { get; set; }
        public Nullable<System.DateTime> TransferredOffDate { get; set; }
        public string AddtoFundCount { get; set; }
        public string RegisteredwithNFAExempt { get; set; }
        public string RegisteredwithSECExempt { get; set; }
        public string HasFundBeenPermissionedOnHMOpsWebsite { get; set; }
        public string HasNAVReviewChecklistBeenSetUp { get; set; }
        public string HasNAVReviewChecklistBeenCustomizedToIncludeFundSpecifictask { get; set; }
        public string HasNAVReviewBeenAssignedToRespectiveAccountant { get; set; }
        public string HasThisFundBeenSetUpInNewReposrtingModule { get; set; }
        public string AreWeRecievingRequiredDataForThisModuleFromTheAdmin { get; set; }
        public string DidWeCommunicateToIndiaTeamToVerifyTheNAVDataOnAMonthlyBasis { get; set; }
        public string HasTheIMASAAbeenReviewedByResponsibleAccountant { get; set; }
        public string HaveWeSentTheIMASAAToAdminForTheirReview { get; set; }
        public string HaveWePutTogetherExpenseAccrualFileAndAgreedUponAllAsset { get; set; }
        public string HaveWeUpdatedOurInvoicePaymentProcedure { get; set; }
        public string HaveWeSetUpAllTheFeeTypesOnExpenseModule { get; set; }
        public string HasTheInvestmentManagerBeenNotifiedOfTheInitialCapital { get; set; }
        public string IsTheFundSetUpInTheCapitalActivityModule { get; set; }
        public string HasTheInitialSubscriptionBeenApprovedInTheCapitalActivityModule { get; set; }
        public string IsThisFundAComingledFundOrSingleInvestorFund { get; set; }
        public string DoesTheFundHaveOnshoreAndOffshoreInvestors { get; set; }
        public string IsThisFundOwnedByOnshoreInvetorsOrOffshoreInvestors { get; set; }
        public string IfTheFundIsOwnedByEitherThenDidWeSendInitialTaxDocs { get; set; }
        public string IfTheFundIsOwnedByBothOnshoreAndOffshoreInvestors { get; set; }
        public string IfYesThenDidProvideAllTheTaxDocumentsAlongWithWithholdingTaxStatements { get; set; }
        public string DidWeWalkIndiaThroughTheNewFundAndWhsProcess { get; set; }
        public string DoesThisFundHaveOvernightSweepSetUpAtBNYM { get; set; }
        public string WorkWithLiquidityDirectCorporateTaxDeptToEnsureTaxDocProvided { get; set; }
        public string IsThisPricingPolicyForAnExisitingClient { get; set; }
        public string HaveWeUpdatedThePricingPolicyToIncludeTheFundOnTheAppendix { get; set; }
        public string IsThisPricingPolicyForANewClient { get; set; }
        public string DidWeWorkOnANewPricingPolicy { get; set; }
        public string HasStructuringTeamReviewedAndApprovedThePricingPolicy { get; set; }
        public string HasHMAcctingReviewedAndApprovedThePricingPolicy { get; set; }
        public string HasDirectorsReviewedAndApprovedThePricingPolicy { get; set; }
        public string HasClientReviewedAndApprovedThePricingPolicy { get; set; }
        public string HasAdminReviewedAndApprovedThePricingPolicy { get; set; }
        public string IsThisForAnExisitingClientBondAccretion { get; set; }
        public string UpdatedTheBondAccretionPolicyToIncludeTheFundOnTheAppendix { get; set; }
        public string IsThisForaNewClientBondAccretionPolicy { get; set; }
        public string DidsWeWorkOnaNewPolicyForBondAccretion { get; set; }
        public string HasClientReviewedAndApprovedTheBondAccretionPolicy { get; set; }
        public string HasAdminReviewedAndApprovedTheBondAccretionPolicy { get; set; }
        public string DidWePrepareCashRecForThe1stMonthEnd { get; set; }
        public string After1stMonthSetUpTheCashRecInTheCashRecModule { get; set; }
        public string HasTheCashRecBeenSetUpOnCashRecModule { get; set; }
        public string ConfirmWithOpsAnalystThatPositionReconciliationHasBeenSetUpOnHM { get; set; }
        public string FundCustodian { get; set; }
        public string SetUpClientAndManagerWithReportingAccess { get; set; }
        public string OpenBankAccount { get; set; }
        public string HasInitialFilingBeenCompleted { get; set; }
        public Nullable<System.DateTime> LEIRenewalDate { get; set; }
        public string LegalCounsel { get; set; }
        public string LegalCounselTradingAgreements { get; set; }
        public string LegalCounselFundorLocal { get; set; }
        public string TaxAdvisor { get; set; }
        public string AuthorizedTraderList { get; set; }
        public string OCRID { get; set; }
        public string TargetLaunchType { get; set; }
        public string MarkitwireBIC { get; set; }
        public string DTCCID { get; set; }
        public string FundThreshold { get; set; }
        public string TotalEquityThreshold { get; set; }
        public Nullable<double> TotalEquityThresholdValue { get; set; }
        public string MarginRequirementThreshold { get; set; }
        public Nullable<double> MarginRequirementThresholdValue { get; set; }
        public string LastNAVDateNotes { get; set; }
        public string FeeComplexityTier { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dmaFundOnBoardPermission> dmaFundOnBoardPermissions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onboardingContactFundMap> onboardingContactFundMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onboardingSubAdvisorFundMap> onboardingSubAdvisorFundMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onboardingSubAdvisorFundMap> onboardingSubAdvisorFundMaps1 { get; set; }
    }
}

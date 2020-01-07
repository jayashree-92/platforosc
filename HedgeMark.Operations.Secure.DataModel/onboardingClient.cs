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
    
    public partial class onboardingClient
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public onboardingClient()
        {
            this.onboardingFunds = new HashSet<onboardingFund>();
        }
    
        public long dmaClientOnBoardId { get; set; }
        public string ClientLegalEntityName { get; set; }
        public string ClientShortName { get; set; }
        public string ClientType { get; set; }
        public string ClientCID { get; set; }
        public string CDDClassification { get; set; }
        public string CDDClassificationReason { get; set; }
        public string OnboardingCallsFrequency { get; set; }
        public string OnboardingCallsDays { get; set; }
        public Nullable<System.TimeSpan> OnboardingCallsTime { get; set; }
        public string IsClientCurrentCustodyClientofBNYM { get; set; }
        public string IsHegdeMarkAccessPlatformClient { get; set; }
        public string FormDResponsibility { get; set; }
        public string FATCAResponsibility { get; set; }
        public string FATCAResponsibilityNotes { get; set; }
        public string HedgeMarkVolckerSponsorshipStatus { get; set; }
        public string CoveredFundStatus { get; set; }
        public string AMLROorMLROorDeputy { get; set; }
        public string PPOCorPPOCChangeNotifier { get; set; }
        public string Notes { get; set; }
        public string PlatformName { get; set; }
        public string FundNamingConvention { get; set; }
        public Nullable<long> FundStructureId { get; set; }
        public Nullable<long> MasterFundDomicileId { get; set; }
        public string MasterFundCorporateStructure { get; set; }
        public string GeneralPartner { get; set; }
        public string DirectorsoftheGP { get; set; }
        public string ManagingMemberorManager { get; set; }
        public string Directors { get; set; }
        public string AIFM { get; set; }
        public string Depositary { get; set; }
        public Nullable<long> FeederFund1DomicileId { get; set; }
        public string FeederFund1CorporateStructure { get; set; }
        public Nullable<long> FeederFund2DomicileId { get; set; }
        public string FeederFund2CorporateStructure { get; set; }
        public Nullable<long> LegalRoleId { get; set; }
        public string EntityName { get; set; }
        public string FundAdministratorsforPlatform { get; set; }
        public string LegalCounsel { get; set; }
        public string LegalCounselTradingAgreements { get; set; }
        public string LegalCounselFundorLocal { get; set; }
        public string CustodianorBankforSubscriptions { get; set; }
        public string Auditor { get; set; }
        public string TaxAdvisor { get; set; }
        public string CPOName { get; set; }
        public string PlatformAcceptERISAFunds { get; set; }
        public string ERISAFunds { get; set; }
        public string OfferingDocumentsRequired { get; set; }
        public string FundRegisterwithCIMA { get; set; }
        public string NumberofCashAccountsNeededforeachFund { get; set; }
        public string ShareClasses { get; set; }
        public string PerformanceFees { get; set; }
        public Nullable<long> AccountingMethodologyId { get; set; }
        public string ResponsiblePartyforCashManagement { get; set; }
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
        public string ResponsiblePartySendingNAV { get; set; }
        public string ResponsiblePartySendingNAVAdmin { get; set; }
        public string FileForm1099MISC { get; set; }
        public string FileForm1099MISCHMorAdmin { get; set; }
        public string EstimatesRequiredInvestors { get; set; }
        public string EstimatesRequiredInvestorsFrequency { get; set; }
        public string EstimatesRequiredInvestorsType { get; set; }
        public Nullable<System.DateTime> EstimatesRequiredInvestorsDeliveryDate { get; set; }
        public string EstimatesRequiredInvestorsNotes { get; set; }
        public string NAVFrequency { get; set; }
        public string TimingofNAVRelease { get; set; }
        public string ClientSignoffNAV { get; set; }
        public string ManagerSignoffNAV { get; set; }
        public string AdminMethodforDeliveryofNAV { get; set; }
        public string ClientNAVDistributionList { get; set; }
        public Nullable<int> NumberofDecimalPlacesforNAV { get; set; }
        public Nullable<int> NumberofDecimalPlacesforShares { get; set; }
        public string AccountingStandard { get; set; }
        public string EstablishCPOReportingProtocols { get; set; }
        public string ConfirmProtocolApprovingSubscriptionsRedemptions { get; set; }
        public string EstablishTemplateCapitalActivityTrackerandTiming { get; set; }
        public string ProtocolNotifyingManagersCapitalActivityorTradingLevel { get; set; }
        public string RequirementTrackERISAInvestors { get; set; }
        public string TalkaboutProcessFlowofInformationonHedging { get; set; }
        public string AuditedFinancialsRequired { get; set; }
        public string AuditedFinancialsRequiredPlatform { get; set; }
        public string AuditedFinancialsRequiredFund { get; set; }
        public string FinanicalYearEndDate { get; set; }
        public Nullable<System.DateTime> FinanicalYearEndDatePlatform { get; set; }
        public Nullable<System.DateTime> FinanicalYearEndDateFund { get; set; }
        public string InvestmentGuidelineBreachProtocol { get; set; }
        public string DistributionlistInvestmentGuidelineMonitoring { get; set; }
        public string ClientUnderstandReportingNeeds { get; set; }
        public string DetermineRiskReportingInvestors { get; set; }
        public string ClientUsersTraining { get; set; }
        public string SetupTimeTrainingSystem { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> TargetLaunchDate { get; set; }
        public string RiskCSPrimary { get; set; }
        public string RiskCSSecondaryA { get; set; }
        public string RiskCSSecondaryB { get; set; }
        public string NoOfFeeders { get; set; }
        public string GeneralPartnerFeeder1 { get; set; }
        public string GeneralPartnerFeeder2 { get; set; }
        public string DirectorsOfGPFeeder1 { get; set; }
        public string DirectorsOfGPFeeder2 { get; set; }
        public string ManagingMemberFeeder1 { get; set; }
        public string ManagingMemberFeeder2 { get; set; }
        public string DirectorsFeeder1 { get; set; }
        public string DirectorsFeeder2 { get; set; }
        public string AIFMFeeder1 { get; set; }
        public string AIFMFeeder2 { get; set; }
        public string DepositoryFeeder1 { get; set; }
        public string DepositoryFeeder2 { get; set; }
        public string QPAMDirectionLetter { get; set; }
        public string FormDResponsiblePartyForFiling { get; set; }
        public string OfferingDocumentsType { get; set; }
        public string InvestorAudience { get; set; }
        public string CPONameValue { get; set; }
        public string ExcessCashManagementForBrokerValue { get; set; }
        public string ShareClassNotes { get; set; }
        public string ResponsiblePartySendingNAVHM { get; set; }
        public string FieldBusinessDay { get; set; }
        public string MonthEndValuation { get; set; }
        public string ClientChecklistView { get; set; }
        public string AgreementTemplateRelationship { get; set; }
        public string SweepAtCustodian { get; set; }
        public string EstimatesRequiredInvestorDeliveryDate { get; set; }
        public string FinanicalYearEndPlatformDate { get; set; }
        public string FinanicalYearEndFundDate { get; set; }
        public string OtherDocumentNotes { get; set; }
        public string CashAccountForMasterFund { get; set; }
        public string CashAccountForFeeder1Fund { get; set; }
        public string CashAccountForFeeder2Fund { get; set; }
        public string TaxReturnPFIC { get; set; }
        public string ProjectLaunchDate { get; set; }
        public string DealingDay { get; set; }
        public string DefaultHmOnboarding { get; set; }
        public string DefaultHmAccounting { get; set; }
        public string DefaultHmOps { get; set; }
        public string DefaultHmStructuring { get; set; }
        public string HmRiskClientServiceAndAnalytics { get; set; }
        public string Tier { get; set; }
        public string ConfirmProtocolForSubscriptionRedemption { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onboardingFund> onboardingFunds { get; set; }
    }
}

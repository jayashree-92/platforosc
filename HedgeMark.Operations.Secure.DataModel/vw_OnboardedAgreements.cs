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
    
    public partial class vw_OnboardedAgreements
    {
        public long dmaAgreementOnBoardingId { get; set; }
        public long dmaFundOnBoardId { get; set; }
        public long dmaClientOnBoardId { get; set; }
        public Nullable<int> AgreementTypeId { get; set; }
        public Nullable<int> AgreementStatusId { get; set; }
        public Nullable<long> dmaOnBoardingAgreementVersionId { get; set; }
        public Nullable<long> dmaCounterPartyFamilyId { get; set; }
        public Nullable<long> dmaCounterPartyOnBoardId { get; set; }
        public string Comments { get; set; }
        public string CurrencyCode { get; set; }
        public string CreditSupportAmount { get; set; }
        public Nullable<double> ThresholdwrtFund { get; set; }
        public Nullable<double> ThresholdwrtCounterparty { get; set; }
        public Nullable<double> MTAwrtFund { get; set; }
        public Nullable<double> MTAwrtCounterparty { get; set; }
        public Nullable<double> DeliverRoundingAmount { get; set; }
        public string DeliverRoundingMethod { get; set; }
        public Nullable<double> ReturnRoundingAmount { get; set; }
        public string ReturnRoundingMethod { get; set; }
        public string ThresholdIATermsforMTM { get; set; }
        public Nullable<double> ThresholdwrtFundforMTM { get; set; }
        public Nullable<double> ThresholdwrtCounterpartyforMTM { get; set; }
        public Nullable<double> MTAwrtFundforMTM { get; set; }
        public Nullable<double> MTAwrtCounterpartyforMTM { get; set; }
        public Nullable<double> DeliverRoundingAmountforMTM { get; set; }
        public string DeliverRoundingMethodforMTM { get; set; }
        public Nullable<double> ReturnRoundingAmountforMTM { get; set; }
        public string ReturnRoundingMethodforMTM { get; set; }
        public string NotificationTimeZone { get; set; }
        public string InterestRateSource { get; set; }
        public string TransferOfInterest { get; set; }
        public string NOTIFICATION_DEADLINE { get; set; }
        public string MARGIN_DELIVERY_DEADLINE { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string HMOpsStatus { get; set; }
        public bool PBFXNetting { get; set; }
        public string InterestMethod { get; set; }
        public Nullable<bool> LegalOrOperational { get; set; }
        public string TransferTiming { get; set; }
        public string AgreementTask { get; set; }
        public Nullable<System.DateTime> ActualLaunchDate { get; set; }
        public Nullable<System.DateTime> RevisedLaunchedDate { get; set; }
        public Nullable<System.DateTime> OriginalTargetLaunchDate { get; set; }
        public Nullable<System.DateTime> TradeStartDate { get; set; }
        public string LaunchStatus { get; set; }
        public string AgreementType { get; set; }
        public string AgreementStatus { get; set; }
        public string AgreementVersion { get; set; }
        public string TargetCompletion { get; set; }
        public Nullable<System.DateTime> TargetCompletionDate { get; set; }
        public Nullable<int> RelativeDays { get; set; }
        public string RelativeOptions { get; set; }
        public string Notes { get; set; }
        public string HistoricalNotes { get; set; }
        public Nullable<System.DateTime> NotesModifiedDate { get; set; }
        public Nullable<System.DateTime> FullyExecutedDate { get; set; }
        public Nullable<System.DateTime> StatusDate { get; set; }
        public string CounterpartyFamily { get; set; }
        public string CounterpartyName { get; set; }
        public string CounterpartyShortCode { get; set; }
        public Nullable<int> hFundId { get; set; }
        public string ClientName { get; set; }
        public string LegalFundName { get; set; }
        public string ShortFundName { get; set; }
        public string AgreementLongName { get; set; }
        public string AgreementShortName { get; set; }
        public string FundManagerName { get; set; }
        public string AdminChoice { get; set; }
    }
}

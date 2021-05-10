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
    
    public partial class vw_FundAccounts
    {
        public long onBoardingAccountId { get; set; }
        public Nullable<long> dmaAgreementOnBoardingId { get; set; }
        public Nullable<int> dmaAgreementTypeId { get; set; }
        public string AgreementType { get; set; }
        public string AgreementLongName { get; set; }
        public string AgreementShortName { get; set; }
        public string AccountType { get; set; }
        public string ApprovalStatus { get; set; }
        public long hmFundId { get; set; }
        public string ShortFundName { get; set; }
        public string LegalFundName { get; set; }
        public Nullable<long> dmaCounterpartyId { get; set; }
        public string CounterpartyName { get; set; }
        public string CounterpartyFamily { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string FFCNumber { get; set; }
        public string UltimateBeneficiaryAccountNumber { get; set; }
        public string MarginAccountNumber { get; set; }
        public string Currency { get; set; }
        public string AccountPurpose { get; set; }
        public string AccountStatus { get; set; }
        public string AuthorizedParty { get; set; }
        public string Description { get; set; }
        public string TickerorISIN { get; set; }
        public string CashSweep { get; set; }
        public Nullable<System.TimeSpan> CashSweepTime { get; set; }
        public string CashSweepTimeZone { get; set; }
        public string AssociatedCustodyAcctNumber { get; set; }
        public string TopLevelManagerAccountNumber { get; set; }
        public string ClientName { get; set; }
        public string LaunchStatus { get; set; }
        public Nullable<long> dmaClientOnBoardId { get; set; }
        public Nullable<double> HoldbackAmount { get; set; }
    }
}

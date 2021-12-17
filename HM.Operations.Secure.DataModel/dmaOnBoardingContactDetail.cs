//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HM.Operations.Secure.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class dmaOnBoardingContactDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public dmaOnBoardingContactDetail()
        {
            this.onboardingContactFundMaps = new HashSet<onboardingContactFundMap>();
        }
    
        public long dmaOnBoardingContactDetailId { get; set; }
        public long dmaOnBoardingTypeId { get; set; }
        public long dmaOnBoardingEntityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public string Email { get; set; }
        public string BusinessPhone { get; set; }
        public string MobileNumber { get; set; }
        public string FaxNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP { get; set; }
        public string Country { get; set; }
        public string Website { get; set; }
        public string Notes { get; set; }
        public string ContactType { get; set; }
        public string ContactCompany { get; set; }
        public bool CapitalActivity { get; set; }
        public bool NAVSignOff { get; set; }
        public bool Collateral { get; set; }
        public bool InvoiceApproval { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public string ContactSubType { get; set; }
        public bool CounterpartyNAVReporting { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
        public bool Cash { get; set; }
        public bool Margin { get; set; }
        public bool Wires { get; set; }
        public bool NAVReview { get; set; }
        public bool GAVReview { get; set; }
        public bool InterestRate { get; set; }
        public bool TradeFiles { get; set; }
        public bool TradeBreaks { get; set; }
        public bool NAVandFees { get; set; }
        public bool MarginCommunications { get; set; }
        public bool SubscriptionRedemptionCommunications { get; set; }
        public bool TradingDocumentNoticesAndCommunications { get; set; }
        public bool Compliance { get; set; }
        public bool Trading { get; set; }
        public bool TradeFilesFTPSetUp { get; set; }
        public bool Onboarding { get; set; }
        public string ClientSubCategory { get; set; }
        public bool ComplainceCert { get; set; }
        public string FundIds { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onboardingContactFundMap> onboardingContactFundMaps { get; set; }
    }
}

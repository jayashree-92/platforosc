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
    
    public partial class onBoardingAccount
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public onBoardingAccount()
        {
            this.hmsAccountCallbacks = new HashSet<hmsAccountCallback>();
            this.hmsWires = new HashSet<hmsWire>();
            this.hmsWires1 = new HashSet<hmsWire>();
            this.onBoardingAccountDocuments = new HashSet<onBoardingAccountDocument>();
            this.onBoardingAccountModuleAssociations = new HashSet<onBoardingAccountModuleAssociation>();
            this.onBoardingAccountSSITemplateMaps = new HashSet<onBoardingAccountSSITemplateMap>();
        }
    
        public long onBoardingAccountId { get; set; }
        public Nullable<long> dmaAgreementOnBoardingId { get; set; }
        public string AccountName { get; set; }
        public string UltimateBeneficiaryAccountNumber { get; set; }
        public string BeneficiaryType { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public string IntermediaryType { get; set; }
        public string IntermediaryAccountNumber { get; set; }
        public string UltimateBeneficiaryType { get; set; }
        public string FFCName { get; set; }
        public string FFCNumber { get; set; }
        public string Reference { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string AuthorizedParty { get; set; }
        public string CashInstruction { get; set; }
        public string CashSweep { get; set; }
        public Nullable<System.TimeSpan> CashSweepTime { get; set; }
        public string CashSweepTimeZone { get; set; }
        public string ContactType { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }
        public string onBoardingAccountStatus { get; set; }
        public string StatusComments { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string AccountType { get; set; }
        public Nullable<long> dmaCounterpartyFamilyId { get; set; }
        public string AccountPurpose { get; set; }
        public string AccountStatus { get; set; }
        public Nullable<double> HoldbackAmount { get; set; }
        public string SweepComments { get; set; }
        public string AssociatedCustodyAcct { get; set; }
        public string PortfolioSelection { get; set; }
        public string TickerorISIN { get; set; }
        public string SweepCurrency { get; set; }
        public string UltimateBeneficiaryAccountName { get; set; }
        public string AccountModule { get; set; }
        public string ApprovedBy { get; set; }
        public long hmFundId { get; set; }
        public Nullable<long> BeneficiaryBICorABAId { get; set; }
        public Nullable<long> IntermediaryBICorABAId { get; set; }
        public Nullable<long> UltimateBeneficiaryBICorABAId { get; set; }
        public Nullable<long> WirePortalCutoffId { get; set; }
        public Nullable<long> SwiftGroupId { get; set; }
        public string AssociatedCustodyAcctNumber { get; set; }
        public Nullable<long> dmaCounterpartyId { get; set; }
        public Nullable<long> DummyClientFundId { get; set; }
        public string MarginAccountNumber { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsAccountCallback> hmsAccountCallbacks { get; set; }
        public virtual hmsSwiftGroup SwiftGroup { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsWire> hmsWires { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsWire> hmsWires1 { get; set; }
        public virtual onBoardingAccountBICorABA Beneficiary { get; set; }
        public virtual onBoardingAccountBICorABA Intermediary { get; set; }
        public virtual onBoardingAccountBICorABA UltimateBeneficiary { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onBoardingAccountDocument> onBoardingAccountDocuments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onBoardingAccountModuleAssociation> onBoardingAccountModuleAssociations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onBoardingAccountSSITemplateMap> onBoardingAccountSSITemplateMaps { get; set; }
        public virtual hmsWirePortalCutoff WirePortalCutoff { get; set; }
    }
}

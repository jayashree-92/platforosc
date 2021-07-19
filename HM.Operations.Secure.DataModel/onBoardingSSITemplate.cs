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
    
    public partial class onBoardingSSITemplate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public onBoardingSSITemplate()
        {
            this.onBoardingAccountSSITemplateMaps = new HashSet<onBoardingAccountSSITemplateMap>();
            this.onBoardingSSITemplateDocuments = new HashSet<onBoardingSSITemplateDocument>();
            this.hmsSSICallbacks = new HashSet<hmsSSICallback>();
            this.hmsWires = new HashSet<hmsWire>();
        }
    
        public long onBoardingSSITemplateId { get; set; }
        public int dmaAgreementTypeId { get; set; }
        public string TemplateName { get; set; }
        public int TemplateTypeId { get; set; }
        public long TemplateEntityId { get; set; }
        public string SSITemplateType { get; set; }
        public string ServiceProvider { get; set; }
        public string Currency { get; set; }
        public string ReasonDetail { get; set; }
        public string OtherReason { get; set; }
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
        public string SSITemplateStatus { get; set; }
        public string StatusComments { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string MessageType { get; set; }
        public string UltimateBeneficiaryAccountName { get; set; }
        public string ApprovedBy { get; set; }
        public Nullable<long> BeneficiaryBICorABAId { get; set; }
        public Nullable<long> IntermediaryBICorABAId { get; set; }
        public Nullable<long> UltimateBeneficiaryBICorABAId { get; set; }
        public System.DateTime LastUsedAt { get; set; }
        public bool IsKeyFieldsChanged { get; set; }
    
        public virtual onBoardingAccountBICorABA Beneficiary { get; set; }
        public virtual onBoardingAccountBICorABA Intermediary { get; set; }
        public virtual onBoardingAccountBICorABA UltimateBeneficiary { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onBoardingAccountSSITemplateMap> onBoardingAccountSSITemplateMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onBoardingSSITemplateDocument> onBoardingSSITemplateDocuments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsSSICallback> hmsSSICallbacks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsWire> hmsWires { get; set; }
    }
}

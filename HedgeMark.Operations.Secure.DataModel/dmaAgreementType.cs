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
    
    public partial class dmaAgreementType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public dmaAgreementType()
        {
            this.dmaAgreementOnBoardings = new HashSet<dmaAgreementOnBoarding>();
            this.OnBoardingAccountDescriptions = new HashSet<OnBoardingAccountDescription>();
            this.OnBoardingSSITemplateAccountTypes = new HashSet<OnBoardingSSITemplateAccountType>();
            this.onBoardingSSITemplates = new HashSet<onBoardingSSITemplate>();
        }
    
        public int dmaAgreementTypeId { get; set; }
        public string AgreementType { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dmaAgreementOnBoarding> dmaAgreementOnBoardings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OnBoardingAccountDescription> OnBoardingAccountDescriptions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OnBoardingSSITemplateAccountType> OnBoardingSSITemplateAccountTypes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onBoardingSSITemplate> onBoardingSSITemplates { get; set; }
    }
}

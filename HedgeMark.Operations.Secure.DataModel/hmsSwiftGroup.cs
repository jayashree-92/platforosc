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
    
    public partial class hmsSwiftGroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public hmsSwiftGroup()
        {
            this.onBoardingAccounts = new HashSet<onBoardingAccount>();
        }
    
        public long hmsSwiftGroupId { get; set; }
        public string SwiftGroup { get; set; }
        public string SendersBIC { get; set; }
        public string RecCreatedBy { get; set; }
        public Nullable<System.DateTime> RecCreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string AcceptedMessages { get; set; }
        public string Notes { get; set; }
        public Nullable<int> SwiftGroupStatusId { get; set; }
        public Nullable<long> BrokerLegalEntityId { get; set; }
    
        public virtual hmsSwiftGroupStatusLkp hmsSwiftGroupStatusLkp { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<onBoardingAccount> onBoardingAccounts { get; set; }
    }
}

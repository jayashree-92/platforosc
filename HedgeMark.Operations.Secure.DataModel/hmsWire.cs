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
    
    public partial class hmsWire
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public hmsWire()
        {
            this.hmsWireDocuments = new HashSet<hmsWireDocument>();
            this.hmsWireJobSchedules = new HashSet<hmsWireJobSchedule>();
            this.hmsWireLogs = new HashSet<hmsWireLog>();
            this.hmsWireWorkflowLogs = new HashSet<hmsWireWorkflowLog>();
            this.hmsWireInvoiceAssociations = new HashSet<hmsWireInvoiceAssociation>();
        }
    
        public long hmsWireId { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int WirePurposeId { get; set; }
        public System.DateTime ContextDate { get; set; }
        public System.DateTime ValueDate { get; set; }
        public string PaymentOrReceipt { get; set; }
        public long hmFundId { get; set; }
        public string SendingAccountNumber { get; set; }
        public string SendingPlatform { get; set; }
        public string ReceivingAccountNumber { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public long OnBoardAccountId { get; set; }
        public long OnBoardSSITemplateId { get; set; }
        public long OnBoardAgreementId { get; set; }
        public int WireMessageTypeId { get; set; }
        public int WireStatusId { get; set; }
        public System.DateTime LastModifiedAt { get; set; }
        public int LastUpdatedBy { get; set; }
        public string DeliveryCharges { get; set; }
        public int SwiftStatusId { get; set; }
        public int WireTransferTypeId { get; set; }
        public string NotesToApprover { get; set; }
        public Nullable<int> SenderInformationId { get; set; }
        public string SenderDescription { get; set; }
    
        public virtual hmsSwiftStatusLkup hmsSwiftStatusLkup { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsWireDocument> hmsWireDocuments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsWireJobSchedule> hmsWireJobSchedules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsWireLog> hmsWireLogs { get; set; }
        public virtual hmsWireMessageType hmsWireMessageType { get; set; }
        public virtual hmsWirePurposeLkup hmsWirePurposeLkup { get; set; }
        public virtual hmsWireStatusLkup hmsWireStatusLkup { get; set; }
        public virtual hmsWireTransferTypeLKup hmsWireTransferTypeLKup { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsWireWorkflowLog> hmsWireWorkflowLogs { get; set; }
        public virtual hmsWireSenderInformation hmsWireSenderInformation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hmsWireInvoiceAssociation> hmsWireInvoiceAssociations { get; set; }
    }
}

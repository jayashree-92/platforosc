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
    
    public partial class dmaInvoiceFileAttachment
    {
        public long dmaInvoiceFileAttachmentsId { get; set; }
        public long dmaInvoiceReportId { get; set; }
        public string InvoiceFileName { get; set; }
        public string InvoiceFilePath { get; set; }
        public bool IsDeleted { get; set; }
        public string RecCreatedBy { get; set; }
        public System.DateTime RecCreatedAt { get; set; }
        public string FileSource { get; set; }
    
        public virtual dmaInvoiceReport dmaInvoiceReport { get; set; }
    }
}

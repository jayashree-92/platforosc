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
    
    public partial class hmsUserAuditLog
    {
        public long hmsUserAuditLogId { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public string UserName { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string Log { get; set; }
        public Nullable<long> AssociationId { get; set; }
        public string Field { get; set; }
        public string PreviousStateValue { get; set; }
        public string ModifiedStateValue { get; set; }
        public bool IsLogFromOps { get; set; }
    }
}

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
    
    public partial class hmsWireLog
    {
        public long hmsWireLogId { get; set; }
        public long hmsWireId { get; set; }
        public long hmsWireWorkflowLogId { get; set; }
        public int hmsWireLogTypeId { get; set; }
        public int WireMessageTypeId { get; set; }
        public string SwiftMessage { get; set; }
        public string AdditionalDetails { get; set; }
        public System.DateTime RecCreatedAt { get; set; }
    
        public virtual hmsWireLogTypeLkup hmsWireLogTypeLkup { get; set; }
        public virtual hmsWireMessageType hmsWireMessageType { get; set; }
        public virtual hmsWire hmsWire { get; set; }
        public virtual hmsWireWorkflowLog hmsWireWorkflowLog { get; set; }
    }
}

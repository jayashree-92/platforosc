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
    
    public partial class hmsWireDocument
    {
        public long hmsWireDocumentId { get; set; }
        public long hmsWireId { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string FileName { get; set; }
    
        public virtual hmsWire hmsWire { get; set; }
    }
}

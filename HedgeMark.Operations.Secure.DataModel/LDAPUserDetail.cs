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
    
    public partial class LDAPUserDetail
    {
        public int LDAPUserDetailId { get; set; }
        public string LDAPUserID { get; set; }
        public int LoginID { get; set; }
        public Nullable<bool> CreatedInLdap { get; set; }
        public Nullable<bool> IsInternal { get; set; }
    }
}

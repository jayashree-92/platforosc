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
    
    public partial class onBoardingAccountSSITemplateMap
    {
        public long onBoardingAccountSSITemplateMapId { get; set; }
        public long onBoardingAccountId { get; set; }
        public long onBoardingSSITemplateId { get; set; }
        public string FFCName { get; set; }
        public string FFCNumber { get; set; }
        public string Reference { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public string Status { get; set; }
        public string StatusComments { get; set; }
    
        public virtual onBoardingSSITemplate onBoardingSSITemplate { get; set; }
        public virtual onBoardingAccount onBoardingAccount { get; set; }
    }
}

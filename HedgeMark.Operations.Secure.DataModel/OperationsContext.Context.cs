﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class OperationsContext : DbContext
    {
        public OperationsContext()
            : base("name=OperationsContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<dmaReport> dmaReports { get; set; }
        public virtual DbSet<dmaOpsCashCollateral> dmaOpsCashCollaterals { get; set; }
        public virtual DbSet<vw_dmaInvoiceReport> vw_dmaInvoiceReport { get; set; }
        public virtual DbSet<vw_HFundOps> vw_HFundOps { get; set; }
        public virtual DbSet<dmaUserPreference> dmaUserPreferences { get; set; }
        public virtual DbSet<dmaTreasuryCashBalance> dmaTreasuryCashBalances { get; set; }
        public virtual DbSet<vw_ProxyCurrencyConversionData> vw_ProxyCurrencyConversionData { get; set; }
        public virtual DbSet<dmaCollateralData> dmaCollateralDatas { get; set; }
    }
}

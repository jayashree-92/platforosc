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
    
    public partial class OperationsSecureContext : DbContext
    {
        public OperationsSecureContext()
            : base("name=OperationsSecureContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<hmsUserAuditLog> hmsUserAuditLogs { get; set; }
        public virtual DbSet<hmsWireDocument> hmsWireDocuments { get; set; }
        public virtual DbSet<hmsWireMessageType> hmsWireMessageTypes { get; set; }
        public virtual DbSet<hmsWireStatusLkup> hmsWireStatusLkups { get; set; }
        public virtual DbSet<hmsSwiftStatusLkup> hmsSwiftStatusLkups { get; set; }
        public virtual DbSet<hmsWireWorkflowLog> hmsWireWorkflowLogs { get; set; }
        public virtual DbSet<hmsWireLog> hmsWireLogs { get; set; }
        public virtual DbSet<hmsWireJobSchedule> hmsWireJobSchedules { get; set; }
        public virtual DbSet<hmsNotificationStaging> hmsNotificationStagings { get; set; }
        public virtual DbSet<hmsWirePurposeLkup> hmsWirePurposeLkups { get; set; }
        public virtual DbSet<hmsWireTransferTypeLKup> hmsWireTransferTypeLKups { get; set; }
        public virtual DbSet<hmsMQLog> hmsMQLogs { get; set; }
        public virtual DbSet<hmsWire> hmsWires { get; set; }
    }
}

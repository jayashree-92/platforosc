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
    
        public virtual DbSet<hmsWireDocument> hmsWireDocuments { get; set; }
        public virtual DbSet<hmsWireMessageType> hmsWireMessageTypes { get; set; }
        public virtual DbSet<hmsWireStatusLkup> hmsWireStatusLkups { get; set; }
        public virtual DbSet<hmsSwiftStatusLkup> hmsSwiftStatusLkups { get; set; }
        public virtual DbSet<hmsWireWorkflowLog> hmsWireWorkflowLogs { get; set; }
        public virtual DbSet<hmsWireJobSchedule> hmsWireJobSchedules { get; set; }
        public virtual DbSet<hmsNotificationStaging> hmsNotificationStagings { get; set; }
        public virtual DbSet<hmsWirePurposeLkup> hmsWirePurposeLkups { get; set; }
        public virtual DbSet<hmsWireTransferTypeLKup> hmsWireTransferTypeLKups { get; set; }
        public virtual DbSet<hmsMQLog> hmsMQLogs { get; set; }
        public virtual DbSet<hmsWireLog> hmsWireLogs { get; set; }
        public virtual DbSet<hmsWireLogTypeLkup> hmsWireLogTypeLkups { get; set; }
        public virtual DbSet<hmsUserAuditLog> hmsUserAuditLogs { get; set; }
        public virtual DbSet<onBoardingAccountBICorABA> onBoardingAccountBICorABAs { get; set; }
        public virtual DbSet<OnBoardingAccountDescription> OnBoardingAccountDescriptions { get; set; }
        public virtual DbSet<onBoardingAccountDocument> onBoardingAccountDocuments { get; set; }
        public virtual DbSet<onBoardingAccountSSITemplateMap> onBoardingAccountSSITemplateMaps { get; set; }
        public virtual DbSet<onBoardingAuthorizedParty> onBoardingAuthorizedParties { get; set; }
        public virtual DbSet<onBoardingCashInstruction> onBoardingCashInstructions { get; set; }
        public virtual DbSet<onBoardingCurrency> onBoardingCurrencies { get; set; }
        public virtual DbSet<onBoardingSSITemplate> onBoardingSSITemplates { get; set; }
        public virtual DbSet<OnBoardingSSITemplateAccountType> OnBoardingSSITemplateAccountTypes { get; set; }
        public virtual DbSet<onBoardingSSITemplateDocument> onBoardingSSITemplateDocuments { get; set; }
        public virtual DbSet<OnBoardingSSITemplateServiceProvider> OnBoardingSSITemplateServiceProviders { get; set; }
        public virtual DbSet<onBoardingSwiftGroup> onBoardingSwiftGroups { get; set; }
        public virtual DbSet<onBoardingWirePortalCutoff> onBoardingWirePortalCutoffs { get; set; }
        public virtual DbSet<hmsWire> hmsWires { get; set; }
        public virtual DbSet<hmsWireSenderInformation> hmsWireSenderInformations { get; set; }
        public virtual DbSet<onBoardingAccountModuleAssociation> onBoardingAccountModuleAssociations { get; set; }
        public virtual DbSet<onBoardingModule> onBoardingModules { get; set; }
        public virtual DbSet<hmsWireInvoiceAssociation> hmsWireInvoiceAssociations { get; set; }
        public virtual DbSet<hmsWireCollateralAssociation> hmsWireCollateralAssociations { get; set; }
        public virtual DbSet<hmsBulkUploadLog> hmsBulkUploadLogs { get; set; }
        public virtual DbSet<hmsWireCutoffTimeZone> hmsWireCutoffTimeZones { get; set; }
        public virtual DbSet<hmsSystemPreference> hmsSystemPreferences { get; set; }
        public virtual DbSet<onBoardingAccount> onBoardingAccounts { get; set; }
        public virtual DbSet<hmsActionInProgress> hmsActionInProgresses { get; set; }
    }
}

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
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class AdminContext : DbContext
    {
        public AdminContext()
            : base("name=AdminContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<aspnet_Roles> aspnet_Roles { get; set; }
        public virtual DbSet<aspnet_Users> aspnet_Users { get; set; }
        public virtual DbSet<hLoginRegistration> hLoginRegistrations { get; set; }
        public virtual DbSet<dmaAgreementDocument> dmaAgreementDocuments { get; set; }
        public virtual DbSet<dmaAgreementOnBoarding> dmaAgreementOnBoardings { get; set; }
        public virtual DbSet<dmaAgreementOnBoardingAttribute> dmaAgreementOnBoardingAttributes { get; set; }
        public virtual DbSet<dmaAgreementOnBoardingChecklist> dmaAgreementOnBoardingChecklists { get; set; }
        public virtual DbSet<dmaAgreementSettlementInstruction> dmaAgreementSettlementInstructions { get; set; }
        public virtual DbSet<dmaAgreementStatu> dmaAgreementStatus { get; set; }
        public virtual DbSet<dmaAgreementType> dmaAgreementTypes { get; set; }
        public virtual DbSet<OnBoardingAccountDescription> OnBoardingAccountDescriptions { get; set; }
        public virtual DbSet<onBoardingAccountDocument> onBoardingAccountDocuments { get; set; }
        public virtual DbSet<onBoardingAccountSSITemplateMap> onBoardingAccountSSITemplateMaps { get; set; }
        public virtual DbSet<OnBoardingSSITemplateAccountType> OnBoardingSSITemplateAccountTypes { get; set; }
        public virtual DbSet<onBoardingSSITemplateDocument> onBoardingSSITemplateDocuments { get; set; }
        public virtual DbSet<OnBoardingSSITemplateServiceProvider> OnBoardingSSITemplateServiceProviders { get; set; }
        public virtual DbSet<dmaCounterPartyOnBoarding> dmaCounterPartyOnBoardings { get; set; }
        public virtual DbSet<onboardingFund> onboardingFunds { get; set; }
        public virtual DbSet<onBoardingAccount> onBoardingAccounts { get; set; }
        public virtual DbSet<onBoardingSSITemplate> onBoardingSSITemplates { get; set; }
        public virtual DbSet<dmaFundOnBoardPermission> dmaFundOnBoardPermissions { get; set; }
        public virtual DbSet<onBoardingAuthorizedParty> onBoardingAuthorizedParties { get; set; }
        public virtual DbSet<onBoardingCashInstruction> onBoardingCashInstructions { get; set; }
        public virtual DbSet<onBoardingCurrency> onBoardingCurrencies { get; set; }
        public virtual DbSet<onBoardingWirePortalCutoff> onBoardingWirePortalCutoffs { get; set; }
        public virtual DbSet<onBoardingSwiftGroup> onBoardingSwiftGroups { get; set; }
        public virtual DbSet<dmaCounterpartyFamily> dmaCounterpartyFamilies { get; set; }
        public virtual DbSet<onBoardingAccountBICorABA> onBoardingAccountBICorABAs { get; set; }
        public virtual DbSet<dmaOnBoardingContactDetail> dmaOnBoardingContactDetails { get; set; }
        public virtual DbSet<onboardingClient> onboardingClients { get; set; }
        public virtual DbSet<vw_HFund> vw_HFund { get; set; }
    
        public virtual ObjectResult<USP_NEXEN_GetUserDetails_Result> USP_NEXEN_GetUserDetails(string userID, string userType)
        {
            var userIDParameter = userID != null ?
                new ObjectParameter("userID", userID) :
                new ObjectParameter("userID", typeof(string));
    
            var userTypeParameter = userType != null ?
                new ObjectParameter("userType", userType) :
                new ObjectParameter("userType", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<USP_NEXEN_GetUserDetails_Result>("USP_NEXEN_GetUserDetails", userIDParameter, userTypeParameter);
        }
    }
}

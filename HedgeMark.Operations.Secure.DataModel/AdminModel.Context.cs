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
        public virtual DbSet<dmaAgreementType> dmaAgreementTypes { get; set; }
        public virtual DbSet<dmaCounterPartyOnBoarding> dmaCounterPartyOnBoardings { get; set; }
        public virtual DbSet<dmaFundOnBoardPermission> dmaFundOnBoardPermissions { get; set; }
        public virtual DbSet<dmaCounterpartyFamily> dmaCounterpartyFamilies { get; set; }
        public virtual DbSet<dmaOnBoardingContactDetail> dmaOnBoardingContactDetails { get; set; }
        public virtual DbSet<OnBoardingServiceProvider> OnBoardingServiceProviders { get; set; }
        public virtual DbSet<vw_CounterpartyAgreements> vw_CounterpartyAgreements { get; set; }
        public virtual DbSet<onboardingContactFundMap> onboardingContactFundMaps { get; set; }
        public virtual DbSet<vw_HFund> vw_HFund { get; set; }
        public virtual DbSet<vw_EmailDomailForFunds> vw_EmailDomailForFunds { get; set; }
        public virtual DbSet<LDAPUserDetail> LDAPUserDetails { get; set; }
        public virtual DbSet<onBoardingAssignmentUserGroup> onBoardingAssignmentUserGroups { get; set; }
        public virtual DbSet<onBoardingAssignmentUserGroupMap> onBoardingAssignmentUserGroupMaps { get; set; }
        public virtual DbSet<onboardingSubAdvisorFundMap> onboardingSubAdvisorFundMaps { get; set; }
    
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

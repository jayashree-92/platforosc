using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeMark.Operations.Secure.DataModel;

namespace HMOSecureMiddleware
{
    public class ContactManager
    {
        public const int ClientTypeId = 1;
        public const int FundTypeId = 2;
        public const int CounterpartyTypeId = 3;
        public const int HedgemarkTypeId = 5;
        public const int AdminTypeId = 6;
        public const int LegalCounselTypeId = 7;
        public const int CustodianTypeId = 8;
        public const int AuditorTypeId = 9;
        public const int TaxAdvisorTypeId = 10;
    }

    public class OnBoardingDataManager
    {

        public static List<onboardingFund> GetAllOnBoardedFunds(List<long> onBoardFundIds, bool isPreviledgedUser)
        {
            using (var context = new AdminContext())
            {
                return context.onboardingFunds.Include(x => x.onboardingClient).Where(s => (isPreviledgedUser || onBoardFundIds.Contains(s.dmaFundOnBoardId)) && !s.IsDeleted).ToList();
            }
        }

        public static Dictionary<long, string> GetAllFunds()
        {
            using (var context = new AdminContext())
            {
                return context.onboardingFunds.Where(f => !f.IsDeleted).ToDictionary(s => s.dmaFundOnBoardId, s => s.LegalFundName);
            }
        }

        public static List<dmaAgreementOnBoarding> GetAllAgreements()
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.dmaAgreementOnBoardings
                    .Include(x => x.onboardingFund)
                    .Include(x => x.dmaCounterPartyOnBoarding)
                    .Include(x => x.dmaAgreementType)
                    .Where(a => !a.IsDeleted).AsNoTracking().ToList();
            }
        }

        public static List<dmaCounterpartyFamily> GetAllCounterpartyFamilies()
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.dmaCounterpartyFamilies.ToList();
            }
        }

        public static List<dmaAgreementOnBoarding> GetAgreementsForOnboardingAccountPreloadData(List<long> onBoardFundIds, bool isPreviledgedUser)
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                //var agreementStatus = context.dmaAgreementStatus.FirstOrDefault(s => s.AgreementStatus == "Fully Executed"); a.AgreementStatusId == agreementStatus.dmaAgreementStatusId &&
                var permittedAgreementTypes = new List<string>() { "CDA", "Custody", "DDA", "Deemed ISDA", "Enhanced Custody", "FCM", "FXPB", "GMRA", "ISDA", "Listed Options", "MRA", "MSFTA", "Non-US Listed Options", "PB" };
                return context.dmaAgreementOnBoardings.Include(x => x.onboardingFund).Include(x => x.dmaAgreementType).Include(x => x.dmaCounterPartyOnBoarding)
                    .Where(a => permittedAgreementTypes.Contains(a.dmaAgreementType.AgreementType) && (isPreviledgedUser || onBoardFundIds.Contains(a.dmaFundOnBoardId)) && !a.IsDeleted)
                    .AsNoTracking().ToList();
            }
        }

        public static dmaAgreementOnBoarding GetOnBoardedAgreement(long agreementId)
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.dmaAgreementOnBoardings
                    //.Include(y => y.dmaAgreementSettlementInstructions)
                    //.Include(y => y.dmaEligibleCollaterals)
                    //.Include(y => y.dmaNonCashCollaterals)
                    //.Include(t => t.dmaNavTriggerRules)
                    //.Include(d => d.dmaAgreementDocuments)
                    //.Include(a => a.dmaAgreementAcadiaSoftMaps)
                    .Include(a => a.dmaAgreementType)
                    .FirstOrDefault(x => x.dmaAgreementOnBoardingId == agreementId);
            }
        }

        public static int GetAgreementTypeId(string agreementType)
        {
            using (var context = new AdminContext())
            {
                var dmaAgreementType = context.dmaAgreementTypes.FirstOrDefault(x => x.AgreementType == agreementType);
                return dmaAgreementType != null ? dmaAgreementType.dmaAgreementTypeId : 0;
            }
        }

        public static List<dmaOnBoardingContactDetail> GetAllOnBoardingContacts(long onBoardingTypeId, long entityId)
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.dmaOnBoardingContactDetails.Where(contact => contact.dmaOnBoardingTypeId == onBoardingTypeId && contact.dmaOnBoardingEntityId == entityId && !contact.IsDeleted).ToList();
            }
        }

        public static List<long> GetCounterpartyIdsbyFund(long fundId)
        {
            using (var context = new AdminContext())
            {

                return context.dmaAgreementOnBoardings.Where(x => x.dmaFundOnBoardId == fundId && !x.IsDeleted && x.dmaCounterPartyOnBoardId.HasValue && x.HMOpsStatus == "Approved")
                .Select(x => x.dmaCounterPartyOnBoardId.Value)
                .Distinct()
                .ToList();
            }
        }

        public static string GetCounterpartyFamilyName(long counterpartyFamilyId)
        {
            using (var context = new AdminContext())
            {

                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var counterpartyFamily = context.dmaCounterpartyFamilies.FirstOrDefault(x => x.dmaCounterpartyFamilyId == counterpartyFamilyId);
                return (counterpartyFamily != null) ? counterpartyFamily.CounterpartyFamily : string.Empty;
            }
        }

        public static string GetOnBoardedFundName(long fundId)
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var onboardingFund = context.onboardingFunds.FirstOrDefault(x => x.dmaFundOnBoardId == fundId);
                return (onboardingFund != null) ? onboardingFund.LegalFundName : string.Empty;
            }
        }

        public static Dictionary<int, string> GetAllAgreementTypes()
        {
            using (var context = new AdminContext())
            {
                return context.dmaAgreementTypes.ToDictionary(x => x.dmaAgreementTypeId, x => x.AgreementType);
            }
        }

        public static List<dmaCounterPartyOnBoarding> GetAllOnBoardedCounterparties()
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.dmaCounterPartyOnBoardings.Where(c => !c.IsDeleted).Include(x => x.dmaCounterpartyFamily).ToList();
            }
        }
    }
}

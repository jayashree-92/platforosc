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
    public class AgreementBaseData
    {
        public long HMFundId { get; set; }
        public long AgreementOnboardingId { get; set; }
        public string AgreementShortName { get; set; }
        public int AgreementTypeId { get; set; }
        public string AgreementType { get; set; }
        public long BrokerId { get; set; }
    }

    public class OnBoardingDataManager
    {
        public static Dictionary<long, string> GetAllAgreements()
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.vw_OnboardedAgreements.AsNoTracking().ToDictionary(s => s.dmaAgreementOnBoardingId, s => s.AgreementShortName);
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

        public static string GetCounterpartyFamilyName(long counterpartyFamilyId)
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.dmaCounterpartyFamilies.Where(s => s.dmaCounterpartyFamilyId == counterpartyFamilyId).Select(s => s.CounterpartyFamily).FirstOrDefault() ?? string.Empty;
            }
        }

        public static List<AgreementBaseData> GetAgreementsForOnboardingAccountPreloadData(List<long> hmFundIds, bool isPreviledgedUser)
        {
            var permittedAgreementTypes = PreferencesManager.GetSystemPreference(PreferencesManager.SystemPreferences.AllowedAgreementTypesForAccounts).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                return context.vw_OnboardedAgreements
                    .Where(a => a.AgreementStatus != "Terminated – Agreement" && permittedAgreementTypes.Contains(a.AgreementType) && (isPreviledgedUser || hmFundIds.Contains(a.hmFundId ?? 0)))
                    .AsNoTracking().Select(x => new AgreementBaseData() { AgreementOnboardingId = x.dmaAgreementOnBoardingId, AgreementShortName = x.AgreementShortName, HMFundId = x.hmFundId ?? 0, AgreementTypeId = x.AgreementTypeId ?? 0, AgreementType = x.AgreementType, BrokerId = x.dmaCounterPartyFamilyId ?? 0 }).ToList();
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
                return context.vw_OnboardedAgreements.Where(x => x.hmFundId == fundId && x.dmaCounterPartyOnBoardId.HasValue && x.HMOpsStatus == "Approved").Select(x => x.dmaCounterPartyOnBoardId.Value).Distinct().ToList();
            }
        }


        public static List<long> GetFundIdsbyCounterparty(long brokerId)
        {
            using (var context = new AdminContext())
            {
                var intFundIds = context.vw_OnboardedAgreements.Where(x => (x.dmaCounterPartyOnBoardId ?? 0) == brokerId && x.hmFundId.HasValue && x.HMOpsStatus == "Approved").Select(x => x.hmFundId.Value).Distinct().ToList();
                return intFundIds.Select(Convert.ToInt64).ToList();
            }
        }

        public static Dictionary<int, string> GetAllAgreementTypes(List<string> agreementTypes = null)
        {
            using (var context = new AdminContext())
            {
                if (agreementTypes == null)
                    agreementTypes = new List<string>();
                return context.dmaAgreementTypes.Where(s => agreementTypes.Count == 0 || agreementTypes.Contains(s.AgreementType)).ToDictionary(x => x.dmaAgreementTypeId, x => x.AgreementType);
            }
        }

        public static Dictionary<long, string> GetAllOnBoardedCounterparties()
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.dmaCounterPartyOnBoardings.Where(c => !c.IsDeleted).Include(x => x.dmaCounterpartyFamily).ToDictionary(x => x.dmaCounterPartyOnBoardId, x => x.CounterpartyName);
            }
        }
    }
}

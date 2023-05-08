using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HM.Operations.Secure.DataModel;

namespace HM.Operations.Secure.Middleware
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
        public long CounterpartyFamilyId { get; set; }
        public long CounterpartyId { get; set; }
    }

    public class CounterpartyData
    {
        public long CounterpartyId { get; set; }
        public long CounterpartyFamilyId { get; set; }
        public string CounterpartyFamilyName { get; set; }
        public string CounterpartyName { get; set; }
    }

    public class OnBoardingDataManager
    {
        public static Dictionary<long, string> GetAllAgreements()
        {
            using var context = new AdminContext();
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            return context.vw_CounterpartyAgreements.AsNoTracking().ToDictionary(s => s.dmaAgreementOnBoardingId, s => s.AgreementShortName);
        }

        public static List<dmaCounterpartyFamily> GetAllCounterpartyFamilies()
        {
            using var context = new AdminContext();
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            return context.dmaCounterpartyFamilies.ToList();
        }

        public static List<CounterpartyData> GetAllCounterparties()
        {
            using var context = new AdminContext();
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            return context.dmaCounterPartyOnBoardings.Where(s => !s.IsDeleted).Include(s => s.dmaCounterpartyFamily).Select(s => new CounterpartyData
            {
                CounterpartyName = s.CounterpartyName,
                CounterpartyFamilyId = s.dmaCounterpartyFamilyId ?? 0,
                CounterpartyId = s.dmaCounterPartyOnBoardId,
                CounterpartyFamilyName = s.dmaCounterpartyFamily.CounterpartyFamily
            }).ToList();
        }

        public static Dictionary<long, string> GetAllOnBoardedCounterparties()
        {
            using var context = new AdminContext();
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            return context.dmaCounterPartyOnBoardings.Where(c => !c.IsDeleted).ToDictionary(x => x.dmaCounterPartyOnBoardId, x => x.CounterpartyName);
        }

        public static string GetCounterpartyFamilyName(long counterpartyFamilyId)
        {
            using var context = new AdminContext();
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            return context.dmaCounterpartyFamilies.Where(s => s.dmaCounterpartyFamilyId == counterpartyFamilyId).Select(s => s.CounterpartyFamily).FirstOrDefault() ?? string.Empty;
        }

        public static List<AgreementBaseData> GetAgreementsForOnboardingAccountPreloadData(List<long> hmFundIds, bool isPrivilegedUser)
        {
            var allowedAgreementTypes = OpsSecureSwitches.AllowedAgreementTypesForFundAccountCreation;
            var allowedAgreementStatus = OpsSecureSwitches.AllowedAgreementStatusForFundAccountCreation;
            using var context = new AdminContext();
            var allAgreements = context.vw_CounterpartyAgreements
                .Where(a => allowedAgreementStatus.Contains(a.AgreementStatus)
                            && allowedAgreementTypes.Contains(a.AgreementType)
                            && a.dmaCounterPartyOnBoardId != null && a.dmaCounterPartyOnBoardId > 0)
                .AsNoTracking().Select(x => new AgreementBaseData()
                {
                    AgreementOnboardingId = x.dmaAgreementOnBoardingId,
                    AgreementShortName = x.AgreementShortName,
                    HMFundId = x.FundMapId ?? 0,
                    AgreementTypeId = x.AgreementTypeId ?? 0,
                    AgreementType = x.AgreementType,
                    CounterpartyFamilyId = x.dmaCounterPartyFamilyId ?? 0,
                    CounterpartyId = x.dmaCounterPartyOnBoardId ?? 0
                }).ToList();

            return isPrivilegedUser ? allAgreements : allAgreements.Where(s => hmFundIds.Contains(s.HMFundId)).ToList();
        }

        public static List<dmaOnBoardingContactDetail> GetAllOnBoardingContacts(long hmFundId)
        {
            using var context = new AdminContext();
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;

            return (from cont in context.dmaOnBoardingContactDetails
                    join cntM in context.onboardingContactFundMaps on cont.dmaOnBoardingContactDetailId equals cntM.dmaOnBoardingContactDetailId
                    join fnd in context.vw_HFund on cntM.dmaFundOnBoardId equals fnd.dmaFundOnBoardId
                    where fnd.hmFundId == hmFundId && cont.Wires && !cont.IsDeleted
                    select cont).Union(
                from cont in context.dmaOnBoardingContactDetails
                where cont.dmaOnBoardingTypeId == ContactManager.FundTypeId && cont.dmaOnBoardingEntityId == hmFundId && cont.Wires && !cont.IsDeleted
                select cont).Distinct().ToList();
        }

        public static List<long> GetCounterpartyIdsbyFund(long fundId)
        {
            using var context = new AdminContext();
            return context.vw_CounterpartyAgreements.Where(x => x.FundMapId == fundId && x.dmaCounterPartyOnBoardId.HasValue && x.HMOpsStatus == "Approved").Select(x => x.dmaCounterPartyOnBoardId.Value).Distinct().ToList();
        }

        public static List<string> GetServiceProvidersbyFund(long fundId)
        {
            using var context = new AdminContext();
            {
                var onbFundId = context.vw_HFund.Where(s => s.hmFundId == fundId).Select(s => s.dmaFundOnBoardId).FirstOrDefault() ?? 0;
                return context.OnBoardingFundServiceProviderMaps.Where(x => x.dmaFundOnBoardId == onbFundId && !x.IsDeleted).Select(x => x.OnBoardingServiceProvider.ServiceProvider).Distinct().ToList();
            }
        }

        public static List<long> GetFundIdsbyCounterparty(long brokerId)
        {
            using var context = new AdminContext();
            var intFundIds = context.vw_CounterpartyAgreements.Where(x => (x.dmaCounterPartyOnBoardId ?? 0) == brokerId && x.FundMapId.HasValue && x.HMOpsStatus == "Approved").Select(x => x.FundMapId.Value).Distinct().ToList();
            return intFundIds.Select(Convert.ToInt64).ToList();
        }

        public static Dictionary<int, string> GetAllAgreementTypes(List<string> agreementTypes = null)
        {
            using var context = new AdminContext();
            agreementTypes ??= new List<string>();
            return context.dmaAgreementTypes.AsNoTracking().Where(s => agreementTypes.Count == 0 || agreementTypes.Contains(s.AgreementType)).ToDictionary(x => x.dmaAgreementTypeId, x => x.AgreementType);
        }
    }
}

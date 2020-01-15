﻿using System;
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

    public class FundBaseData
    {
        public long FundOnBoardId { get; set; }
        public string LegalFundName { get; set; }
    }

    public class AgreementBaseData
    {
        public long FundOnBoardId { get; set; }
        public long AgreementOnboardingId { get; set; }
        public string AgreementShortName { get; set; }
        public int AgreementTypeId { get; set; }
    }

    public class OnBoardingDataManager
    {

        public static List<FundBaseData> GetAllOnBoardedFunds(List<long> onBoardFundIds, bool isPreviledgedUser)
        {
            using (var context = new AdminContext())
            {
                return context.vw_HFund.Where(s => s.dmaFundOnBoardId != null).Where(s => (isPreviledgedUser || onBoardFundIds.Contains(s.dmaFundOnBoardId ?? 0))).Select(s => new FundBaseData() { FundOnBoardId = s.dmaFundOnBoardId ?? 0, LegalFundName = s.LegalFundName }).ToList();
            }
        }

        public static Dictionary<long, string> GetAllFunds()
        {
            using (var context = new AdminContext())
            {
                return context.vw_HFund.Where(f => f.dmaFundOnBoardId != null).ToDictionary(s => s.dmaFundOnBoardId ?? 0, s => s.LegalFundName);
            }
        }

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

        public static List<AgreementBaseData> GetAgreementsForOnboardingAccountPreloadData(List<long> onBoardFundIds, bool isPreviledgedUser)
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                //var agreementStatus = context.dmaAgreementStatus.FirstOrDefault(s => s.AgreementStatus == "Fully Executed"); a.AgreementStatusId == agreementStatus.dmaAgreementStatusId &&
                var permittedAgreementTypes = new List<string>() { "CDA", "Custody", "DDA", "Deemed ISDA", "Enhanced Custody", "FCM", "FXPB", "GMRA", "ISDA", "Listed Options", "MRA", "MSFTA", "Non-US Listed Options", "PB" };
                return context.vw_OnboardedAgreements
                    .Where(a => a.AgreementStatus != "Terminated – Agreement" && permittedAgreementTypes.Contains(a.AgreementType) && (isPreviledgedUser || onBoardFundIds.Contains(a.dmaFundOnBoardId)))
                    .AsNoTracking().Select(x => new AgreementBaseData() { AgreementOnboardingId = x.dmaAgreementOnBoardingId, AgreementShortName = x.AgreementShortName, FundOnBoardId = x.dmaFundOnBoardId }).ToList();
            }
        }

        public static vw_OnboardedAgreements GetOnBoardedAgreement(long agreementId)
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.vw_OnboardedAgreements.FirstOrDefault(x => x.dmaAgreementOnBoardingId == agreementId);
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
                return context.vw_OnboardedAgreements.Where(x => x.dmaFundOnBoardId == fundId && x.dmaCounterPartyOnBoardId.HasValue && x.HMOpsStatus == "Approved").Select(x => x.dmaCounterPartyOnBoardId.Value).Distinct().ToList();
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
                var onboardingFund = context.vw_HFund.FirstOrDefault(x => x.dmaFundOnBoardId == fundId);
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

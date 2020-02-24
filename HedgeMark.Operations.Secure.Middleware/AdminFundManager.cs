using HedgeMark.Operations.Secure.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace HMOSecureMiddleware
{
    public class AdminFundManager
    {
        public class QueryableHFund
        {
            public int hmFundId { get; set; }
            public string PreferredFundName { get; set; }
            public vw_HFundOps HFund { get; set; }
        }


        public static Dictionary<int, string> GetHFundsCreatedForDMA(PreferencesManager.FundNameInDropDown preferredFundName)
        {
            using (var context = new OperationsContext())
            {
                return GetUniversalDMAFundListQuery(context, preferredFundName).ToDictionary(s => s.hmFundId, v => v.PreferredFundName);
            }
        }

        public static Dictionary<int, string> GetHFundsCreatedForDMA(List<long> hFundIds, bool isPreviledgedUser, PreferencesManager.FundNameInDropDown preferredFundName)
        {
            using (var context = new OperationsContext())
            {
                return GetUniversalDMAFundListQuery(context, preferredFundName).ToDictionary(s => s.hmFundId, v => v.PreferredFundName);
            }
        }

        public static List<HFund> GetHFundsCreatedForDMA(List<long> hFundIds, PreferencesManager.FundNameInDropDown preferredFundName)
        {
            using (var context = new OperationsContext())
            {
                return GetUniversalDMAFundListQuery(context, preferredFundName).Where(s => hFundIds.Contains(s.hmFundId)).Select(s => new HFund
                {
                    HFundId = s.hmFundId,
                    ClientFundName = s.HFund.ClientFundName,
                    ShortFundName = s.HFund.ShortFundName,
                    PerferredFundName = s.PreferredFundName,
                    HMDataFundName = s.HFund.HMRAName,
                    Currency = s.HFund.BaseCurrencyShareclass != null ? s.HFund.BaseCurrencyShareclass : string.Empty,
                    LegalFundName = s.HFund.LegalFundName,
                    ClientShortName = s.HFund.ClientShortName,
                    ClientLegalName = s.HFund.ClientLegalEntityName,
                    
                }).ToList();
            }
        }

        public static HFund GetHFundCreatedForDMA(long hmFundId, PreferencesManager.FundNameInDropDown preferredFundName)
        {
            return GetHFundsCreatedForDMA(new List<long>() {hmFundId}, preferredFundName).FirstOrDefault();
        }

        public static IQueryable<QueryableHFund> GetUniversalDMAFundListQuery(OperationsContext context, PreferencesManager.FundNameInDropDown preferredFundName)
        {
            return (from fund in context.vw_HFundOps
                    let prefName = preferredFundName == PreferencesManager.FundNameInDropDown.ClientFundName ? fund.ClientFundName
                        : preferredFundName == PreferencesManager.FundNameInDropDown.HMRAName ? fund.HMRAName
                        : preferredFundName == PreferencesManager.FundNameInDropDown.LegalFundName ? fund.LegalFundName
                        : preferredFundName == PreferencesManager.FundNameInDropDown.OpsShortName && fund.ShortFundName != null ? fund.ShortFundName
                        : fund.ClientFundName

                    select new QueryableHFund()
                    {
                        hmFundId = fund.intFundId,
                        HFund = fund,
                        PreferredFundName = prefName.Replace("\t", "")
                    }).OrderBy(s => s.PreferredFundName);
        }
    }

    public class HFund
    {
        public HFund()
        {
            Children = new List<HFund>();
        }
        public bool IsGroupLine
        {
            get { return !IsMasterFund && !IsFeederFund && !IsStandAloneFund; }
        }
        public bool IsMasterFund { get; set; }
        public bool IsFeederFund { get; set; }
        public bool IsStandAloneFund { get; set; }
        public string ClientFundName { get; set; }
        public string ShortFundName { get; set; }
        public string HMDataFundName { get; set; }
        public string PerferredFundName { get; set; }
        public int HFundId { get; set; }
        public string LegalFundName { get; set; }
        public string Currency { get; set; }
        public List<HFund> Children { get; set; }

        public string ClientLegalName { get; set; }
        public string ClientShortName { get; set; }
    }
}

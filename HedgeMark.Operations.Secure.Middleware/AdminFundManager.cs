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
            public vw_HFund HFund { get; set; }
        }

        public static List<HFund> GetHFundsCreatedForDMA(List<long> hFundIds, string userName, string preferredCode = null)
        {
            using (var context = new AdminContext())
            {
                return GetUniversalDMAFundListQuery(context, userName, preferredCode).Where(s => hFundIds.Contains(s.hmFundId)).Select(s => new HFund
                {
                    HFundId = s.hmFundId,
                    ClientFundName = s.HFund.ClientFundName,
                    OpsFundName = s.HFund.ShortFundName,
                    PerferredFundName = s.PreferredFundName,
                    HMDataFundName = s.HFund.HMRAName,
                    Currency = s.HFund.BaseCurrencyShareclass != null ? s.HFund.BaseCurrencyShareclass : string.Empty,
                    LegalFundName = s.HFund.LegalFundName
                }).ToList();
            }
        }

        public static IQueryable<QueryableHFund> GetUniversalDMAFundListQuery(AdminContext context, string userName, string defaultPreferredCode = null)
        {
            return (from fund in context.vw_HFund
                    where fund.CreatedFor.Contains("DMA")
                    //let prefName = preferredFundName == PreferencesManager.FundNameInDropDown.RiskLongFundName ? fund.varFundLongName
                    //    : preferredFundName == PreferencesManager.FundNameInDropDown.HMRAName ? fund.varMRDBName
                    //    : fundOps == null ? fund.varFundLongName
                    //    : preferredFundName == PreferencesManager.FundNameInDropDown.OpsShortName ? fundOps.ShortFundName
                    //    : fundOps.LegalFundName
                    let prefName = fund.ShortFundName

                    select new QueryableHFund()
                    {
                        hmFundId = fund.intFundID,
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
        public string OpsFundName { get; set; }
        public string HMDataFundName { get; set; }
        public string PerferredFundName { get; set; }
        public int HFundId { get; set; }
        public string LegalFundName { get; set; }
        public string Currency { get; set; }
        public List<HFund> Children { get; set; }
    }
}

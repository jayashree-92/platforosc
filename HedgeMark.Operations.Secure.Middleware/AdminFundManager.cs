using HedgeMark.Operations.Secure.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMOSecureMiddleware
{
    public class AdminFundManager
    {
        public class QueryableHFund
        {
            public int hmFundId { get; set; }
            public string PreferredFundName { get; set; }
            public vw_HFund HFund { get; set; }
            public vw_HFundOps OpsFund { get; set; }
        }

        public static List<HFund> GetHFundsCreatedForDMA(List<long> hFundIds, string userName, string preferredCode = null)
        {
            using (var context = new AdminContext())
            {
                return GetUniversalDMAFundListQuery(context, userName, preferredCode).Where(s => hFundIds.Contains(s.hmFundId)).Select(s => new HFund
                {
                    HFundId = s.hmFundId,
                    FundShortName = s.HFund.varFundShortName,
                    FundLongName = s.HFund.varFundLongName,
                    OpsFundName = s.OpsFund.ShortFundName,
                    PerferredFundName = s.PreferredFundName,
                    HMDataFundName = s.HFund.varMRDBName,
                    Currency = s.OpsFund != null ? s.OpsFund.BaseCurrencyShareclass : string.Empty,
                    LegalFundName = s.OpsFund != null ? s.OpsFund.LegalFundName : string.Empty
                }).ToList();
            }
        }

        public static List<HFund> GetHFundsCreatedForDMA(List<string> fundNames, string userName)
        {
            using (var context = new AdminContext())
            {
                return GetUniversalDMAFundListQuery(context, userName).Where(s => fundNames.Contains(s.PreferredFundName)).Select(s => new HFund
                {
                    HFundId = s.hmFundId,
                    FundShortName = s.HFund.varFundShortName,
                    FundLongName = s.HFund.varFundLongName,
                    OpsFundName = s.OpsFund.ShortFundName,
                    PerferredFundName = s.PreferredFundName,
                    Currency = s.OpsFund != null ? s.OpsFund.BaseCurrencyShareclass : string.Empty,
                    LegalFundName = s.OpsFund != null ? s.OpsFund.LegalFundName : string.Empty
                }).ToList();
            }
        }
        public static IQueryable<QueryableHFund> GetUniversalDMAFundListQuery(AdminContext context, string userName, string defaultPreferredCode = null)
        {
            return (from fund in context.vw_HFund
                    join fundOps in context.vw_HFundOps on fund.intFundID equals fundOps.intFundId
                    where fundOps.CreatedFor.Contains("DMA")
                    //let prefName = preferredFundName == PreferencesManager.FundNameInDropDown.RiskLongFundName ? fund.varFundLongName
                    //    : preferredFundName == PreferencesManager.FundNameInDropDown.HMRAName ? fund.varMRDBName
                    //    : fundOps == null ? fund.varFundLongName
                    //    : preferredFundName == PreferencesManager.FundNameInDropDown.OpsShortName ? fundOps.ShortFundName
                    //    : fundOps.LegalFundName
                    let prefName = fundOps.ShortFundName

                    select new QueryableHFund()
                    {
                        hmFundId = fund.intFundID,
                        HFund = fund,
                        OpsFund = fundOps,
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
        public string FundLongName { get; set; }
        public string OpsFundName { get; set; }
        public string HMDataFundName { get; set; }
        public string PerferredFundName { get; set; }
        public long dmaFundId { get; set; }
        public int HFundId { get; set; }
        public string FundShortName { get; set; }
        public string LegalFundName { get; set; }
        public string Currency { get; set; }
        public string AgreementName { get; set; }
        public long? AgreementId { get; set; }
        public long? ParentFundId { get; set; }
        public List<HFund> Children { get; set; }
    }
}

using System.Collections.Generic;
using System.Linq;
using HedgeMark.Operations.Secure.DataModel;

namespace HedgeMark.Operations.Secure.Middleware
{
    public class AdminFundManager
    {
        public class QueryableHFund
        {
            public int hmFundId { get; set; }
            public string PreferredFundName { get; set; }
            public vw_HFundOps HFund { get; set; }
            //public List<int> CalendarIds { get; set; }
        }

        public static List<HFund> GetHFundsCreatedForDMA(List<long> hFundIds, PreferencesManager.FundNameInDropDown preferredFundName)
        {
            using (var context = new OperationsContext())
            {
                return GetUniversalDMAFundListQuery(context, preferredFundName).Where(s => hFundIds.Contains(s.hmFundId)).Select(s => new HFund
                {
                    FundId = s.hmFundId,
                    ShortFundName = s.HFund.ShortFundName,
                    PerferredFundName = s.PreferredFundName,
                    HMDataFundName = s.HFund.HMRAName,
                    Currency = s.HFund.BaseCurrencyShareclass != null ? s.HFund.BaseCurrencyShareclass : string.Empty,
                    LegalFundName = s.HFund.LegalFundName,
                    ClientShortName = s.HFund.ClientShortName,
                    ClientLegalName = s.HFund.ClientLegalEntityName,
                    ClientId = s.HFund.dmaClientOnBoardId ?? 0

                }).ToList();
            }
        }

        public static List<HFundBasic> GetHFundsCreatedForDMAOnly(PreferencesManager.FundNameInDropDown preferredFundName, List<long> hmFundIds = null)
        {
            var shouldBringAllFunds = hmFundIds == null;
            if (hmFundIds == null)
                hmFundIds = new List<long>();

            using (var context = new OperationsContext())
            {
                return (from fnd in GetUniversalDMAFundListQuery(context, preferredFundName)
                        where shouldBringAllFunds || hmFundIds.Contains(fnd.HFund.hmFundId)
                        select new HFundBasic()
                        {
                            HmFundId = fnd.HFund.hmFundId,
                            OnBoardFundId = fnd.HFund.OnBoardFundId ?? 0,
                            PreferredFundName = fnd.PreferredFundName ?? "Unknown Fund-" + fnd.HFund.hmFundId,
                            FundType = "DMA",
                            LegalFundName = fnd.HFund.LegalFundName,
                            ClientLegalName = fnd.HFund.ClientLegalEntityName
                            //CalendarIds = fnd.CalendarIds,
                        }).ToList();
            }
        }

        public static HFundBasic GetHFundsCreatedForDMAOnly(PreferencesManager.FundNameInDropDown preferredFundName, long hmFundId)
        {
            return GetHFundsCreatedForDMAOnly(preferredFundName, new List<long>() { hmFundId }).FirstOrDefault();
        }

        public static IQueryable<QueryableHFund> GetUniversalDMAFundListQuery(OperationsContext context, PreferencesManager.FundNameInDropDown preferredFundName)
        {
            return (from fund in context.vw_HFundOps
                    let prefName = preferredFundName == PreferencesManager.FundNameInDropDown.HMRAName ? fund.HMRAName
                        : preferredFundName == PreferencesManager.FundNameInDropDown.OpsShortName ? fund.ShortFundName
                        : fund.LegalFundName

                    select new QueryableHFund()
                    {
                        hmFundId = fund.hmFundId,
                        HFund = fund,
                        PreferredFundName = prefName.Replace("\t", ""),
                        //CalendarIds = fndWitMap.Select(s => s.onboardHolidayCalendarId ?? 0).ToList()
                    }).OrderBy(s => s.PreferredFundName);
        }

        public static List<HFundBasic> GetFundData(AuthorizedData authorizedData, PreferencesManager.FundNameInDropDown preferredFundName)
        {
            var allFunds = GetHFundsCreatedForDMAOnly(preferredFundName);

            if (authorizedData.IsPrivilegedUser)
                return allFunds;

            var authorizedIds = authorizedData.HMFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            return allFunds.Where(s => authorizedIds.Contains(s.HmFundId)).ToList();
        }

    }

    public class HFundBasic
    {
        public long HmFundId { get; set; }
        public long OnBoardFundId { get; set; }
        public string PreferredFundName { get; set; }
        public string LegalFundName { get; set; }
        public string FundType { get; set; }
        public string ClientLegalName { get; set; }
        //public List<int> CalendarIds { get; set; }
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
        public string ShortFundName { get; set; }
        public string HMDataFundName { get; set; }
        public string PerferredFundName { get; set; }
        public string LegalFundName { get; set; }
        public string Currency { get; set; }
        public List<HFund> Children { get; set; }
        public string ClientLegalName { get; set; }
        public string ClientShortName { get; set; }
        public long ClientId { get; set; }
        public int FundId { get; set; }
    }
}

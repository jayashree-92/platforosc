using System.Collections.Generic;
using System.Linq;
using HM.Operations.Secure.DataModel;

namespace HM.Operations.Secure.Middleware
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
                            ClientLegalName = fnd.HFund.ClientLegalEntityName,
                            RegisteredAddress = fnd.HFund.RegisterAddress,
                            IsFundAllowedForBankLoanAndIpOs = fnd.HFund.IsFundAllowedForBankLoanAndIPOs

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
        public string AdminName { get; set; }
        public string RegisteredAddress { get; set; }
        public bool IsFundAllowedForBankLoanAndIpOs { get; set; }
        //public List<int> CalendarIds { get; set; }
    }
}

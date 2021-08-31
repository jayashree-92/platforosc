using System.Collections.Generic;
using System.Linq;
using Com.HedgeMark.Commons.Extensions;
using HM.Operations.Secure.DataModel;

namespace HM.Operations.Secure.Middleware
{
    public class SystemSwitches : Switches
    {
        public static List<string> TreasuryReportAgreementTypesToUseMarginExcessOrDeficit => ((string)SystemSwitches[SwitchKey.TreasuryReportAgreementTypesToUseMarginExcessOrDeficit]).ToStringList();
        public static int SchedulerProcessId => (int)SystemSwitches[SwitchKey.SchedulerProcessId];

        public dynamic this[SwitchKey key] => GetSwitch(key);

        private static dynamic GetSwitch(SwitchKey switchKey)
        {
            using (var context = new OperationsContext())
            {
                var swtch = context.dmaSystemSwitches.FirstOrDefault(s => s.Key == switchKey.ToString());
                return swtch == null ? string.Empty : swtch.Value;
            }
        }


        public enum SwitchKey
        {
            TreasuryReportAgreementTypesToUseMarginExcessOrDeficit,
            SchedulerProcessId
        }

    }

    public class Switches
    {
        //for singleton indexer
        public static SystemSwitches SystemSwitches = new SystemSwitches();

    }
}

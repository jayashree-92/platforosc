using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.Secure.DataModel;

namespace HMOSecureMiddleware
{
    public class SystemSwitches : Switches
    {
        public static List<string> TreasuryReportAgreementTypesToUseMarginExcessOrDeficit { get { return ((string)SystemSwitches[SwitchKey.TreasuryReportAgreementTypesToUseMarginExcessOrDeficit]).ToStringList(); } }
        public dynamic this[SwitchKey key]
        {
            get
            {
                return GetSwitch(key);
            }
        }

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
            TreasuryReportAgreementTypesToUseMarginExcessOrDeficit
        }

    }

    public class Switches
    {
        //for singleton indexer
        public static SystemSwitches SystemSwitches = new SystemSwitches();

    }
}

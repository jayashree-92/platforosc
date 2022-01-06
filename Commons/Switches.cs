using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.HedgeMark.Commons.Extensions;

namespace Com.HedgeMark.Commons
{
    public class SystemSwitches : Switches
    {
        //public dynamic this[SwitchKey key]
        //{
        //    get
        //    {
        //        var thisSwitch = AllSwitches.First(s => s.Key == key);
        //        return GetSwitchValue(thisSwitch);
        //    }
        //}

        public static dynamic GetSwitchValue(Switch thisSwitch)
        {
            switch (thisSwitch.Type)
            {
                case SwitchType.Boolean:
                    return thisSwitch.Value.ToBool();
                case SwitchType.Integer:
                    return thisSwitch.Value.ToInt();
                case SwitchType.String:
                    return Convert.ToString(thisSwitch.Value);
                case SwitchType.StringList:
                    return Convert.ToString(thisSwitch.Value).Split(',').Select(x => x.Trim()).ToList();
                case SwitchType.MultiLines:
                    return Convert.ToString(thisSwitch.Value);
                case SwitchType.TimeSpan:
                default:
                    return thisSwitch.Value.ToInt();
            }
        }

        public static Dictionary<string, List<Switch>> GroupedSwitchList
        {
            get
            {
                return AllSwitches.GroupBy(s => s.Module).ToDictionary(s => s.Key, v => v.ToList());
            }
        }

        public static string GetSwitchModule(SwitchKey switchKey)
        {
            return AllSwitches.First(s => s.Key == switchKey).Module;
        }

        public static List<string> AllModules => AllSwitches.Select(s => s.Module).Distinct().ToList();
    }

    public class Switches
    {

        public enum SwitchType { Boolean, Integer, String, StringList, TimeSpan, MultiLines }

        public class Switch
        {
            public string Module { get; set; }
            public SwitchKey Key { get; set; }
            public object Value { get; set; }
            public SwitchType Type { get; set; }
        }

        protected static List<Switch> AllSwitches;

        //for singleton indexer
        public static SystemSwitches SystemSwitches = new SystemSwitches();

        static Switches()
        {
            InitializeAllSwitches();
        }

        private static Switch CreateSwitch(SwitchKey key, SwitchType type, string module = Modules.General, object defaultVal = null)
        {
            var thisSwitch = new Switch
            {
                Key = key,
                Type = type,
            };

            if (module != null)
                thisSwitch.Module = module;

            if (defaultVal != null)
            {
                thisSwitch.Value = type == SwitchType.String ? defaultVal.ToString() : defaultVal;
            }

            return thisSwitch;
        }

        //*Note: To add new switches into the system, please add an Enum and set the module in Dictionary*//

        protected class Modules
        {
            public const string General = "General Settings";
            public const string OpsSecure = "HM-Ops-Secure";
        }

        public enum SwitchKey
        {
            /*General Settings*/
            HedgeMarkMailOpsSignatureLines,
            /* System Allowed File Types */
            SupportingFileTypesToUploadViaUi,
            AgreementTypesEligibleForSendingWires,
            TreasuryReportAgreementTypesToUseMarginExcessOrDeficit,
            SchedulerProcessId,
            /*HM-Ops-Secure settings*/
            SwiftBicToEnableField21,
        }

        public static void InitializeAllSwitches()
        {
            AllSwitches = new List<Switch>
            {
                /*General Setting*/
                CreateSwitch(SwitchKey.SchedulerProcessId, SwitchType.Integer, Modules.General, 35997),
                CreateSwitch(SwitchKey.HedgeMarkMailOpsSignatureLines, SwitchType.MultiLines, Modules.General, @"HedgeMark Advisors, LLC - A BNY Mellon Company
240 Greenwich Street, 6th Floor
New York, NY 10286
Main: (212) 888-1300 
<a href = 'mailto:HM-Operations@bnymellon.com' title='Follow link' style = 'cursor: pointer;'>HM-Operations@bnymellon.com</a>
Disclaimer: This message is Confidential until classified otherwise.
"),
                CreateSwitch(SwitchKey.SupportingFileTypesToUploadViaUi, SwitchType.String, Modules.General, ".msg,.csv,.txt,.pdf,.xls,.xlsx,.xlsm,.xlsb,.zip,.rar,.json,.doc,.docx"),
                /*HM-Ops-Secure settings*/
                CreateSwitch(SwitchKey.AgreementTypesEligibleForSendingWires, SwitchType.StringList, Modules.OpsSecure, "Custody,DDA,PB,Synthetic Prime Brokerage"),
                CreateSwitch(SwitchKey.TreasuryReportAgreementTypesToUseMarginExcessOrDeficit, SwitchType.StringList, Modules.OpsSecure, "PB,FXPB,FCM,CDA,Synthetic Prime Brokerage"),
                CreateSwitch(SwitchKey.SwiftBicToEnableField21, SwitchType.StringList, Modules.OpsSecure, "SBOSUS30,SBOSUS3U,CNORLULX,SBOSUS3UIMS")
            };

            
            
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Com.HedgeMark.Commons;
using Com.HedgeMark.Commons.Extensions;
using HM.Operations.Secure.DataModel;

namespace HM.Operations.Secure.Middleware
{
    public class OpsSecureSwitches : Switches
    {
        static OpsSecureSwitches()
        {
            ResetSwitches();
        }

        public static List<string> AllowedAgreementTypesForFundAccountCreation => GetSwitchValue(SwitchKey.AllowedAgreementTypesForFundAccountCreation);
        public static List<string> AllowedAgreementStatusForFundAccountCreation => GetSwitchValue(SwitchKey.AllowedAgreementStatusForFundAccountCreation);
        public static List<string> AllowedAgreementTypesForReceivingFundAccounts => GetSwitchValue(SwitchKey.AllowedAgreementTypesForReceivingFundAccounts);
        public static List<string> TreasuryReportAgreementTypesToUseMarginExcessOrDeficit => GetSwitchValue(SwitchKey.TreasuryReportAgreementTypesToUseMarginExcessOrDeficit);
        public static List<string> SwiftBicToEnableField21 => GetSwitchValue(SystemSwitches.SwitchKey.SwiftBicToEnableField21);
        public static List<string> SwiftGroupToIncludeWirePurposeInWireMessage => OpsSecureSwitches.GetSwitchValue(Switches.SwitchKey.SwiftGroupToIncludeWirePurposeInWireMessage);
        public static int SchedulerProcessId => (int)GetSwitchValue(SwitchKey.SchedulerProcessId);

        public static void ResetSwitches()
        {
            InitializeAllSwitches();

            using (var context = new OperationsSecureContext())
            {
                var allSwitchValues = context.hmsSystemSwitches.ToList();
                foreach (var systemSwitch in AllSwitches)
                {
                    var overridenValue = allSwitchValues.FirstOrDefault(s => s.Key == systemSwitch.Key.ToString());

                    if (overridenValue != null)
                        systemSwitch.Value = overridenValue.Value;
                }
            }
        }

        public static dynamic GetSwitchValue(SwitchKey switchKey)
        {
            return SystemSwitches.GetSwitchValue(GetSwitch(switchKey));
        }

        private static Switch GetSwitch(SwitchKey switchKey)
        {
            using (var context = new OperationsSecureContext())
            {
                var swtch = context.hmsSystemSwitches.FirstOrDefault(s => s.Key == switchKey.ToString());
                var thisSwitch = AllSwitches.First(s => s.Key == switchKey);

                if (swtch != null)
                    thisSwitch.Value = swtch.Value;

                return thisSwitch;
            }
        }

        private static string GetSwitchModule(SwitchKey switchKey)
        {
            return AllSwitches.First(s => s.Key == switchKey).Module;
        }

        public static void SetSwitch(SwitchKey switchKey, string value, string modifiedBy)
        {
            using (var context = new OperationsSecureContext())
            {
                var switchModule = GetSwitchModule(switchKey);

                var switchData = new hmsSystemSwitch()
                {
                    Key = switchKey.ToString(),
                    Value = value,
                    Module = switchModule,
                    LastModifiedBy = modifiedBy,
                    LastModifiedDt = DateTime.Now
                };

                context.hmsSystemSwitches.AddOrUpdate(s => new { s.Key }, switchData);
                context.SaveChanges();
            }

            var thisSwitch = AllSwitches.First(s => s.Key == switchKey);
            thisSwitch.Value = value;
        }

        public static List<string> AllModules => AllSwitches.Select(s => s.Module).Distinct<string>().ToList();
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeMark.Operations.Secure.DataModel;

namespace HMOSecureMiddleware
{
    public class PreferencesManager
    {
        public static readonly string ShowRiskOrShortFundNames = "CONFIG:ShowRiskOrShortFundNames";

        public enum FundNameInDropDown
        {
            OpsShortName = 0, LegalFundName = 1, ClientFundName = 2, HMRAName = 3
        }

        public static FundNameInDropDown GetPreferredFundName(string userName)
        {
            var key = ShowRiskOrShortFundNames;
            using (var context = new OperationsContext())
            {
                var preferredFundName = context.dmaUserPreferences.Where(up => up.UserId == userName && up.Key == key).Select(s => s.Value).FirstOrDefault() ?? "0";
                return (FundNameInDropDown)Convert.ToInt32(preferredFundName);
            }
        }


        public enum SystemPreferences
        {
            AllowedAgreementTypesForAccounts
        }


        public static string GetSystemPreference(SystemPreferences preference)
        {
            using (var context = new OperationsSecureContext())
            {
                var pref = context.hmsSystemPreferences.FirstOrDefault(s => s.SystemKey == preference.ToString()) ?? new hmsSystemPreference();
                return pref.SystemValue;
            }
        }

    }
}

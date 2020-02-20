using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeMark.Operations.Secure.DataModel;

namespace HMOSecureMiddleware
{
    public class PreferencesManager
    {
        public enum SystemPreferences
        {
            AllowedAgreementTypesForAccounts,
            ReceivingAgreementTypesForAccount
        }

        public static readonly string ShowRiskOrShortFundNames = "CONFIG:ShowRiskOrShortFundNames";

        public static List<string> AllPreferrenceKeys = new List<string>()
        {
            ShowRiskOrShortFundNames
        };

        public enum FundNameInDropDown
        {
            OpsShortName = 0, LegalFundName = 1, ClientFundName = 2, HMRAName = 3
        }

        public static List<dmaUserPreference> GetAllUserPreferences(int hmUserId)
        {
            using (var context = new OperationsContext())
            {
                return context.dmaUserPreferences.Where(s => s.hmUserId == hmUserId && AllPreferrenceKeys.Contains(s.Key)).ToList();
            }
        }

        public static void SaveUserPreferences(int hmUserId, string key, string value)
        {
            var preference = new dmaUserPreference() { Key = key, Value = value, hmUserId = hmUserId, UserId = string.Empty };
            SaveUserPreferences(preference);
        }

        private static void SaveUserPreferences(dmaUserPreference userPreference)
        {
            userPreference.RecCreatedAt = DateTime.Now;

            using (var context = new OperationsContext())
            {
                context.dmaUserPreferences.AddOrUpdate(s => new { s.Key, s.hmUserId }, userPreference);
            }
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

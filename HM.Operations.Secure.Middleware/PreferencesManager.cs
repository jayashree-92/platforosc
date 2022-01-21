using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using HM.Operations.Secure.DataModel;

namespace HM.Operations.Secure.Middleware
{
    public class PreferencesManager
    {
        public static readonly string ShowRiskOrShortFundNames = "CONFIG:ShowRiskOrShortFundNames";
        public static readonly string FavoriteDashboardTemplateForWires = "FavoriteDashboardTemplateForWires";
        public static readonly string LocalQaMailListKey = "CONFIG:LocalOrQa:USERS";

        private static readonly List<string> AllPreferenceKeys = new List<string>()
        {
            ShowRiskOrShortFundNames,
            FavoriteDashboardTemplateForWires
        };

        public enum FundNameInDropDown
        {
            OpsShortName = 0, LegalFundName = 1, ClientFundName = 2, HMRAName = 3
        }

        public static List<dmaUserPreference> GetAllUserPreferences(int hmUserId)
        {
            using (var context = new OperationsContext())
            {
                return context.dmaUserPreferences.Where(s => s.hmUserId == hmUserId && AllPreferenceKeys.Contains(s.Key)).ToList();
            }
        }

        public static void SaveUserPreferences(int hmUserId, string key, string value)
        {
            var preference = new dmaUserPreference() { Key = key, Value = value, hmUserId = hmUserId };
            SaveUserPreferences(preference);
        }

        private static void SaveUserPreferences(dmaUserPreference userPreference)
        {
            userPreference.RecCreatedAt = DateTime.Now;

            using (var context = new OperationsContext())
            {
                context.dmaUserPreferences.AddOrUpdate(s => new { s.Key, s.hmUserId }, userPreference);
                context.SaveChanges();
            }
        }
        
        //This setting is applicable only for Lower environment and not for PRODUCTION
        public static string GetLocalQaUsers()
        {

            using (var context = new OperationsContext())
            {
                var userPreferenceData = context.dmaUserPreferences.Where(up => up.Key == LocalQaMailListKey).Select(v => v.Value).FirstOrDefault();
                return userPreferenceData;
            }
        }

    }
}

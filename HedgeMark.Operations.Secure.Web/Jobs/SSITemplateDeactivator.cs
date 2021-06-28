using System;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using Com.HedgeMark.Commons;
using HedgeMark.Operations.Secure.DataModel;
using log4net;

namespace HMOSecureWeb.Jobs
{
    public class SSITemplateDeactivator : OperationsSecureSystemSchedule
    {
        public new const string JobName = "SSITemplate-Deactivator";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(SSITemplateDeactivator));

        private static int DeactivationPeriodInMonths
        {
            get
            {
                return ConfigurationManagerWrapper.IntegerSetting("DeactivationPeriodInMonths", 12);
            }
        }
        [DisplayName("SSITemplate-Deactivator")]
        public static void DeacitvateStaleSSITemplates()
        {
            var deactivationDeadline = DateTime.Today.AddMonths(-DeactivationPeriodInMonths);
            using (var context = new OperationsSecureContext())
            {
                var allSsIDateMap = context.hmsWires.Where(s => s.OnBoardSSITemplateId != null && s.OnBoardSSITemplateId > 0).GroupBy(s => s.OnBoardSSITemplateId ?? 0).ToDictionary(s => s.Key, v => v.Max(v1 => v1.CreatedAt.DateTime));
                var lastUsedToDeactivateMap = allSsIDateMap.Where(s => s.Value < deactivationDeadline).ToDictionary(s => s.Key, v => v.Value);

                var staleSSIIds = lastUsedToDeactivateMap.Keys.ToList();
                var allStaleSSITemplatesToDeactivate = context.onBoardingSSITemplates.Where(s => s.SSITemplateStatus == "Active" && staleSSIIds.Contains(s.onBoardingSSITemplateId)).ToList();


                if (allStaleSSITemplatesToDeactivate.Count == 0)
                    return;

                foreach (var staleSSI in allStaleSSITemplatesToDeactivate)
                {
                    staleSSI.SSITemplateStatus = "De-Activated";
                    staleSSI.LastUsedAt = lastUsedToDeactivateMap.ContainsKey(staleSSI.onBoardingSSITemplateId) ? lastUsedToDeactivateMap[staleSSI.onBoardingSSITemplateId] : deactivationDeadline;
                }

                context.onBoardingSSITemplates.AddOrUpdate(s => new { s.onBoardingSSITemplateId }, allStaleSSITemplatesToDeactivate.ToArray());
                context.SaveChanges();
            }
        }
    }
}
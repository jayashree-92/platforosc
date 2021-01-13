using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using Com.HedgeMark.Commons;
using Hangfire;
using HedgeMark.Operations.Secure.DataModel;
using Hangfire;
using HMOSecureWeb.Utility;
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
                var allSsIDateMap = context.hmsWires.Where(s => s.OnBoardSSITemplateId != null && s.OnBoardSSITemplateId > 0).GroupBy(s => s.OnBoardSSITemplateId ?? 0).ToDictionary(s => s.Key, v => v.Max(v1 => v1.CreatedAt));
                var lastUsedToDeactivateMap = allSsIDateMap.Where(s => s.Value < deactivationDeadline).ToDictionary(s => s.Key, v => v.Value);
                var allStaleSSITemplatesToDeactivate = context.onBoardingSSITemplates.Where(s => s.SSITemplateStatus == "Active" && lastUsedToDeactivateMap.ContainsKey(s.onBoardingSSITemplateId)).ToList();

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
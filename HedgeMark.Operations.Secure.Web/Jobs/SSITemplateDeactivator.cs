using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using Com.HedgeMark.Commons;
using Hangfire;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
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
                //Get all Wires' SSI initiated beteen now and Deactivation Time period
                var allUsedSsIs = context.hmsWires
                    .Where(s => s.CreatedAt > deactivationDeadline && s.OnBoardSSITemplateId != null && s.OnBoardSSITemplateId > 0).Select(s => s.OnBoardSSITemplateId ?? 0).Distinct()
                    .ToList();

                var allStaleSSITemplatesToDeactivate = context.onBoardingSSITemplates.Where(s => s.SSITemplateStatus == "Active" && !allUsedSsIs.Contains(s.onBoardingSSITemplateId)).ToList();

                var allStaleSSITemplateIds = allStaleSSITemplatesToDeactivate.Select(s => s.onBoardingSSITemplateId).Distinct().ToList();
                var lastUsedMap = context.hmsWires
                    .Where(s => allStaleSSITemplateIds.Contains(s.OnBoardSSITemplateId ?? 0))
                    .GroupBy(s => s.OnBoardSSITemplateId ?? 0).ToDictionary(s => s.Key, v => v.Max(s1 => s1.CreatedAt));

                allStaleSSITemplatesToDeactivate.ForEach(s =>
                {
                    s.SSITemplateStatus = "De-Activated";
                    s.LastUsedAt = lastUsedMap.ContainsKey(s.onBoardingSSITemplateId) ?lastUsedMap[s.onBoardingSSITemplateId] : deactivationDeadline;
                });

                context.onBoardingSSITemplates.AddOrUpdate(s => new { s.onBoardingSSITemplateId }, allStaleSSITemplatesToDeactivate.ToArray());
                context.SaveChanges();
            }
        }
    }
}
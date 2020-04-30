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
            var currentDate = DateTime.Now;
            using (var context = new OperationsSecureContext())
            {
                var allStaleSSITemplates = (from ssi in context.onBoardingSSITemplates
                                            join wire in context.hmsWires on ssi.onBoardingSSITemplateId equals wire.OnBoardSSITemplateId
                                            where DbFunctions.DiffMonths(wire.CreatedAt, currentDate) > DeactivationPeriodInMonths
                                            select new { ssi, wire.CreatedAt }).ToList();

                allStaleSSITemplates.ForEach(s =>
                {
                    s.ssi.SSITemplateStatus = "De-Activated";
                    s.ssi.LastUsedAt = s.CreatedAt;
                });

                context.onBoardingSSITemplates.AddOrUpdate(s => new { s.onBoardingSSITemplateId }, allStaleSSITemplates.Select(s => s.ssi).ToArray());
                context.SaveChanges();
            }
        }
    }
}
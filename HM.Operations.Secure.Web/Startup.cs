using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Com.HedgeMark.Commons;
using Com.HedgeMark.Commons.Extensions;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using HM.Operations.Secure.DataModel.Models;
using HM.Operations.Secure.Web;
using HM.Operations.Secure.Web.Controllers;
using HM.Operations.Secure.Web.Jobs;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace HM.Operations.Secure.Web
{
    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return HttpContext.Current.User.Identity.IsAuthenticated && AccountController.AllowedUserRoles.Any(role => HttpContext.Current.User.IsInRole(role));
        }
    }
    public class Startup
    {
        public static readonly int HangFireWorkerCount = ConfigurationManagerWrapper.IntegerSetting("HangFireWorkerCount", Environment.ProcessorCount * 4);
        private static readonly string HangfireConnectionString = ConfigurationManagerWrapper.GetConnectionString("HangfireContext");
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(HangfireConnectionString,
                     new SqlServerStorageOptions
                     {
                         CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                         SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                         QueuePollInterval = TimeSpan.Zero,
                         UseRecommendedIsolationLevel = true,
                         UsePageLocksOnDequeue = true,
                         DisableGlobalLocks = true,
                         SchemaName = "HMOpsSecure"
                     });


            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 5 });

            var options = new DashboardOptions
            {
                Authorization = new[] { new HangFireAuthorizationFilter() },
                DashboardTitle = "HM-Operations-Secure Jobs Dashboard"
            };

            app.UseHangfireDashboard("/jobs", options);

            var queues = new List<string> { Environment.MachineName, Middleware.Util.Utility.Environment, "default" }.Distinct().Select(s => s.ToLower()).ToArray();

            app.UseHangfireServer(new BackgroundJobServerOptions { Queues = queues, WorkerCount = HangFireWorkerCount });

            Task.Factory.StartNew(BackGroundJobScheduler.StartAll, TaskCreationOptions.LongRunning);
        }
    }
}

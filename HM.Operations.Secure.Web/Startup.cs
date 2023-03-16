using Com.HedgeMark.Commons;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using HM.Authentication;
using HM.Operations.Secure.Web;
using HM.Operations.Secure.Web.Utility;
using HM.Operations.Secure.Web.Jobs;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

[assembly: OwinStartup(typeof(Startup))]

namespace HM.Operations.Secure.Web
{
    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return HttpContext.Current.User.IsWireUser();
        }
    }
    public class Startup
    {
        public static readonly int HangFireWorkerCount = ConfigurationManagerWrapper.IntegerSetting("HangFireWorkerCount", Environment.ProcessorCount * 4);
        private static readonly string HangfireConnectionString = ConfigurationManagerWrapper.GetConnectionString("HangfireContext");
        private static bool IsLocalHost = ConfigurationManagerWrapper.BooleanSetting("IsLocalHost", true);
        public void Configuration(IAppBuilder app)
        {
            AzureADAuthentication.ConfigureADAuth(app, Utility.Util.Environment, ConfigurationManagerWrapper.AppName, IsLocalHost);

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

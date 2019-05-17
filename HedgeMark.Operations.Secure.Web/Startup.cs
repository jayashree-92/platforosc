using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Com.HedgeMark.Commons;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using HedgeMark.Operations.Secure.DataModel.Models;
using HMOSecureWeb.Controllers;
using HMOSecureWeb.Jobs;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HMOSecureWeb.Startup))]

namespace HMOSecureWeb
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
        public static readonly int HangFirePollingInterval = ConfigurationManagerWrapper.IntegerSetting("hangfirePollingInterval", 30);
        public void Configuration(IAppBuilder app)
        {
            var sqlOptions = new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(HangFirePollingInterval) };
            GlobalConfiguration.Configuration.UseSqlServerStorage(new OperationsSecureSettings().ConnectionString, sqlOptions);
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 5 });

            var options = new DashboardOptions
            {
                Authorization = new[] { new HangFireAuthorizationFilter() },
            };

            app.UseHangfireDashboard("/jobs", options);

            var queues = new List<string> { Environment.MachineName, HMOSecureMiddleware.Util.Utility.Environment }.Distinct().Select(s => s.ToLower()).ToArray();

            app.UseHangfireServer(new BackgroundJobServerOptions { Queues = queues, WorkerCount = HangFireWorkerCount });

            Task.Factory.StartNew(BackGroundJobScheduler.StartAll, TaskCreationOptions.LongRunning);
        }
    }
}

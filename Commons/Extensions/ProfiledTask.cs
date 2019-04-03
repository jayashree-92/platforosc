using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using log4net;

namespace Com.HedgeMark.Commons.Extensions
{
    public class ProfiledTask : IDisposable
    {
        private readonly ILog logger;
        private readonly Stopwatch stopwatch;
        public ProfiledTask(string taskName = null, [CallerMemberName] string caller = "ProfiledTask")
        {
            logger = LogManager.GetLogger(taskName ?? caller);
            stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            stopwatch.Stop();
            logger.InfoFormat("{0} took {1} milliseconds", logger.Logger.Name, stopwatch.ElapsedMilliseconds);
        }
    }
}

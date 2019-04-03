using System;
using System.Reflection;
using HMOSecureMiddleware.Queues;
using log4net;

namespace HMOSecureMiddleware
{
    public static class BootUpMiddleware
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BootUpMiddleware));

        public static void BootUp()
        {
            if (Utility.IsLocal())
                return;

            QueueSystemManager.EstablishConnection();
            Logger.Info("MQ Connection establishment - Complete");
        }
    }
}
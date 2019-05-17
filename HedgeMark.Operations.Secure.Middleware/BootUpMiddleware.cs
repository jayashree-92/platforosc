using HMOSecureMiddleware.Queues;
using HMOSecureMiddleware.Util;
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
        }
    }
}
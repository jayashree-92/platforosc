using HM.Operations.Secure.Middleware.Queues;
using HM.Operations.Secure.Middleware.Util;
using log4net;

namespace HM.Operations.Secure.Middleware
{
    public static class BootUpMiddleware
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BootUpMiddleware));

        public static void BootUp()
        {
            if (Utility.IsLocal)
                return;

            QueueSystemManager.EstablishConnection();
        }
    }
}
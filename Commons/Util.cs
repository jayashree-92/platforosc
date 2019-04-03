
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Com.HedgeMark.Commons
{
    public class Util
    {
        private static readonly bool IsParallelismEnabled = ConfigurationManagerWrapper.BooleanSetting(Config.EnableParallelism);

        public static void RunInParallel<T>(IEnumerable<T> items, Action<T> codeToRun)
        {
            if (IsParallelismEnabled)
                RunParallelly(items, codeToRun);
            else
                RunSequentially(items, codeToRun);
        }

        private static void RunSequentially<T>(IEnumerable<T> items, Action<T> codeToRun)
        {
            foreach (T currentItem in items)
            {
                codeToRun(currentItem);
            }
        }

        private static void RunParallelly<T>(IEnumerable<T> items, Action<T> codeToRun)
        {
            Parallel.ForEach(items, codeToRun);
        }
    }
}

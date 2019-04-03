
namespace Com.HedgeMark.Commons
{
    public class Synchroniser
    {
        private static int staticInt;
        private static object thisLock;

        static Synchroniser()
        {
            staticInt = 1000;
            thisLock = new object();
        }

        public static int SyncInt
        {
            get
            {
                lock (thisLock)
                {
                    if (staticInt == 9999)
                        staticInt = 1000;
                    return ++staticInt;
                }
            }
        }
    }
}
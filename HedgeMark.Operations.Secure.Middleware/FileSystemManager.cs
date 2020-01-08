using System.IO;
using Com.HedgeMark.Commons;
using System.Collections.Generic;
using HedgeMark.Operations.Secure.DataModel;
using System.Linq;

namespace HMOSecureMiddleware
{
    public class FileSystemManager
    {
        public static string OpsSecureRootDirectory
        {
            get
            {
                var configPath = ConfigurationManagerWrapper.StringSetting("OpsSecureRootDirectory", @"C:\ManagedAccountFiles\");
                return GetValidatedConfigPath(configPath);
            }
        }

        public static string OpsSecureWiresFilesPath
        {

            get
            {
                var configPath = ConfigurationManagerWrapper.StringSetting("OpsSecureWiresPath", string.Format(@"{0}\{1}\", OpsSecureRootDirectory, "Wires"));
                return GetValidatedConfigPath(configPath);
            }
        }

        public static string UploadTemporaryFilesPath
        {
            get
            {
                var configPath = string.Format(@"{0}\SecureUploads\", OpsSecureRootDirectory);
                return GetValidatedConfigPath(configPath);
            }
        }

        public static string OpsSecureAccountsFileUploads
        {
            get
            {
                var configPath = string.Format(@"{0}\AccountsFileUploads\", OpsSecureRootDirectory);
                return GetValidatedConfigPath(configPath);
            }
        }

        public static string OpsSecureSSITemplateFileUploads
        {
            get
            {
                var configPath = string.Format(@"{0}\SSITemplateFileUploads\", OpsSecureRootDirectory);
                return GetValidatedConfigPath(configPath);
            }
        }

        public static string OpsSecureBulkFileUploads
        {
            get
            {
                var configPath = string.Format(@"{0}\BulkFileUploads\", OpsSecureRootDirectory);
                return GetValidatedConfigPath(configPath);
            }
        }

        public static string DefaultTimeZone
        {
            get
            {
                return ConfigurationManagerWrapper.StringSetting("DefaultTimeZone", "EST");
            }
        }

        private static readonly object InitialiseLock;

        static FileSystemManager()
        {
            InitialiseLock = new object();
            Initialise();

            if (!Directory.Exists(UploadTemporaryFilesPath))
                Directory.CreateDirectory(UploadTemporaryFilesPath);

            if (!Directory.Exists(OpsSecureWiresFilesPath))
                Directory.CreateDirectory(OpsSecureWiresFilesPath);

            if (!Directory.Exists(OpsSecureSSITemplateFileUploads))
                Directory.CreateDirectory(OpsSecureSSITemplateFileUploads);
        }

        private static void Initialise()
        {
            lock (InitialiseLock)
            {
                DmaReports = GetAllReports();
                //InitializeManagedAccounts();
            }
        }

        public static List<dmaReport> DmaReports { get; set; }
        public static Dictionary<long, string> AllReports
        {
            get
            {
                return DmaReports.OrderBy(s => s.DisplayOrder).ToDictionary(s => s.dmaReportsId, v => v.ReportName);
            }
        }

        private static List<dmaReport> GetAllReports()
        {
            using (var context = new OperationsContext())
            {
                return context.dmaReports.OrderBy(s => s.DisplayOrder).ToList();
            }
        }

        public static Dictionary<string, string> GetAllTimeZones()
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsWireCutoffTimeZones.OrderBy(s => s.hmsWireCutoffTimeZoneId).ToDictionary(s => s.TimeZone, v => v.TimeZoneStandardName);
            }
        }

        public static string GetReportName(long reportId)
        {
            return AllReports.FirstOrDefault(k => k.Key == reportId).Value;
        }

        public static long GetReportId(string reportName)
        {
            return AllReports.FirstOrDefault(v => v.Value == reportName).Key;
        }


        public static string GetValidatedConfigPath(string configPath)
        {
            if (configPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new FileLoadException(string.Format("Invalid file path : {0}", configPath));

            return configPath;
        }

    }
}

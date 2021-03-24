using System.Collections.Generic;
using System.IO;
using System.Linq;
using Com.HedgeMark.Commons;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware.Models;

namespace HedgeMark.Operations.Secure.Middleware
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

        public static string ManagedAccountRootDirectory
        {
            get
            {
                var configPath = ConfigurationManagerWrapper.StringSetting("ManagedAccountRootDirectory", @"D:\ManagedAccountFiles\");
                return GetValidatedConfigPath(configPath);
            }
        }
        public static string InternalOutputFilesDropPath
        {
            get
            {
                var configPath = ConfigurationManagerWrapper.StringSetting("InternalOutputFilesDropPath", @"D:\InternalOutputFilesDropPath\");
                return GetValidatedConfigPath(configPath);
            }
        }

        public static string SftpOutputFilesPath
        {
            get
            {
                var configPath = ConfigurationManagerWrapper.StringSetting("SftpOutputFilesPath", string.Format(@"{0}\{1}\", OpsSecureRootDirectory, "SftpOutputFilesPath"));
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

        public static string InvoicesFileAttachement
        {
            get
            {
                var configPath = string.Format(@"{0}\Invoices\FileAttachement\", ManagedAccountRootDirectory);
                return GetValidatedConfigPath(configPath);
            }
        }
        public static string InternalConfigFiles
        {
            get
            {
                var configPath = string.Format(@"{0}\InternalConfigFiles\", ManagedAccountRootDirectory);
                return GetValidatedConfigPath(configPath);
            }
        }
        public static string RawFilesOverridesPath
        {
            get
            {
                var configPath = string.Format(@"{0}\Overrides\", ManagedAccountRootDirectory);
                return GetValidatedConfigPath(configPath);
            }
        }
        public static string SftpRawFilesOfHM
        {
            get
            {
                var configPath = ConfigurationManagerWrapper.StringSetting("SftpRawFilesOfHM", string.Format(@"{0}\{1}\{2}\", ManagedAccountRootDirectory, "SftpOutputFilesPath", "HM"));
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

            if (!Directory.Exists(InternalOutputFilesDropPath))
                Directory.CreateDirectory(InternalOutputFilesDropPath);
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

        public static List<Select2Type> FetchFolderData(string path, bool shouldshowRootFiles = true)
        {
            var dirInfo = new DirectoryInfo(path);
            var index = 0;
            var folders = new List<Select2Type>();

            if (shouldshowRootFiles)
                folders.Add(new Select2Type { id = "-Root-", text = "-Root-" });

            var rootDirectories = dirInfo.GetDirectories().ToList();

            var folderNames = rootDirectories.Select(s => s.Name).ToList();
            folders.AddRange(folderNames.Select(s => new Select2Type { id = s, text = s }).OrderBy(s1 => s1.text).ToList());
            return folders;
        }

    }
}

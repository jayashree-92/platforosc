using System.Collections.Generic;
using System.IO;
using System.Linq;
using Com.HedgeMark.Commons;
using Com.HedgeMark.Commons.Extensions;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware.Models;

namespace HM.Operations.Secure.Middleware
{
    public class FileSystemManager
    {
        public static string OpsSecureRootDirectory => ConfigurationManagerWrapper.StringSetting("OpsSecureRootDirectory", @"D:\HM-Operations-Secure\").GetValidatedConfigPath();

        public static string ManagedAccountRootDirectory => ConfigurationManagerWrapper.StringSetting("ManagedAccountRootDirectory", @"D:\HM-Operations\ManagedAccountFiles\").GetValidatedConfigPath();

        public static string InternalOutputFilesDropPath => ConfigurationManagerWrapper.StringSetting("InternalOutputFilesDropPath", @"D:\InternalOutputFilesDropPath\").GetValidatedConfigPath();

        public static string SftpOutputFilesPath => ConfigurationManagerWrapper.StringSetting("SftpOutputFilesPath", $@"{OpsSecureRootDirectory}\{"SftpOutputFilesPath"}\").GetValidatedConfigPath();

        public static string OpsSecureWiresFilesPath => ConfigurationManagerWrapper.StringSetting("OpsSecureWiresPath", $@"{OpsSecureRootDirectory}\{"Wires"}\").GetValidatedConfigPath();

        public static string UploadTemporaryFilesPath => $@"{OpsSecureRootDirectory}\SecureUploads\".GetValidatedConfigPath();

        public static string OpsSecureAccountsFileUploads => $@"{OpsSecureRootDirectory}\AccountsFileUploads\".GetValidatedConfigPath();

        public static string OpsSecureInternalConfigFiles => $@"{OpsSecureRootDirectory}\InternalConfigFiles\".GetValidatedConfigPath();

        public static string OpsSecureSSITemplateFileUploads => $@"{OpsSecureRootDirectory}\SSITemplateFileUploads\".GetValidatedConfigPath();

        public static string OpsSecureBulkFileUploads => $@"{OpsSecureRootDirectory}\BulkFileUploads\".GetValidatedConfigPath();

        public static string InvoicesFileAttachement => $@"{ManagedAccountRootDirectory}\Invoices\FileAttachement\".GetValidatedConfigPath();

        public static string InternalConfigFiles => $@"{ManagedAccountRootDirectory}\InternalConfigFiles\".GetValidatedConfigPath();

        public static string RawFilesOverridesPath => $@"{ManagedAccountRootDirectory}\Overrides\".GetValidatedConfigPath();

        public static string SftpRawFilesOfHM => ConfigurationManagerWrapper.StringSetting("SftpRawFilesOfHM", $@"{ManagedAccountRootDirectory}\{"SftpOutputFilesPath"}\{"HM"}\").GetValidatedConfigPath();

        public static string DefaultTimeZone => ConfigurationManagerWrapper.StringSetting("DefaultTimeZone", "EST");

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

            if (!Directory.Exists(OpsSecureInternalConfigFiles))
                Directory.CreateDirectory(OpsSecureInternalConfigFiles);
        }

        private static void Initialise()
        {
            lock (InitialiseLock)
            {
                DmaReports = GetAllReports();
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

        public static List<Select2Type> FetchFolderData(string path, bool shoulShowRootFiles = true)
        {
            var dirInfo = new DirectoryInfo(path);
            var index = 0;
            var folders = new List<Select2Type>();

            if (shoulShowRootFiles)
                folders.Add(new Select2Type { id = "-Root-", text = "-Root-" });

            var rootDirectories = dirInfo.GetDirectories().ToList();

            var folderNames = rootDirectories.Select(s => s.Name).ToList();
            folders.AddRange(folderNames.Select(s => new Select2Type { id = s, text = s }).OrderBy(s1 => s1.text).ToList());
            return folders;
        }

    }
}

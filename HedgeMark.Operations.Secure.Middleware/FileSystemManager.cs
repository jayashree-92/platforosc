using System.IO;
using Com.HedgeMark.Commons;

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
                var configPath = ConfigurationManagerWrapper.StringSetting("OpsSecureUploadFilesPath", string.Format(@"{0}SecureUploads\", OpsSecureRootDirectory));
                return GetValidatedConfigPath(configPath);
            }
        }

        public static string OpsSecureAccountsFileUploads
        {
            get
            {
                var configPath = ConfigurationManagerWrapper.StringSetting("OpsSecureAccountsFileUploads", string.Format(@"{0}AccountsFileUploads\", OpsSecureRootDirectory));
                return GetValidatedConfigPath(configPath);
            }
        }

        public static string OpsSecureSSITemplateFileUploads
        {
            get
            {
                var configPath = ConfigurationManagerWrapper.StringSetting("OpsSecureSSITemplateFileUploads", string.Format(@"{0}SSITemplateFileUploads\", OpsSecureRootDirectory));
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

        static FileSystemManager()
        {
            if (!Directory.Exists(UploadTemporaryFilesPath))
                Directory.CreateDirectory(UploadTemporaryFilesPath);

            if (!Directory.Exists(OpsSecureWiresFilesPath))
                Directory.CreateDirectory(OpsSecureWiresFilesPath);

            if (!Directory.Exists(OpsSecureSSITemplateFileUploads))
                Directory.CreateDirectory(OpsSecureSSITemplateFileUploads);
        }


        private static string GetValidatedConfigPath(string configPath)
        {
            if (configPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new FileLoadException(string.Format("Invalid file path : {0}", configPath));

            return configPath;
        }

    }
}

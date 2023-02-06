using Com.HedgeMark.Commons.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Com.HedgeMark.Commons
{
    public class ExternalSftpTransfer
    {
        public static DirectoryInfo ExternalFileDropLocation => new DirectoryInfo(ConfigurationManagerWrapper.StringSetting("ExternalSftpOutgoingFileDropPath", "C://ManagedAccountsFilePath//ExternalSftpOutgoingFileDropPath/Ops-Secure/"));
        public static List<string> ExternalClients { get; set; }

        static ExternalSftpTransfer()
        {
            ExternalClients = ExternalFileDropLocation.GetDirectories().Select(s => s.Name).ToList();
        }

        public static bool SendFilesToClients(List<FileInfo> filesToDeliver, string clientFolderName)
        {
            if(!ExternalFileDropLocation.Exists)
                return false;

            var clientDirInfo = new DirectoryInfo($"{ExternalFileDropLocation.FullName}//{clientFolderName}");

            if(!clientDirInfo.Exists)
                clientDirInfo.Create();

            foreach(var reportFile in filesToDeliver)
            {
                var fileToDeliver = $"{clientDirInfo.FullName}\\{reportFile.Name}".GetValidatedConfigPath();
                File.Copy(reportFile.FullName, fileToDeliver, true);
            }

            return true;
        }
    }
}

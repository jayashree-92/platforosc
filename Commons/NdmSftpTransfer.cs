using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Com.HedgeMark.Commons
{
    public class NdmSftpTransfer
    {
        private static List<NdmSftpClient> Clients;

        private static string CustomClientSftpConfigFile
        {
            get
            {
                return ConfigurationManagerWrapper.StringSetting("SFTPClientsConfig", AppDomain.CurrentDomain.BaseDirectory + "\\Config\\CustomClientSFTP.csv");
            }
        }

        private static string BatchName
        {
            get { return ConfigurationManagerWrapper.StringSetting("NDMBatchFile", @"D:\Apps\NDM\batch\NDMAUTO.bat"); }
        }

        private static string BatchPath
        {
            get { return ConfigurationManagerWrapper.StringSetting("NDMBatchFolder", @"D:\Apps\NDM\batch\"); }
        }

        private static string ScriptPath
        {
            get { return ConfigurationManagerWrapper.StringSetting("NDMScriptFolder", @"D:\Apps\NDM\Process_Lib\"); }
        }

        private static int ArchivalDelayDuration
        {
            get { return ConfigurationManagerWrapper.IntegerSetting("ArchivalDelayDuration", 1000 * 10); }
        }

        static NdmSftpTransfer()
        {
            InitialiseClients();
        }

        public static void InitialiseClients()
        {
            Clients = new List<NdmSftpClient>();

            using (var reader = new StreamReader(File.OpenRead(CustomClientSftpConfigFile)))
            {
                while (!reader.EndOfStream)
                {
                    var sftpClient = new NdmSftpClient();

                    var line = reader.ReadLine() ?? string.Empty;
                    var parts = line.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length <= 2) continue;
                    sftpClient.ClientName = parts[0];
                    sftpClient.SourceFolder = parts[1];
                    sftpClient.ScriptName = parts[2];

                    if (parts.Length == 4)
                        sftpClient.ArchiveFolder = parts[3];

                    Clients.Add(sftpClient);
                }
            }
        }

        public static dynamic GetClientsForSftp()
        {
            return Clients.Select(client => new { id = client.ClientName, text = client.ClientName })
                    .OrderBy(c => c.text)
                    .ToList();
        }

        public static List<string> GetProcFileNames()
        {
            var procFileNames = new List<string>();
            // Added hardcoded values for testing
            if (!Directory.Exists(ScriptPath))
                return new List<string>() { "ABC", "DEF", "GHI" };
            //return procFileNames;
            procFileNames = Directory.GetFiles(ScriptPath, "*.proc")
                .Select(name => new FileInfo(name).Name.Replace(".proc", string.Empty))
                .ToList();
            procFileNames.AddRange(Directory.GetFiles(ScriptPath, "*.PROC").Select(name => new FileInfo(name).Name.Replace(".PROC", string.Empty)));
            procFileNames = procFileNames.Distinct().OrderBy(x => x).ToList();
            return procFileNames;
        }

        public static void ExecuteProcFile(string fileName)
        {
            using (Process process = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = BatchName,
                    Arguments = fileName,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }

        public static string GetArchiveLocation(string clientName)
        {
            var client = Clients.FirstOrDefault(s => s.ClientName == clientName);
            return client == null ? string.Empty : client.ArchiveFolder;
        }

        public static Boolean SendFilesToSftp(FileInfo[] files, string client, ref StringBuilder logs, bool shouldClearSourceFolder = false)
        {
            var status = false;
            var selectedClient = GetClientDetails(client);

            try
            {
                logs.AppendLine("SFTP Process Started for " + client + " - " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                logs.AppendLine("Checking for the availability of " + client + " client details");
                if (selectedClient == null)
                {
                    logs.AppendLine("Client Details not available");
                }
                else
                {
                    logs.AppendLine("Client details available");

                    //var folders = new[] { selectedClient.SourceFolder, BatchPath, ScriptPath, selectedClient.ArchiveFolder };
                    var folders = new[] { selectedClient.SourceFolder, BatchPath, ScriptPath };
                    var scriptFullName = string.Format("{0}{1}.proc", ScriptPath, selectedClient.ScriptName);

                    logs.AppendLine("Checking for the existency of folders and proc file");
                    if (CheckFoldersExist(folders) && File.Exists(scriptFullName))
                    {
                        // Clear source folder if required for the selected client
                        if (shouldClearSourceFolder)
                        {
                            DirectoryInfo diInfo = new DirectoryInfo(selectedClient.SourceFolder);
                            logs.AppendLine("Clearing all the existing files in the source folder");
                            foreach (var file in diInfo.GetFiles())
                                file.Delete();
                            logs.AppendLine("Clearing all the existing folders inside the source folder");
                            foreach (var dir in diInfo.GetDirectories())
                                dir.Delete(true);
                        }

                        logs.AppendLine("Copying files to source folder from where files will be pushed to SFTP");
                        logs.AppendLine("Source Folder: " + selectedClient.SourceFolder);
                        // Save files to a location before sending to SFTP
                        files.ToList()
                            .ForEach(file => File.Copy(file.FullName,
                                Path.Combine(selectedClient.SourceFolder, file.Name), true));

                        // Save files to archive folder
                        if (!string.IsNullOrEmpty(selectedClient.ArchiveFolder))
                        {
                            logs.AppendLine("Copying files to archive folder");
                            logs.AppendLine("Archive Folder: " + selectedClient.ArchiveFolder);
                            files.ToList()
                                .ForEach(file => File.Copy(file.FullName,
                                    Path.Combine(selectedClient.ArchiveFolder, file.Name), true));
                        }

                        logs.AppendLine("Executing NDM script " + selectedClient.ScriptName +
                                        " to push the files to SFTP");
                        ExecuteProcFile(selectedClient.ScriptName);

                        logs.AppendLine("SFTP Transfer Completed successfully - " +
                                        DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

                        status = true;
                    }
                    else
                    {
                        logs.AppendLine("Either folders or proc file will not be available");
                    }
                }
            }
            catch (Exception exception)
            {
                logs.AppendLine("SFTP Transfer Process failed. Some exception occurred in SFTP transfer. Please find below for more details:");
                logs.AppendLine(exception.StackTrace);

                if (selectedClient != null && !string.IsNullOrEmpty(selectedClient.ArchiveFolder))
                {
                    logs.AppendLine("Cleaning up archived files due to exception");
                    // Delete files from archive folder
                    files.ToList().ForEach(file =>
                    {
                        var fileName = Path.Combine(selectedClient.ArchiveFolder, file.Name);
                        if (File.Exists(fileName))
                            File.Delete(fileName);
                    });
                }
            }
            finally
            {
                if (selectedClient != null && !string.IsNullOrEmpty(selectedClient.ArchiveFolder))
                {
                    Task.Factory.StartNew(() =>
                      {
                          Thread.Sleep(ArchivalDelayDuration);
                          // Delete files from source folder
                          files.ToList().ForEach(file =>
                            {
                                var fileName = Path.Combine(selectedClient.SourceFolder, file.Name);
                                if (File.Exists(fileName))
                                    File.Delete(fileName);
                            });
                      });
                }
            }
            logs.AppendLine("SFTP Process Completed for " + client + " - " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            return status;
        }

        private static NdmSftpClient GetClientDetails(string client)
        {
            return Clients.FirstOrDefault(c => c.ClientName.Equals(client));
        }

        private static bool CheckFoldersExist(string[] folders)
        {
            var isExist = true;
            folders.ToList().ForEach(folder =>
            {
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                    isExist = false;
            });
            return isExist;
        }

        private class NdmSftpClient
        {
            public NdmSftpClient()
            {
                ArchiveFolder = string.Empty;
                SourceFolder = string.Empty;
            }
            public string ClientName { get; set; }
            public string SourceFolder { get; set; }
            public string ScriptName { get; set; }
            public string ArchiveFolder { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Com.HedgeMark.Commons;
using Com.HedgeMark.Commons.Extensions;
using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using Kent.Boogaart.KBCsv;
using log4net;

namespace HM.Operations.Secure.Middleware
{
    public class ReportDeliveryManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ReportDeliveryManager));

        public static List<Row> ParseAsRows(FileInfo fileInfo, string sheetname, string password, bool isFirstLineHeader)
        {
            return new Parser().ParseAsRows(fileInfo, sheetname, password, isFirstLineHeader);
        }

        public static void CreateExportFile(Dictionary<string, List<Row>> contentToExport, FileInfo exportFileInfo, bool shouldUseExportHeader = true)
        {
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, shouldUseExportHeader);
        }

        public static void CreateExportFile(List<Row> rowData, string tabName, FileInfo exportFileInfo, bool shouldExcludeIsExportable = true)
        {
            var contentToExport = new ExportContent() { Rows = rowData, TabName = tabName };
            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo);
        }

        public static void CreateExportFile(ExportContent contentToExport, FileInfo exportFileInfo, bool shouldExcludeIsExportable = true)
        {
            if (exportFileInfo.Extension.ToLower() == ".csv" || exportFileInfo.Extension.ToLower() == ".txt")
                CreateCsvOrTxtFile(contentToExport.Rows.ToList(), exportFileInfo, contentToExport.IgnoreHeaders, shouldExcludeIsExportable);
            else
                Exporter.CreateExcelFile(new List<ExportContent>() { contentToExport }, exportFileInfo.FullName.GetValidatedConfigPath());
        }

        public static void CreateCsvOrTxtFile(List<Row> contentTable, FileInfo file, List<string> ignoreHeaders = null, bool shouldExcludeIsExportable = true)
        {
            if (ignoreHeaders == null)
                ignoreHeaders = new List<string>();

            var colNames = new List<string>();

            if (contentTable.Any())
                colNames = shouldExcludeIsExportable ? contentTable.First().CellValues.Select(col => col.Key.Name).ToList() : contentTable.First().CellValues.Where(s => s.Key.IsExportable).Select(col => col.Key.Name).ToList();
            else
                colNames.Add("No Data Available");

            colNames = colNames.Where(s => !ignoreHeaders.Contains(s)).ToList();

            using (var writer = new CsvWriter(file.FullName.GetValidatedConfigPath()))
            {
                writer.ValueSeparator = file.Extension.ToLower() == ".csv" ? ',' : '|';
                writer.WriteHeaderRecord(colNames);

                var content = contentTable.Select(row => row.CellValues.Where(cell => cell.Key.IsExportable && colNames.Contains(cell.Key.Name)).Select(cell => Convert.ToString(cell.Value.Value)).ToArray()).ToList();
                writer.WriteDataRecords(content);

                writer.Flush();
                writer.Close();
            }
        }

        public static bool SendReportToInternalDir(string clientFolderName, FileInfo reportFile)
        {
            var deliveryPath = string.Format("{0}\\{1}\\", FileSystemManager.InternalOutputFilesDropPath, clientFolderName).GetValidatedConfigPath();

            var fileToDeliver = string.Format("{0}\\{1}", deliveryPath, reportFile.Name).GetValidatedConfigPath();

            if (!Directory.Exists(deliveryPath))
                Directory.CreateDirectory(deliveryPath);

            if (File.Exists(fileToDeliver))
                File.Delete(fileToDeliver);

            File.Copy(reportFile.FullName.GetValidatedConfigPath(), fileToDeliver, true);

            return true;
        }

        public static bool SftpThisReport(string clientFolderName, FileInfo reportFile)
        {
            var deliveryPath = string.Format("{0}\\{1}\\", FileSystemManager.SftpOutputFilesPath, clientFolderName).GetValidatedConfigPath();

            var fileToDeliver = string.Format("{0}\\{1}", deliveryPath, reportFile.Name).GetValidatedConfigPath();

            if (!Directory.Exists(deliveryPath))
                Directory.CreateDirectory(deliveryPath);

            if (File.Exists(fileToDeliver))
                File.Delete(fileToDeliver);

            File.Copy(reportFile.FullName.GetValidatedConfigPath(), fileToDeliver, true);

            var sftpLog = new StringBuilder();
            var isTransmissionSuccess = NdmSftpTransfer.SendFilesToSftp(new[] { reportFile }, clientFolderName, ref sftpLog);

            if (!isTransmissionSuccess)
                Logger.ErrorFormat("SFTP Transmission Failure Logs ={0}", sftpLog);
            else
                Logger.InfoFormat("SFTP Transmission Logs ={0}", sftpLog);

            return isTransmissionSuccess;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware.Models;

namespace HM.Operations.Secure.Middleware
{
    public class AuditManager
    {
        public static List<hmsUserAuditLog> GetConsolidatedLogs(DateTime startDate, DateTime endDate, string module)
        {
            using (var context = new OperationsSecureContext())
            {
                endDate = endDate.AddDays(1);
                var auditLogs = module.Equals("All")
                    ? context.hmsUserAuditLogs.Where(al => al.CreatedAt >= startDate && al.CreatedAt <= endDate).Select(a => a)
                    : context.hmsUserAuditLogs.Where(al => al.CreatedAt >= startDate && al.CreatedAt <= endDate && al.Module == module).Select(a => a);
                return auditLogs.OrderByDescending(s => s.CreatedAt).ToList();
            }
        }
        public static Dictionary<long, string> GetModuleNames()
        {
            using (var context = new OperationsSecureContext())
            {
                var auditLogs = context.hmsUserAuditLogs.GroupBy(g => g.Module).Select(x => x.FirstOrDefault()).OrderBy(o => o.Module).ToDictionary(id => id.hmsUserAuditLogId, mod => mod.Module);
                return auditLogs;
            }
        }

        public static void LogAudit(AuditLogData auditData, string userName)
        {
            if (auditData.changes == null)
                return;

            var association = GetAssociationToLog(auditData);
            var userAuditLogData = CreateLog(auditData, association, userName);
            Log(userAuditLogData);
        }

        public static string GetAssociationToLog(AuditLogData auditData)
        {
            var association = string.Format("Purpose: <i>{0}</i><br/>Sending Account: <i>{1}</i><br/>Receiving Account: <i>{2}</i><br/>Transfer Type:<i>{3}</i>", auditData.Purpose, auditData.SendingAccount, (string.IsNullOrWhiteSpace(auditData.ReceivingAccount) ? "N/A" : auditData.ReceivingAccount), auditData.TransferType);

            return association;
        }

        public static List<hmsUserAuditLog> CreateLog(AuditLogData auditLogData, string association, string userMakingTheChange)
        {

            return (from change in auditLogData.changes
                    where change != null
                    select new hmsUserAuditLog
                    {
                        Module = auditLogData.ModuleName,
                        Action = auditLogData.Action,
                        UserName = userMakingTheChange,
                        AssociationId = auditLogData.AssociationId,
                        CreatedAt = DateTime.Now,
                        Field = change[1],
                        Log = association,
                        PreviousStateValue = auditLogData.Action == "Added" ? !string.IsNullOrWhiteSpace(change[2]) ? change[2] : string.Empty : change[2],
                        ModifiedStateValue = change[3],
                        IsLogFromOps = false
                    }).ToList();
        }

        public static readonly List<Type> QualifiedTypeToAudit = new List<Type> { typeof(string), typeof(short), typeof(short?), typeof(int), typeof(int?), typeof(long), typeof(long?), typeof(bool), typeof(bool?),
            typeof(decimal), typeof(decimal?), typeof(float), typeof(float?), typeof(TimeSpan), typeof(TimeSpan?) };

        public static List<hmsUserAuditLog> GetAuditLogs(onBoardingSSITemplate ssiTemplate, string accountType, string broker, string userName)
        {
            var nonUpdatedAccount = SSITemplateManager.GetSsiTemplate(ssiTemplate.onBoardingSSITemplateId);

            var propertyInfos = typeof(onBoardingSSITemplate).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(s => QualifiedTypeToAudit.Contains(s.PropertyType));
            var isNewAccount = nonUpdatedAccount == null;

            var list = (from propertyInfo in propertyInfos
                        let propertyVal = (propertyInfo.GetValue(ssiTemplate, null) ?? string.Empty).ToString()
                        let prevPropertyVal = isNewAccount ? string.Empty : (propertyInfo.GetValue(nonUpdatedAccount, null) ?? string.Empty).ToString()
                        where !propertyVal.Equals(prevPropertyVal)
                        select new hmsUserAuditLog
                        {
                            CreatedAt = DateTime.Now,
                            UserName = userName,
                            Module = "SSITemplate",
                            PreviousStateValue = prevPropertyVal,
                            ModifiedStateValue = propertyVal,
                            Action = isNewAccount ? "Added" : "Edited",
                            Field = propertyInfo.Name,
                            Log = String.Format("Onboarding Name: <i>SSI Template</i><br/>SSI Template Name: <i>{0}</i>", ssiTemplate.TemplateName)
                        }).ToList();


            var auditLogList = new List<hmsUserAuditLog>();

            if (isNewAccount)
            {
                if (!string.IsNullOrWhiteSpace(ssiTemplate.SSITemplateType))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "SSITemplate Type", "Added", "", ssiTemplate.SSITemplateType, userName));

                if (!string.IsNullOrWhiteSpace(broker))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Broker", "Added", "", broker, userName));

                if (!string.IsNullOrWhiteSpace(accountType))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Account Type", "Added", "", accountType, userName));
            }

            auditLogList.AddRange(list);

            return auditLogList;
        }
        public static List<hmsUserAuditLog> GetAuditLogs(onBoardingAccount account, string fundName, string agreement, string broker, string userName)
        {
            var nonUpdatedAccount = FundAccountManager.GetOnBoardingAccount(account.onBoardingAccountId);
            var propertyInfos = typeof(onBoardingAccount).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(s => QualifiedTypeToAudit.Contains(s.PropertyType));
            var isNewAccount = nonUpdatedAccount == null;

            var list = (from propertyInfo in propertyInfos
                        let propertyVal = (propertyInfo.GetValue(account, null) ?? string.Empty).ToString()
                        let prevPropertyVal = isNewAccount ? string.Empty : (propertyInfo.GetValue(nonUpdatedAccount, null) ?? string.Empty).ToString()
                        where !propertyVal.Equals(prevPropertyVal)
                        select new hmsUserAuditLog
                        {
                            CreatedAt = DateTime.Now,
                            UserName = userName,
                            Module = "Account",
                            PreviousStateValue = prevPropertyVal,
                            ModifiedStateValue = propertyVal,
                            Action = isNewAccount ? "Added" : "Edited",
                            Field = propertyInfo.Name,
                            Log = String.Format("Onboarding Name: <i>Account</i><br/>Account Name: <i>{0}</i>", account.AccountName)
                        }).ToList();


            var auditLogList = new List<hmsUserAuditLog>();

            if (isNewAccount)
            {
                if (!string.IsNullOrWhiteSpace(fundName))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Fund",
                        isNewAccount ? "Added" : "Edited", "", fundName, userName));

                if (!string.IsNullOrWhiteSpace(agreement))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Agreement",
                        isNewAccount ? "Added" : "Edited", "", agreement, userName));

                if (!string.IsNullOrWhiteSpace(broker))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Broker",
                        isNewAccount ? "Added" : "Edited", "", broker, userName));

            }

            auditLogList.AddRange(list);

            return auditLogList;
        }

        public static hmsUserAuditLog BuildOnboardingAuditLog(string onboardingType, string onboardingName, string field, string action, string previousStateValue, string modifiedStateValue, string username)
        {
            var auditLog = new hmsUserAuditLog
            {
                CreatedAt = DateTime.Now,
                UserName = username,
                Module = onboardingType,
                PreviousStateValue = previousStateValue,
                ModifiedStateValue = modifiedStateValue,
                Action = action,
                Field = field
            };

            switch (onboardingType)
            {
                case "Account":
                    auditLog.Log = String.Format("Onboarding Name: <i>Account</i><br/>Account Name: <i>{0}</i>", onboardingName);
                    break;
                case "SSITemplate":
                    auditLog.Log = String.Format("Onboarding Name: <i>SSITemplate</i><br/>SSI Template Name: <i>{0}</i>", onboardingName);
                    break;

            }
            //auditLog.Association = ((onboardingType == "Client") ?
            //                       String.Format("Onboarding Name: <i>{0}</i><br/>Client Name: <i>{1}</i>", onboardingType, onboardingName) :
            //                       String.Format("Onboarding Name: <i>{0}</i><br/>Fund Name: <i>{1}</i>", onboardingType, onboardingName));

            return auditLog;
        }

        public static void Log(List<hmsUserAuditLog> auditLogs)
        {
            if (auditLogs.Count == 0)
                return;

            //remove the logs where there are no changes detected.
            var qualifiedChanges = auditLogs.Where(s => !string.Equals(s.PreviousStateValue, s.ModifiedStateValue)).ToList();

            using (var context = new OperationsSecureContext())
            {
                context.hmsUserAuditLogs.AddRange(qualifiedChanges);
                context.SaveChanges();
            }
        }

        public static void LogAudit(hmsUserAuditLog auditLog)
        {
            using (var context = new OperationsSecureContext())
            {
                context.hmsUserAuditLogs.AddOrUpdate(auditLog);
                context.SaveChanges();
            }
        }

        public static List<hmsBulkUploadLog> GetBulkUploadLogs(bool isFundAccountLog, DateTime startDate, DateTime endDate)
        {
            using (var context = new OperationsSecureContext())
            {
                endDate = endDate.AddDays(1);
                return context.hmsBulkUploadLogs.Where(x => x.IsFundAccountLog == isFundAccountLog && x.CreatedAt >= startDate && x.CreatedAt <= endDate).OrderByDescending(s => s.CreatedAt).ToList();
            }
        }

        public static void AddBulkUploadLogs(List<hmsBulkUploadLog> bulkUploadLogs)
        {
            using (var context = new OperationsSecureContext())
            {
                bulkUploadLogs.ForEach(s => s.CreatedAt = DateTime.Now);
                context.hmsBulkUploadLogs.AddRange(bulkUploadLogs);
                context.SaveChanges();
            }

        }

    }
}

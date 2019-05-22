using System;
using System.Collections.Generic;
using System.Linq;
using HedgeMark.Operations.Secure.DataModel;
using System.Data.Entity.Migrations;
using HMOSecureMiddleware.Models;

namespace HMOSecureMiddleware
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

    }
}

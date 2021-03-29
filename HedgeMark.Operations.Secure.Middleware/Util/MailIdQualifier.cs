﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Com.HedgeMark.Commons.Extensions;
using Com.HedgeMark.Commons.Mail;
using HedgeMark.Operations.Secure.DataModel;

namespace HedgeMark.Operations.Secure.Middleware.Util
{
    public class MailIdQualifier
    {
        private static List<string> DevQaMailList
        {
            get
            {
                var allBlockedIds = PreferencesManager.GetLocalQaUsers();
                return string.IsNullOrEmpty(allBlockedIds) ? new List<string>() : allBlockedIds.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower()).ToList();
            }
        }

        private class QualifiedMailIds
        {
            public string To { get; set; }
            public string Cc { get; set; }
            public string Bcc { get; set; }
            public string BlockedIds { get; set; }
            public string AllQualifiedIds { get; set; }
        }

        private const string NewLineCharsRegex = @"\t|\n|\r";

        public static List<string> GetAllEmails(string emailIds)
        {
            var emails = Regex.Split(emailIds, @"\,|;|\|")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => Regex.Replace(Regex.Replace(s, @"\n", ""), NewLineCharsRegex, string.Empty).Trim().ToLower().Replace(" ", string.Empty)).ToList();
            return emails;
        }


        private static QualifiedMailIds FilterMailIds(string allToTypes)
        {
            allToTypes = Regex.Replace(allToTypes, "(\r\n|\r|\n)", ",");

            var allTypes = allToTypes.Split('|').ToList();

            var qualifiedMailIds = new QualifiedMailIds { To = GetFilterMailList(allTypes[0]) };
            if (allTypes.Count > 1)
                qualifiedMailIds.Cc = GetFilterMailList(allTypes[1]);
            if (allTypes.Count > 2)
                qualifiedMailIds.Bcc = GetFilterMailList(allTypes[2]);

            qualifiedMailIds.AllQualifiedIds = GetFilterMailList(allToTypes.Replace("|", ","));

            if (!Utility.IsLowerEnvironment)
                return qualifiedMailIds;

            var allIds = allToTypes.Replace("|", ",").Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLower().Trim()).Distinct().ToList();
            qualifiedMailIds.BlockedIds = string.Join(",", allIds.Where(s => !DevQaMailList.Contains(s)).ToList());
            return qualifiedMailIds;

        }

        private static string GetFilterMailList(string ids)
        {
            var recepientList = GetAllEmails(ids);

            if (Utility.IsLowerEnvironment)
            {
                recepientList = DevQaMailList.Where(x => x.In(recepientList)).ToList();
            }
            ids = string.Join(",", recepientList.Distinct());
            return ids;
        }

        public static string SendMailToQualifiedIds(MailInfo mailInfo)
        {
            var qualifiedIds = FilterMailIds(string.Format("{0}|{1}", mailInfo.ToAddress ?? string.Empty, mailInfo.CcAddress ?? string.Empty));

            if (string.IsNullOrWhiteSpace(qualifiedIds.AllQualifiedIds) || string.IsNullOrWhiteSpace(qualifiedIds.To))
                return string.Format("The following Id's are blocked in current Environment \n {0} ", string.Join(",", qualifiedIds.BlockedIds));

            if (!string.IsNullOrWhiteSpace(qualifiedIds.To))
                mailInfo.ToAddress = qualifiedIds.To;

            if (!string.IsNullOrWhiteSpace(qualifiedIds.Cc))
                mailInfo.CcAddress = qualifiedIds.Cc;

            if (!string.IsNullOrWhiteSpace(qualifiedIds.Bcc))
                mailInfo.BccAddress = qualifiedIds.Bcc;

            //Append environment flag for lower environment
            if (Utility.IsLowerEnvironment)
                mailInfo.Subject = string.Format("{0} | {1}", Utility.Environment.ToUpper(), mailInfo.Subject);

            //Get MailBox Information from Config.
            var configuredMailBox = MailBoxConfigurations.AllMailBoxConfigs.FirstOrDefault(s => s.MailBoxName.ToLower() == mailInfo.FromAddress.GetMailBoxName().ToLower());

            using (SmtpClient client = new SmtpClient(mailInfo.Server))
            {
                if (configuredMailBox != null)
                {
                    client.Credentials = new NetworkCredential(configuredMailBox.UserName, configuredMailBox.Password);
                    client.EnableSsl = configuredMailBox.EnableSsl;
                    client.Port = configuredMailBox.Port;
                }
                MailSender.Send(mailInfo, client);
            }

            return GetMailResponse(qualifiedIds);
        }

        private static string GetMailResponse(QualifiedMailIds qualifiedIds)
        {
            return !string.IsNullOrWhiteSpace(qualifiedIds.BlockedIds)
                ? string.Format("Mail Sent for:{0} ; But The following Id's are blocked in {1} Environment: {2}",
                    string.Join(",", qualifiedIds.AllQualifiedIds), Utility.Environment, string.Join(",", qualifiedIds.BlockedIds))
                : "success";
        }
    }
}

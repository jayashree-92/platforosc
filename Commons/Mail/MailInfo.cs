using System;
using System.Collections.Generic;
using System.IO;

namespace Com.HedgeMark.Commons.Mail
{
    public class MailInfo
    {
        public string Server => ConfigurationManagerWrapper.StringSetting(Config.MailServer);
        public static string DefaultFromAddress => ConfigurationManagerWrapper.StringSetting(Config.FromMailAddress, "no-reply-ops-secure@innocap.com");

        private string fromAddress;
        public string FromAddress
        {
            get => fromAddress ?? DefaultFromAddress;
            set => fromAddress = value;
        }

        private string toAddress;
        public string ToAddress
        {
            get => (toAddress ?? ConfigurationManagerWrapper.StringSetting(Config.ToMailAddress)).Replace(";", ",");
            set => toAddress = value;
        }

        public string BccAddress { get; set; }

        public string CcAddress { get; set; }

        public List<FileInfo> Attachments { get; set; }

        public string Subject { get; set; }

        private string body;

        public string Body
        {
            get
            {
                if (!ConfigurationManagerWrapper.BooleanSetting(Config.ShouldAppendMachineInfoInMails))
                    return body;

                var machineInfo = $"Machine Name: {Environment.MachineName}, System User Name: {Environment.UserName}";
                var lineTerminator = IsHtml ? "<br /><br />" : Environment.NewLine;
                var newBody = body + lineTerminator + machineInfo;
                return newBody;
            }
            set => body = value;
        }

        public bool IsHtml { get; set; }

        public MailInfo(string subject, string body, string toAddress, List<FileInfo> attachments = null, bool isHtml = true, string ccAddress = null, string fromAddress = null, string mailSignature = null)
        {
            SetMailInfo(subject, body, toAddress, attachments, isHtml, ccAddress, mailSignature);
        }

        public MailInfo(string subject, string body, string toAddress, FileInfo attachment = null, bool isHtml = true, string ccAddress = null, string fromAddress = null, string mailSignature = null)
        {
            SetMailInfo(subject, body, toAddress, attachment != null ? new List<FileInfo>() { attachment } : null, isHtml, ccAddress, mailSignature);
        }

        private void SetMailInfo(string subject, string body, string toAddress, List<FileInfo> attachments, bool isHtml, string ccAddress, string mailSignature)
        {
            Subject = subject;
            Body = Uri.UnescapeDataString(string.IsNullOrWhiteSpace(mailSignature) ? body : $"{body}{mailSignature}");
            Attachments = attachments ?? new List<FileInfo>();
            this.toAddress = toAddress;
            if (!string.IsNullOrWhiteSpace(ccAddress)) CcAddress = ccAddress;
            if (!string.IsNullOrWhiteSpace(this.fromAddress)) FromAddress = fromAddress;
            IsHtml = isHtml;
        }
    }
}
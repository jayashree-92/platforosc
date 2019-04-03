using System;
using System.Collections.Generic;
using System.IO;

namespace Com.HedgeMark.Commons.Mail
{
    public class MailInfo
    {
        public string Server
        {
            get { return ConfigurationManagerWrapper.StringSetting(Config.MailServer); }
        }
        public static string FromAddress
        {
            get { return ConfigurationManagerWrapper.StringSetting(Config.FromMailAddress, "no-reply-local@bnymellon.com"); }
        }
        
        public static string HedgeMarkAddress
        {
            get {
                var hedgeMarkAddressFilepath = AppDomain.CurrentDomain.BaseDirectory + @"\\ExternalLibraries\\HedgeMarkAddress.txt";
                return File.ReadAllText(hedgeMarkAddressFilepath);            
            }
        }

        private string toAddress;
        public string ToAddress
        {
            get { return (toAddress ?? ConfigurationManagerWrapper.StringSetting(Config.ToMailAddress)).Replace(";", ","); }
            set { toAddress = value; }
        }

        public string BccAddress { get; set; }

        public string CcAddress { get; set; }

        public FileInfo Attachment { get; set; }

        public List<FileInfo> Attachments { get; set; }

        public string Subject { get; set; }

        private string _body;

        public string Body
        {
            get
            {
                if (!ConfigurationManagerWrapper.BooleanSetting(Config.ShouldAppendMachineInfoInMails))
                    return _body;

                var machineInfo = string.Format("Machine Name: {0}, System User Name: {1}", Environment.MachineName, Environment.UserName);
                var lineTerminator = IsHtml ? "<br /><br />" : Environment.NewLine;
                var newBody = _body + lineTerminator + machineInfo;
                return newBody;
            }
            set
            {
                _body = value;
            }
        }

        public bool IsHtml { get; set; }

        public MailInfo(string subject, string body, string toAddress, List<FileInfo> attachments = null, bool isHtml = true)
        {
            Subject = subject;
            Body = (body ?? "<br/><br/><br/>Regards,<br/>HedgeMark Operations Team ") + HedgeMarkAddress;
            Attachments = attachments;
            this.toAddress = toAddress;
            this.IsHtml = isHtml;
        }

        public MailInfo(string subject, string body, string toAddress, FileInfo attachment = null, bool isHtml = true)
        {
            Subject = subject;
            Body = (body ?? "<br/><br/><br/>Regards,<br/>HedgeMark Operations Team ") + HedgeMarkAddress;
            Attachment = attachment;
            this.toAddress = toAddress;
            this.IsHtml = isHtml;
        }
    }
}
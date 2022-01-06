using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Com.HedgeMark.Commons.Mail
{
    public class MailSender : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MailSender));
        public static string SystemEnvironment = ConfigurationManagerWrapper.StringSetting("Environment");
        private static readonly string MailSenderAuditId = ConfigurationManagerWrapper.StringSetting("HMSystemMailSenderAuditId", "HM-SystemSent@bnymellon.com");

        public static void Send(MailInfo mailInfo, SmtpClient client)
        {
            if (string.IsNullOrWhiteSpace(mailInfo.ToAddress))
                return;

            var toAddress = GetValidatedAddress(mailInfo.ToAddress);

            using (var message = new MailMessage(mailInfo.FromAddress, toAddress, mailInfo.Subject, mailInfo.Body))
            {
                message.IsBodyHtml = mailInfo.IsHtml;

                if (!string.IsNullOrEmpty(mailInfo.CcAddress))
                    message.CC.Add(GetValidatedAddress(mailInfo.CcAddress));

                if (!string.IsNullOrEmpty(mailInfo.BccAddress))
                    message.Bcc.Add(GetValidatedAddress(mailInfo.BccAddress));

                if (SystemEnvironment.Equals("Prod", StringComparison.InvariantCultureIgnoreCase))
                    message.Bcc.Add(GetValidatedAddress(MailSenderAuditId));

                var contentMemoryStream = new List<MemoryStream>();
                try
                {
                    AttachFilesToThisMail(mailInfo, message, out contentMemoryStream);

                    SendMail(message, client);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);

                    if (!SystemEnvironment.Equals("Local", StringComparison.InvariantCultureIgnoreCase))
                        throw;
                }
                finally
                {
                    foreach (var memoryStream in contentMemoryStream)
                        memoryStream.Dispose();
                }
            }
        }

        private static string GetValidatedAddress(string address)
        {
            var toAddress = Regex.Replace(address.Replace(";", ","), @"\s+", string.Empty);
            return toAddress;
        }

        private static void AttachFilesToThisMail(MailInfo mailInfo, MailMessage message, out List<MemoryStream> contentMemoryStream)
        {
            contentMemoryStream = new List<MemoryStream>();

            if (mailInfo.Attachments == null || mailInfo.Attachments.Count == 0)
                return;
          
            foreach (var fileAttachment in mailInfo.Attachments.Where(fileAttachment => fileAttachment.Exists))
            {
                using (var attachmentFileStream = new FileStream(fileAttachment.FullName, FileMode.Open, FileAccess.Read))
                {
                    Logger.DebugFormat("Attaching file {0}", fileAttachment.Name);
                    var contentStream = new MemoryStream();

                    attachmentFileStream.CopyTo(contentStream);
                    contentStream.Position = 0;
                    var attachment = new Attachment(contentStream, fileAttachment.Name, MediaTypeNames.Application.Octet);
                    message.Attachments.Add(attachment);
                    contentMemoryStream.Add(contentStream);
                }
            }
        }

        private static void SendMail(MailMessage message, SmtpClient client)
        {
            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                //Since we are getting the following exception, we are retrying to send mail after 5 seconds  
                //Service not available, closing transmission channel. The server response was: 4.4.1 Connection timed out
                if (ex.Message.Contains("4.4.1"))
                {
                    Thread.Sleep(5000);
                    client.Send(message);
                }
                else
                    throw;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MailSender()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
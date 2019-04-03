using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using log4net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace Com.HedgeMark.Commons.Mail
{
    public class MailSender : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MailSender));
        private const string DebugHeader = "Error Information :";

        public static void Send(MailInfo mailInfo)
        {
            if (string.IsNullOrWhiteSpace(mailInfo.ToAddress))
                return;

            var toAddress = GetValidatedAddress(mailInfo.ToAddress);

            using (var message = new MailMessage(MailInfo.FromAddress, toAddress, mailInfo.Subject, mailInfo.Body))
            {
                message.IsBodyHtml = mailInfo.IsHtml;

                if (!string.IsNullOrEmpty(mailInfo.CcAddress))
                    message.CC.Add(GetValidatedAddress(mailInfo.CcAddress));

                if (!string.IsNullOrEmpty(mailInfo.BccAddress))
                    message.Bcc.Add(GetValidatedAddress(mailInfo.BccAddress));

                var contentMemoryStream = new List<MemoryStream>();
                try
                {
                    Logger.DebugFormat("Mail Details: {0} From: {1} {0} To: {2} {0} BCC: {3} {0} Subject: {4} {0}", Environment.NewLine, MailInfo.FromAddress, mailInfo.ToAddress, mailInfo.BccAddress, mailInfo.Subject);

                    AttachFilesToThisMail(mailInfo, message, out contentMemoryStream);
                    using (var client = new SmtpClient(mailInfo.Server))
                    {
                        SendMail(message, client);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
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

            if (mailInfo.Attachments == null && mailInfo.Attachment == null)
                return;

            if (mailInfo.Attachments == null)
                mailInfo.Attachments = new List<FileInfo>();

            if (mailInfo.Attachment != null)
                mailInfo.Attachments.Add(mailInfo.Attachment);

            if (mailInfo.Attachments.Count == 0)
                return;

            foreach (var fileAttachment in mailInfo.Attachments)
            {
                if (!fileAttachment.Exists)
                    continue;

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

        public void SendErrorNotification(MailInfo mailInfo, Exception exception)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(mailInfo.Body);
            stringBuilder.AppendLine().AppendLine("Error Message: ").Append(exception.Message);

            stringBuilder.AppendLine().AppendLine().AppendLine(DebugHeader);
            stringBuilder.AppendLine().AppendLine(exception.InnerException == null ? exception.StackTrace : exception.InnerException.StackTrace);
            mailInfo.Body = stringBuilder.ToString();
            Send(mailInfo);
        }

        public void Dispose()
        {
            // Use SupressFinalize in case a subclass 
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }
    }
}
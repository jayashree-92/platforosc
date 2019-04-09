using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware.Models;

namespace HMOSecureMiddleware
{
    public class NotificationManager
    {
        public static void NotifyOpsUser(WireTicket wireTicket)
        {
            //Notification is generated 
            //  to initiator when the approver approves the wire
            //  to initiator and approver when Swift status is complete
            //  to initiator and approver when Swift status is failed


            var isSwiftStatusNotInitiated = wireTicket.HMWire.SwiftStatusId == (int)WireDataManager.SwiftStatus.NotInitiated;
            var wireStatus = isSwiftStatusNotInitiated ? ((WireDataManager.WireStatus)wireTicket.HMWire.WireStatusId).ToString() : ((WireDataManager.SwiftStatus)wireTicket.HMWire.SwiftStatusId).ToString();

            var message = new StringBuilder();

            message.AppendFormat("{0}{1} Wire of {2} for ", wireTicket.HMWire.IsBookTransfer ? "Book transfer -" : string.Empty, wireTicket.HMWire.hmsWireMessageType.MessageType, wireTicket.HMWire.Amount.ToCurrency());

            message.Append(wireTicket.HMWire.IsBookTransfer
                ? wireTicket.FundName
                : wireTicket.Agreement.AgreementShortName);

            message.AppendFormat(" is {0}", wireStatus);

            //need to derive recipients 
            var qualifiedReceipients = isSwiftStatusNotInitiated
                ? wireTicket.HMWire.hmsWireWorkflowLogs.Where(s => s.WireStatusId == 2).Select(s => s.CreatedBy).ToList() 
                : wireTicket.HMWire.hmsWireWorkflowLogs.Where(s => s.WireStatusId == 2 || s.WireStatusId == 3).Select(s => s.CreatedBy).ToList();

            using (var context = new OperationsSecureContext())
            {
                var notificationList = new List<hmsNotificationStaging>();
                qualifiedReceipients.ForEach(toUserId=>
                {
                    notificationList.Add(new hmsNotificationStaging()
                    {
                        FromUserId = -1,
                        ToUserId = toUserId,
                        Message = message.ToString(),
                        Title = wireStatus,
                        CreatedAt = DateTime.Now
                    });
                });
                
                context.hmsNotificationStagings.AddRange(notificationList);
                context.SaveChanges();
            }
        }
    }
}

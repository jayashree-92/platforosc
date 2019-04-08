using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeMark.Operations.Secure.DataModel;

namespace HMOSecureMiddleware
{
    public class NotificationManager
    {
        public static void NotifyOpsUser(long wireId, WireDataManager.WireTicketStatus wireStatus, WireDataManager.SwiftStatus swiftStatus)
        {
            //Notification is generated 
            //  to initiator when the approver approves the wire
            //  to initiator and approver when Swift status is complete
            //  to initiator and approver when Swift status is failed

            var message = string.Empty;

            var wireTicket = WireDataManager.GetWireData(wireId);

            message = "";

            using (var context = new OperationsSecureContext())
            {
                var notification = new hmsNotificationStaging()
                {
                    FromUserId = -1,
                    ToUserId = wireTicket.HMWire.LastUpdatedBy,
                    Message = message,
                    Title = swiftStatus != WireDataManager.SwiftStatus.NotInitiated ? swiftStatus.ToString() : wireStatus.ToString(),
                    CreatedAt = DateTime.Now
                };

                context.hmsNotificationStagings.Add(notification);
                context.SaveChanges();
            }
        }



    }
}

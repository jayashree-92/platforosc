using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.SwiftMessageHandler.Model.MT;
using HMOSecureMiddleware.Models;
using HMOSecureMiddleware.Queues;
using HMOSecureMiddleware.SwiftMessageManager;
using log4net;

namespace HMOSecureMiddleware
{
    public class WireTransactionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WireTransactionManager));

        //We need to make sure only one transaction is performed at a given time - to avoid same wire is being approved by two different scenario
        public static object WireTransactionLock = new object();


        /// <summary>
        /// Only the stauts with "Approved" State is entertained
        /// </summary>
        /// <param name="wire"></param>
        /// <param name="comment"></param>
        /// <param name="userId"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ApproveAndInititateWireTransfer(WireTicket wire, string comment, int userId)
        {
            lock (WireTransactionLock)
            {
                //Check if the given Wire Id is still in approved state
                var currentStatus = WireDataManager.GetWireStatus(wire.WireId);

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Approved)
                    throw new InvalidOperationException("The selected wire ticket is already approved.");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Drafted)
                    throw new InvalidOperationException("Cannot process un-initiated Wire ticket");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Cancelled)
                    throw new InvalidOperationException("Cannot process a Cancelled Wire ticket");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Failed || currentStatus.SwiftStatus == WireDataManager.SwiftStatus.Failed)
                    throw new InvalidOperationException("Wire already Failed, please re-initiate the Wire ticket");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Initiated)
                {
                    WireDataManager.SetWireStatusAndWorkFlow(wire.HMWire, WireDataManager.WireStatus.Approved, WireDataManager.SwiftStatus.NotInitiated, comment, userId);
                    ProcessApprovedWire(wire);
                }
            }
        }


        /// <summary>
        /// Only the status with "Cancelled" State is entertained
        /// </summary>
        /// <param name="wire"></param>
        /// <param name="comment"></param>
        /// <param name="userId"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void CancelWireTransfer(WireTicket wire, string comment, int userId)
        {
            lock (WireTransactionLock)
            {
                //Check if the given Wire Id is still in approved state
                var currentStatus = WireDataManager.GetWireStatus(wire.WireId);

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Cancelled)
                    throw new InvalidOperationException("The selected wire ticket is already cancelled.");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Failed)
                    throw new InvalidOperationException("Wire already Failed, please re-initiate the Wire ticket");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Drafted || currentStatus.WireStatus == WireDataManager.WireStatus.Initiated)
                    throw new InvalidOperationException("Cannot generate cancellation of an un-approved Wire ticket");

                if (currentStatus.SwiftStatus == WireDataManager.SwiftStatus.Completed || currentStatus.SwiftStatus == WireDataManager.SwiftStatus.Failed)
                    throw new InvalidOperationException(string.Format("The Wire transaction already {0}. Cannot process cancellation.", currentStatus.SwiftStatus.ToString()));

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Approved)
                {
                    WireDataManager.SetWireStatusAndWorkFlow(wire.HMWire, WireDataManager.WireStatus.Cancelled, WireDataManager.SwiftStatus.NotInitiated, comment, userId);
                    ProcesCancellationOfWire(wire);
                }
            }
        }


        /// <summary>
        /// This function creates an Out-Bound message
        /// </summary>
        /// <param name="wire"></param>
        private static void ProcessApprovedWire(WireTicket wire)
        {
            try
            {
                //Create Swift Message and send to EMX team
                CreateAndSendMessageToMQ(wire, wire.HMWire.hmsWireMessageType);

                //Update the given wire Id to "processing" in workflow and wire table
                WireDataManager.SetWireStatusAndWorkFlow(wire.HMWire, WireDataManager.WireStatus.Approved, WireDataManager.SwiftStatus.Processing, string.Empty, -1);
            }
            catch (Exception ex)
            {
                var failureMsg = string.Format("Wire Transaction failed with System exception: {0}", ex.Message);
                Logger.Error(failureMsg, ex);
                WireDataManager.SetWireStatusAndWorkFlow(wire.HMWire, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.Failed, failureMsg, -1);
                throw;
            }
        }

        private static void ProcesCancellationOfWire(WireTicket wire)
        {
            try
            {
                var originalMessageType = wire.HMWire.hmsWireMessageType.MessageType.Replace("MT", string.Empty);

                //For cancellation of a processing wire, we will send a different message Type
                var cancMsgTypeStr = string.Format("MT{0}", originalMessageType.StartsWith("1") ? MTDirectory.MT_192 : MTDirectory.MT_292);

                hmsWireMessageType cancellationMessageType;
                using (var context = new OperationsSecureContext())
                {
                    cancellationMessageType = context.hmsWireMessageTypes.First(s => s.MessageType == cancMsgTypeStr);
                }

                //Create Swift Message and send to EMX team
                CreateAndSendMessageToMQ(wire, cancellationMessageType);

                //Update the given wire Id to "processing" in workflow and wire table
                WireDataManager.SetWireStatusAndWorkFlow(wire.WireId, WireDataManager.WireStatus.Cancelled, WireDataManager.SwiftStatus.Processing, "Wire Cancellation request sent and Processing", -1);
            }
            catch (Exception ex)
            {
                var failureMsg = string.Format("Wire Cancellation failed with System exception: {0}", ex.Message);
                Logger.Error(failureMsg, ex);
                WireDataManager.SetWireStatusAndWorkFlow(wire.WireId, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.Failed, failureMsg, -1);
                throw;
            }
        }

        private static void CreateAndSendMessageToMQ(WireTicket wire, hmsWireMessageType messageType)
        {
            //Create Swift message
            var swiftMessage = OutboundSwiftMsgCreater.CreateMessage(wire, messageType.MessageType);

            //Validate Error Message 
            //SwiftMessageValidator.Validate(swiftMessage);

            //Put an entry to Wire Log table with the parameters used to create Swift Message
            LogOutBoundWireTransaction(wire, messageType, swiftMessage);

            //Send the message to MQ
            if (!Utility.IsLocal())
                QueueSystemManager.SendMessage(swiftMessage);
        }

        public static void LogFrontEndAcknowledgment(long wireId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireLog = context.hmsWireLogs.FirstOrDefault(s => s.hmsWireId == wireId);

                if (wireLog == null)
                    return;

                wireLog.IsFrontEndAcknowleged = true;

                context.hmsWireLogs.AddOrUpdate(h => h.hmsWireId, wireLog);
                context.SaveChanges();
            }
        }

        public static void ProcessInboundMessage(string swiftMessage)
        {
            LogInBoundWireMessage(swiftMessage);

            var confirmationData = InboundSwiftMsgParser.ParseMessage(swiftMessage);

            if (confirmationData.IsFeAck)
                LogFrontEndAcknowledgment(confirmationData.WireId);

            else if (confirmationData.IsAckOrNack && confirmationData.IsAcknowledged)
                WireDataManager.SetWireStatusAndWorkFlow(confirmationData.WireId, WireDataManager.SwiftStatus.Acknowledged, string.Empty);

            else if (confirmationData.IsAckOrNack && confirmationData.IsNegativeAcknowledged)
                WireDataManager.SetWireStatusAndWorkFlow(confirmationData.WireId, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.NegativeAcknowledged, confirmationData.ExceptionMessage, -1);

            //Update the given wire Id to "Completed" in workflow and wire table
            else if (confirmationData.IsConfirmed)
                WireDataManager.SetWireStatusAndWorkFlow(confirmationData.WireId, WireDataManager.SwiftStatus.Completed, confirmationData.ConfirmationMessage);

            else if (!string.IsNullOrWhiteSpace(confirmationData.ExceptionMessage))
                WireDataManager.SetWireStatusAndWorkFlow(confirmationData.WireId, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.Failed, string.Format("Wire Transaction Failed with error: {0}", confirmationData.ExceptionMessage), -1);

            //Put an entry to Wire Log table with the parameters used to create Swift Message
            if (!confirmationData.IsFeAck)
                LogInBoundWireTransaction(confirmationData, swiftMessage);
        }

        private static void LogOutBoundWireTransaction(WireTicket wireTicket, hmsWireMessageType messageType, string swiftMessage)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireLog = context.hmsWireLogs.FirstOrDefault(s => s.hmsWireId == wireTicket.WireId && s.WireStatusId == wireTicket.HMWire.WireStatusId) ?? new hmsWireLog()
                {
                    hmsWireId = wireTicket.WireId,
                    WireStatusId = wireTicket.HMWire.WireStatusId,
                    WireMessageTypeId = messageType.hmsWireMessageTypeId,
                    RecCreatedAt = DateTime.Now,
                    IsFrontEndAcknowleged = false
                };

                wireLog.OutBoundSwiftMessage = swiftMessage;

                context.hmsWireLogs.AddOrUpdate(h => new { h.hmsWireId, h.WireMessageTypeId }, wireLog);
                context.SaveChanges();
            }

            NotificationManager.NotifyOpsUser(wireTicket);
        }

        private static void LogInBoundWireTransaction(WireInBoundMessage inBoundMsg, string swiftMessage)
        {
            var wireTicket = WireDataManager.GetWireData(inBoundMsg.WireId);

            using (var context = new OperationsSecureContext())
            {

                //We need last or default - as same wireId can be approved and cancelled
                var wireLog = context.hmsWireLogs.Where(s => s.hmsWireId == wireTicket.WireId && s.WireStatusId == wireTicket.HMWire.WireStatusId).OrderBy(s => s.RecCreatedAt).ToList().LastOrDefault() ?? new hmsWireLog()
                {
                    hmsWireId = inBoundMsg.WireId,
                    WireStatusId = wireTicket.HMWire.WireStatusId,
                    WireMessageTypeId = wireTicket.HMWire.WireMessageTypeId,
                    RecCreatedAt = DateTime.Now,
                    IsFrontEndAcknowleged = false
                };

                if (inBoundMsg.IsAckOrNack)
                    wireLog.ServiceSwiftMessage = swiftMessage;
                else
                    wireLog.InBoundSwiftMessage = swiftMessage;

                if (!string.IsNullOrWhiteSpace(inBoundMsg.ExceptionMessage))
                    wireLog.ExceptionDetails = inBoundMsg.ExceptionMessage;

                if (!string.IsNullOrWhiteSpace(inBoundMsg.ConfirmationMessage))
                    wireLog.ConfirmationMessageDetails = inBoundMsg.ConfirmationMessage;

                context.hmsWireLogs.AddOrUpdate(wireLog);
                context.SaveChanges();
            }

            if (wireTicket.HMWire.SwiftStatusId != (int)WireDataManager.SwiftStatus.Acknowledged)
                NotificationManager.NotifyOpsUser(wireTicket);

        }

        private static void LogInBoundWireMessage(string inBoundMQMessage)
        {
            using (var context = new OperationsSecureContext())
            {
                var inBoundMessageMQLog = new hmsInBoundMQLog
                {
                    InBoundMessage = inBoundMQMessage,
                    CreatedAt = DateTime.Now
                };
                context.hmsInBoundMQLogs.Add(inBoundMessageMQLog);
                context.SaveChanges();
            }
        }
    }
}

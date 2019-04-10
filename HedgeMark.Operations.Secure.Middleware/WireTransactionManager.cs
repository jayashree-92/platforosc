using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using HedgeMark.Operations.Secure.DataModel;
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
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void InititateWireTransfer(WireTicket wire)
        {
            lock (WireTransactionLock)
            {
                //Check if the given Wire Id is still in approved state
                var currentStatus = WireDataManager.GetWireStatus(wire.WireId);

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Drafted || currentStatus.WireStatus == WireDataManager.WireStatus.Initiated)
                    throw new InvalidOperationException("Cannot process an un-approved Wire ticket");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Cancelled)
                    throw new InvalidOperationException("Cannot process a Cancelled Wire ticket");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Failed || currentStatus.SwiftStatus == WireDataManager.SwiftStatus.Failed)
                    throw new InvalidOperationException("Wire already Failed, please re-initiate the Wire ticket");

                //if (currentStatus.SwiftStatus != WireDataManager.SwiftStatus.NotInitiated)
                //    throw new InvalidOperationException(string.Format("The Wire transaction is already {0}. Please initate a new Wire", currentStatus.SwiftStatus.ToString()));

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Approved)
                    ProcessApprovedWire(wire);
            }
        }


        /// <summary>
        /// Only the status with "Cancelled" State is entertained
        /// </summary>
        /// <param name="wire"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void CancelWireTransfer(WireTicket wire)
        {
            lock (WireTransactionLock)
            {
                //Check if the given Wire Id is still in approved state
                var currentStatus = WireDataManager.GetWireStatus(wire.WireId);

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Drafted || currentStatus.WireStatus == WireDataManager.WireStatus.Initiated)
                    throw new InvalidOperationException("Cannot process an un-approved Wire ticket");

                if (currentStatus.SwiftStatus != WireDataManager.SwiftStatus.NotInitiated)
                    throw new InvalidOperationException(string.Format("The Wire transaction already {0}. Please initate a new Wire", currentStatus.SwiftStatus.ToString()));

                //if (currentStatus.SwiftStatus == WireDataManager.SwiftStatus.Completed)
                //    throw new InvalidOperationException("Wire is already Cancelled and in processing state. Please refresh and check again");

                //if (currentStatus.WorkflowStatus == WireDataManager.WorkflowStatus.Failed || currentStatus.SwiftStatus == WireDataManager.SwiftStatus.Failed)
                //    throw new InvalidOperationException("Wire already Failed, please re-initiate the Wire ticket");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Cancelled)
                    ProcesCancellationOfWire(wire);
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
                var messageType = wire.HMWire.hmsWireMessageType.MessageType;

                //Create Swift Message and send to EMX team
                CreateAndSendMessageToMQ(wire, messageType);

                //Update the given wire Id to "processing" in workflow and wire table
                WireDataManager.SetWireStatus(wire.WireId, (WireDataManager.WireStatus)wire.HMWire.WireStatusId, WireDataManager.SwiftStatus.Processing, string.Empty);
            }
            catch (Exception ex)
            {
                var failureMsg = string.Format("Wire Transaction failed with System exception: {0}", ex.Message);
                Logger.Error(failureMsg, ex);
                WireDataManager.SetWireStatus(wire.WireId, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.Failed, failureMsg);
                throw;
            }
        }

        private static void ProcesCancellationOfWire(WireTicket wire)
        {
            try
            {
                var messageType = wire.HMWire.hmsWireMessageType.MessageType.Replace("MT", string.Empty);

                //For cancellation of a processing wire, we will send a different message Type
                messageType = messageType.StartsWith("1") ? "MT192" : "MT292";

                //Create Swift Message and send to EMX team
                CreateAndSendMessageToMQ(wire, messageType);

                //Update the given wire Id to "processing" in workflow and wire table
                WireDataManager.SetWireStatus(wire.WireId, (WireDataManager.WireStatus)wire.HMWire.WireStatusId, WireDataManager.SwiftStatus.Processing, "Wire Cancellation request sent and Processing");
            }
            catch (Exception ex)
            {
                var failureMsg = string.Format("Wire Cancellation failed with System exception: {0}", ex.Message);
                Logger.Error(failureMsg, ex);
                WireDataManager.SetWireStatus(wire.WireId, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.Failed, failureMsg);
                throw;
            }
        }

        private static void CreateAndSendMessageToMQ(WireTicket wire, string messageType)
        {
            //Create Swift message
            var swiftMessage = SwiftMessageCreater.CreateMessage(messageType, wire);

            //Validate Error Message 
            //SwiftMessageValidator.Validate(swiftMessage);

            //Put an entry to Wire Log table with the parameters used to create Swift Message
            LogOutBoundWireTransaction(wire, swiftMessage);

            //Send the message to MQ
            if (!Utility.IsLocal())
                QueueSystemManager.SendMessage(swiftMessage, wire.WireId);
        }

        public static void LogFrontEndAcknowledgment(string swiftMessage, long wireId)
        {
            LogInBoundWireMessage(swiftMessage);

            if (wireId == -1)
            {
                var confirmationData = SwiftMessageParser.ParseMessage(swiftMessage.Replace("FEACK", string.Empty));
                wireId = confirmationData.WireId;
            }

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

            //var confirmationData = GetWireInBoundMessage(swiftMessage);
            var confirmationData = SwiftMessageParser.ParseMessage(swiftMessage);

            if (confirmationData.IsAckOrNack && confirmationData.IsAcknowledged)
                WireDataManager.SetWireStatus(confirmationData.WireId, WireDataManager.SwiftStatus.Acknowledged, string.Empty);

            else if (confirmationData.IsAckOrNack && confirmationData.IsNegativeAcknowledged)
                WireDataManager.SetWireStatus(confirmationData.WireId, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.NegativeAcknowledged, confirmationData.ExceptionMessage);

            //Update the given wire Id to "Completed" in workflow and wire table
            else if (confirmationData.IsConfirmed)
                WireDataManager.SetWireStatus(confirmationData.WireId, WireDataManager.SwiftStatus.Completed, confirmationData.ConfirmationMessage);

            else if (!string.IsNullOrWhiteSpace(confirmationData.ExceptionMessage))
                WireDataManager.SetWireStatus(confirmationData.WireId, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.Failed, string.Format("Wire Transaction Failed with error: {0}", confirmationData.ExceptionMessage));

            //Put an entry to Wire Log table with the parameters used to create Swift Message
            LogInBoundWireTransaction(confirmationData, swiftMessage);
        }

        private static void LogOutBoundWireTransaction(WireTicket wireTicket, string swiftMessage)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireLog = context.hmsWireLogs.FirstOrDefault(s => s.hmsWireId == wireTicket.WireId && s.WireStatusId == wireTicket.HMWire.WireStatusId) ?? new hmsWireLog()
                {
                    hmsWireId = wireTicket.WireId,
                    WireStatusId = wireTicket.HMWire.WireStatusId,
                    WireMessageTypeId = wireTicket.HMWire.WireMessageTypeId,
                    RecCreatedAt = DateTime.Now
                };

                wireLog.OutBoundSwiftMessage = swiftMessage;

                context.hmsWireLogs.AddOrUpdate(h => h.hmsWireId, wireLog);
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
                    RecCreatedAt = DateTime.Now
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

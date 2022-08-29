using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT;
using HedgeMark.SwiftMessageHandler.Model.MT.MT1XX;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Middleware.Queues;
using HM.Operations.Secure.Middleware.SwiftMessageManager;
using log4net;

namespace HM.Operations.Secure.Middleware
{
    public enum WireReferenceTag { NA, COV, NOT, CANC }

    public class WireTransactionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WireTransactionManager));

        //We need to make sure only one transaction is performed at a given time - to avoid same wire is being approved by two different scenario
        public static object WireTransactionLock = new object();



        /// <summary>
        /// Only the status with "Approved" State is entertained
        /// </summary>
        /// <param name="wire"></param>
        /// <param name="comment"></param>
        /// <param name="userId"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ApproveAndInitiateWireTransfer(WireTicket wire, string comment, int userId)
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
                    throw new InvalidOperationException(
                        $"The Wire transaction already {currentStatus.SwiftStatus.ToString()}. Cannot process cancellation.");

                if (currentStatus.WireStatus == WireDataManager.WireStatus.Approved)
                {
                    WireDataManager.SetWireStatusAndWorkFlow(wire.HMWire, WireDataManager.WireStatus.Cancelled, WireDataManager.SwiftStatus.NotInitiated, comment, userId);
                    ProcessCancellationOfWire(wire);
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
                //Update the given wire Id to "processing" in workflow and wire table
                var workflow = WireDataManager.SetWireStatusAndWorkFlow(wire.HMWire, WireDataManager.WireStatus.Approved, WireDataManager.SwiftStatus.Processing, string.Empty, -1);

                //Create Swift Message and send to EMX team
                CreateAndSendMessageToMQ(wire, wire.HMWire.hmsWireMessageType, null, workflow.hmsWireWorkflowLogId);

            }
            catch (Exception ex)
            {
                var failureMsg = $"Wire Transaction failed with System exception: {ex.Message}";
                Logger.Error(failureMsg, ex);
                WireDataManager.SetWireStatusAndWorkFlow(wire.HMWire, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.Failed, failureMsg, -1);
                throw;
            }
        }

        private static void ProcessCancellationOfWire(WireTicket wire)
        {
            try
            {
                var originalMessageType = wire.HMWire.hmsWireMessageType.MessageType.Replace("MT", string.Empty);

                //For cancellation of a processing wire, we will send a different message Type
                var cancelMsgTypeStr = $"MT{(originalMessageType.StartsWith("1") ? MTDirectory.MT_192 : MTDirectory.MT_292)}";

                hmsWireMessageType cancellationMessageType;
                using (var context = new OperationsSecureContext())
                {
                    cancellationMessageType = context.hmsWireMessageTypes.First(s => s.MessageType == cancelMsgTypeStr);
                }

                //Update the given wire Id to "processing" in workflow and wire table
                var workflow = WireDataManager.SetWireStatusAndWorkFlow(wire.WireId, WireDataManager.WireStatus.Cancelled, WireDataManager.SwiftStatus.Processing, "Wire cancellation request sent and processing", -1);

                //Create Swift Message and send to EMX team
                CreateAndSendMessageToMQ(wire, cancellationMessageType, wire.HMWire.hmsWireMessageType, workflow.hmsWireWorkflowLogId);
            }
            catch (Exception ex)
            {
                var failureMsg = $"Wire Cancellation failed with System exception: {ex.Message}";
                Logger.Error(failureMsg, ex);
                WireDataManager.SetWireStatusAndWorkFlow(wire.WireId, WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.Failed, failureMsg, -1);
                throw;
            }
        }

        public const string MT103 = "MT103";
        public const string MT202COV = "MT202 COV";
        public const string MT210 = "MT210";

        private static void CreateAndSendMessageToMQ(WireTicket wire, hmsWireMessageType messageType, hmsWireMessageType originalMessageType, long workflowLogId)
        {
            AbstractMT swiftMessage103 = null;
            var is202Cov = messageType.MessageType.Equals(MT202COV) || messageType.MessageType.Equals("MT202COV");

            if (is202Cov)
            {
                var mt103MessageType = GetMessageType(MT103);
                //We need to create an MT 103 and use TransactionRef of 103 as Related Ref of MT 202 COV
                swiftMessage103 = OutboundSwiftMsgCreator.CreateMessage(wire, MT103, string.Empty, WireReferenceTag.COV);
                SendAndLogWireTransaction(wire, mt103MessageType, workflowLogId, swiftMessage103);
            }

            //Create Swift message
            var swiftMessage = OutboundSwiftMsgCreator.CreateMessage(wire, messageType.MessageType, originalMessageType == null ? string.Empty : originalMessageType.MessageType);

            if (is202Cov)
            {
                swiftMessage.updateFieldValue(FieldDirectory.FIELD_121, swiftMessage103.Block3.GetFieldValue(FieldDirectory.FIELD_121));
                swiftMessage.updateFieldValue(FieldDirectory.FIELD_21, swiftMessage103.Block4.GetFieldValue(FieldDirectory.FIELD_20));
            }

            SendAndLogWireTransaction(wire, messageType, workflowLogId, swiftMessage);

            if (wire.IsFundTransfer && wire.ReceivingAccount.SwiftGroup.AcceptedMessages.Contains(MT210))
            {
                var familyName = OnBoardingDataManager.GetCounterpartyFamilyName(wire.SendingAccount.SwiftGroup.BrokerLegalEntityId ?? 0);
                if (familyName.Equals("BNY", StringComparison.InvariantCultureIgnoreCase))
                    return;

                var mt210MessageType = GetMessageType(MT210);
                var mt210Message = wire.DeepCopy();
                var stagedSendingAccount = mt210Message.SendingAccount.DeepCopy();
                var stagedReceivingAccount = mt210Message.SendingAccount.DeepCopy();
                mt210Message.ReceivingAccount = stagedSendingAccount;
                mt210Message.SendingAccount = stagedReceivingAccount;

                var swiftMessage210 = OutboundSwiftMsgCreator.CreateMessage(mt210Message, MT210, string.Empty, WireReferenceTag.NOT);
                swiftMessage210.updateFieldValue(FieldDirectory.FIELD_21, swiftMessage.Block4.GetFieldValue(FieldDirectory.FIELD_20));
                SendAndLogWireTransaction(mt210Message, mt210MessageType, workflowLogId, swiftMessage210);
            }
        }

        private static hmsWireMessageType GetMessageType(string MTMessageType)
        {
            using var context = new OperationsSecureContext();
            return context.hmsWireMessageTypes.First(s => s.MessageType == MTMessageType);
        }

        private static void SendAndLogWireTransaction(WireTicket wire, hmsWireMessageType messageType, long workflowLogId, AbstractMT swiftMessage)
        {
            //Validate Error Message 
            //SwiftMessageValidator.Validate(swiftMessage);

            //Send the message to MQ
            QueueSystemManager.SendMessage(swiftMessage.GetMessage());

            //Put an entry to Wire Log table with the parameters used to create Swift Message
            LogOutBoundWireTransaction(wire, messageType, swiftMessage.GetMessage(), workflowLogId);
        }

        public static void ProcessInboundMessage(string swiftMessage)
        {
            var confirmationData = InboundSwiftMsgParser.ParseMessage(swiftMessage);

            //As of now we are not logging FEACK in wire logs, but are logged in MQLogs table
            if (confirmationData.IsFeAck)
                return;

            //When  reference tag has "COV", it means its a MT103 generated on behalf of MT202COV. We should skip tracking MT103 and track only original MT202COV
            if (confirmationData.ReferenceTag == WireReferenceTag.COV.ToString() || confirmationData.ReferenceTag == WireReferenceTag.NOT.ToString())
                return;

            //Ignore messages from the ignore list Eg. MT 094 - Broadcast messages
            if (InboundSwiftMsgParser.MTMessageTypesToIgnore.Any(s => s.Equals(confirmationData.SwiftMessage.GetMTType())))
                return;

            var isCancellationMessage = confirmationData.MessageType.EndsWith("192") || confirmationData.MessageType.EndsWith("292");
            var wireTicket = WireDataManager.GetWire(confirmationData.WireId);
            if (wireTicket == null)
                throw new UnhandledWireMessageException($"Unknown Transaction - unable to wire with id {confirmationData.WireId}");

            hmsWireWorkflowLog workflowLog = null;
            if (confirmationData.IsAckOrNack && confirmationData.IsAcknowledged)
                workflowLog = WireDataManager.SetWireStatusAndWorkFlow(wireTicket, isCancellationMessage ? WireDataManager.WireStatus.Cancelled : WireDataManager.WireStatus.Approved, WireDataManager.SwiftStatus.Acknowledged, string.Empty, -1);

            else if (confirmationData.IsAckOrNack && confirmationData.IsNegativeAcknowledged)
                workflowLog = WireDataManager.SetWireStatusAndWorkFlow(wireTicket, isCancellationMessage ? WireDataManager.WireStatus.Cancelled : WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.NegativeAcknowledged, confirmationData.ExceptionMessage, -1);

            //Update the given wire Id to "Completed" in workflow and wire table
            else if (confirmationData.IsConfirmed)
                workflowLog = WireDataManager.SetWireStatusAndWorkFlow(wireTicket, isCancellationMessage ? WireDataManager.WireStatus.Cancelled : WireDataManager.WireStatus.Approved, WireDataManager.SwiftStatus.Completed, confirmationData.ConfirmationMessage, -1);

            else if (!string.IsNullOrWhiteSpace(confirmationData.ExceptionMessage))
                workflowLog = WireDataManager.SetWireStatusAndWorkFlow(wireTicket, isCancellationMessage ? WireDataManager.WireStatus.Cancelled : WireDataManager.WireStatus.Failed, WireDataManager.SwiftStatus.Failed, $"Wire Transaction Failed with error: {confirmationData.ExceptionMessage}", -1);

            //Put an entry to Wire Log table with the parameters used to create Swift Message
            LogInBoundWireTransaction(confirmationData, workflowLog.hmsWireWorkflowLogId);
        }

        private static void LogOutBoundWireTransaction(WireTicket wireTicket, hmsWireMessageType messageType, string swiftMessage, long workflowId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireLog = new hmsWireLog()
                {
                    hmsWireId = wireTicket.WireId,
                    hmsWireWorkflowLogId = workflowId,
                    WireMessageTypeId = messageType.hmsWireMessageTypeId,
                    SwiftMessage = swiftMessage,
                    hmsWireLogTypeId = 1,
                    RecCreatedAt = DateTime.Now,
                };

                context.hmsWireLogs.Add(wireLog);
                context.SaveChanges();
            }

            NotificationManager.NotifyOpsUser(wireTicket);
        }

        private static void LogInBoundWireTransaction(WireInBoundMessage inBoundMsg, long workflowLogId)
        {
            using (var context = new OperationsSecureContext())
            {
                //Get wire Details
                var hmWire = context.hmsWires.First(s => s.hmsWireId == inBoundMsg.WireId);

                var messageType = inBoundMsg.SwiftMessage.GetFullMessageType();
                var wireTypeId = context.hmsWireMessageTypes.First(s => s.MessageType == messageType).hmsWireMessageTypeId;

                //We need last or default - as same wireId can be approved and cancelled
                var wireLog = new hmsWireLog()
                {
                    hmsWireId = inBoundMsg.WireId,
                    hmsWireWorkflowLogId = workflowLogId,
                    WireMessageTypeId = wireTypeId,
                    SwiftMessage = inBoundMsg.OriginalFinMessage,
                    RecCreatedAt = DateTime.Now,
                };

                if (inBoundMsg.IsAcknowledged)
                    wireLog.hmsWireLogTypeId = 2;
                else if (inBoundMsg.IsNegativeAcknowledged)
                    wireLog.hmsWireLogTypeId = 3;
                else if (inBoundMsg.IsConfirmed)
                    wireLog.hmsWireLogTypeId = 4;

                if (!string.IsNullOrWhiteSpace(inBoundMsg.ExceptionMessage))
                    wireLog.AdditionalDetails = inBoundMsg.ExceptionMessage;

                else if (!string.IsNullOrWhiteSpace(inBoundMsg.ConfirmationMessage))
                    wireLog.AdditionalDetails = inBoundMsg.ConfirmationMessage;

                context.hmsWireLogs.Add(wireLog);
                context.SaveChanges();

                if (hmWire.SwiftStatusId != (int)WireDataManager.SwiftStatus.Acknowledged)
                    NotificationManager.NotifyOpsUser(hmWire.hmsWireId);
            }
        }
    }
}

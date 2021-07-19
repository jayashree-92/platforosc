using System.Collections.Generic;
using System.Linq;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT;
using HM.Operations.Secure.Middleware.Models;

namespace HM.Operations.Secure.Middleware.SwiftMessageManager
{
    public class InboundSwiftMsgParser
    {
        private static readonly List<string> HandledMTMessageConfirmationList = new List<string>() { MTDirectory.MT_196, MTDirectory.MT_296, MTDirectory.MT_900, MTDirectory.MT_910, MTDirectory.MT_548 };
        public static readonly List<string> MTMessageTypesToIgnore = new List<string>() { MTDirectory.MT_094 };

        public static WireInBoundMessage ParseMessage(string swiftMessage)
        {
            var inBoundWireMessage = new WireInBoundMessage().Parse(swiftMessage);

            if (inBoundWireMessage.WireId == 0)
                throw new UnhandledWireMessageException(string.Format("Unknown Transaction ref for MT {0}:{1}/{2}", inBoundWireMessage.SwiftMessage.GetMTType(),
                    inBoundWireMessage.SwiftMessage.GetFieldValue(FieldDirectory.FIELD_20), inBoundWireMessage.SwiftMessage.GetFieldValue(FieldDirectory.FIELD_21)));

            if (inBoundWireMessage.IsFeAck)
                return inBoundWireMessage;

            if (MTMessageTypesToIgnore.Any(s => s.Equals(inBoundWireMessage.SwiftMessage.GetMTType())))
                return inBoundWireMessage;

            if (inBoundWireMessage.IsAckOrNack)
                return ParseServiceMessage(inBoundWireMessage);

            //MT 548 Status of original 540/542 e.g.matched, settled, partial settlement, etc.      
            if (inBoundWireMessage.SwiftMessage.IsType("548"))
                return Parse548(inBoundWireMessage);

            //MT196 and 296 and 900, 910- confirmation of Cancellation
            if (HandledMTMessageConfirmationList.Any(s => s.Equals(inBoundWireMessage.SwiftMessage.GetMTType())))
                return ParseGeneralConfirmation(inBoundWireMessage);

            return ParseUnHandled(inBoundWireMessage);
        }

        private static WireInBoundMessage ParseUnHandled(WireInBoundMessage message)
        {
            message.ExceptionMessage = string.Format("Wire Message type MT{0} not handled", message.MessageType);
            return message;
        }

        public static WireInBoundMessage ParseServiceMessage(WireInBoundMessage message)
        {
            if (message.SwiftMessage.IsAck())
                message.IsAcknowledged = true;

            if (!message.SwiftMessage.IsNack())
                return message;

            message.IsNegativeAcknowledged = true;
            message.ExceptionMessage = string.Format("Swift returned Un-acknowledged for wire ticket Id :{0}  with errorCode: {1}", message.WireId, message.SwiftMessage.GetNackReasonCode());
            return message;
        }

        private static WireInBoundMessage ParseGeneralConfirmation(WireInBoundMessage message)
        {
            message.IsConfirmed = true;

            //This applies to MT 196 and MT 296
            var confirmationMessage = message.SwiftMessage.GetFieldValue("76");
            if (!string.IsNullOrWhiteSpace(confirmationMessage))
                message.ConfirmationMessage = confirmationMessage;

            return message;
        }

        public static WireInBoundMessage Parse548(WireInBoundMessage message)
        {
            return message;
        }
    }
}

using HMOSecureMiddleware.Models;

namespace HMOSecureMiddleware.SwiftMessageManager
{
    public class SwiftMessageParser
    {
        public static WireInBoundMessage ParseMessage(string swiftMessage)
        {
            var inBoundWireMessage = new WireInBoundMessage(swiftMessage);

            if (inBoundWireMessage.IsAckOrNack)
                return ParseServiceMessage(inBoundWireMessage);

            //MT 900 Confirmation of Debit OR //MT 910 Confirmation of Credit
            if (inBoundWireMessage.SwiftMessage.IsType("900") || inBoundWireMessage.SwiftMessage.IsType("910"))
                return Parse900Or910Confirmation(inBoundWireMessage);

            //MT 548 Status of original 540/542 e.g.matched, settled, partial settlement, etc.      
            if (inBoundWireMessage.SwiftMessage.IsType("548"))
                return Parse548(inBoundWireMessage);

            //MT196 and 296 - confirmation of Cancelation
            if (inBoundWireMessage.SwiftMessage.IsType("296") || inBoundWireMessage.SwiftMessage.IsType("196"))
                return Parse196Or296Confirmation(inBoundWireMessage);

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

        private static WireInBoundMessage Parse196Or296Confirmation(WireInBoundMessage message)
        {
            message.IsConfirmed = true;
            message.ConfirmationMessage = message.SwiftMessage.GetFieldValue("76");

            //We might need to compare with the original message and double check on the confirmation

            return message;
        }

        private static WireInBoundMessage Parse900Or910Confirmation(WireInBoundMessage message)
        {
            message.IsConfirmed = true;

            //We might need to compare with the original message and double check on the confirmation

            return message;
        }

        public static WireInBoundMessage Parse548(WireInBoundMessage message)
        {
            return message;
        }
    }
}

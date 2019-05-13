using System.Text;
using HedgeMark.SwiftMessageHandler.Model;
using HedgeMark.SwiftMessageHandler.Model.Fields;

namespace HedgeMark.SwiftMessageHandler
{
    public class SwiftMessageInterpreter
    {
        public static string GetSimpleFormatted(string swiftMessage)
        {
            if (string.IsNullOrWhiteSpace(swiftMessage))
                return string.Empty;

            return GetSimpleFormatted(SwiftMessage.Parse(swiftMessage));
        }

        public static string GetSimpleFormatted(SwiftMessage swiftMessage)
        {
            var builder = new StringBuilder();
            foreach (var field in swiftMessage.Block4.GetFields())
            {
                var parentFieldName = string.Format("{0}: ", field.Name);
                parentFieldName = parentFieldName.PadRight(parentFieldName.Length + (5 - parentFieldName.Length), ' ');
                builder.AppendFormat(parentFieldName);

                builder.Append(field.Label);
                var fieldLength = 8;

                if (field.Components.Count == 0)
                {
                    builder.AppendLine(string.Format(":{0}", field.GetComponentValue(field.Label)));
                    continue;
                }

                builder.AppendLine();

                foreach (var component in field.Components)
                {
                    var componentValue = field.GetComponentValue(component);

                    if (string.IsNullOrWhiteSpace(componentValue))
                        continue;

                    var fieldInfo = string.Format("{0}: {1}", component, componentValue);
                    fieldInfo = fieldInfo.PadLeft(fieldInfo.Length + fieldLength, ' ');
                    builder.AppendLine(fieldInfo);
                }
            }
            return builder.ToString();
        }

        public static string GetDetailedFormatted(string swiftMessage, bool shouldIncludeOriginalMessage = false)
        {
            if (string.IsNullOrWhiteSpace(swiftMessage))
                return string.Empty;

            var formattedMessage = GetDetailedFormatted(SwiftMessage.Parse(swiftMessage));

            if (!shouldIncludeOriginalMessage)
                return formattedMessage;

            var builder = new StringBuilder();

            builder.AppendLine("------------------------------ Original FIN Message ------------------------------");
            builder.AppendLine(swiftMessage);

            return string.Format("{0}{1}", builder, formattedMessage);
        }


        public static string GetDetailedFormatted(SwiftMessage swiftMsg)
        {
            var builder = new StringBuilder();

            builder.AppendLine("------------------------- Instance Type and Transmission ----------------------------");

            if (swiftMsg.IsServiceMessage21() || swiftMsg.IsIncoming())
            {
                builder.AppendLine("Copy received from SWIFT");
            }
            else if (swiftMsg.IsOutgoing())
            {
                builder.AppendLine("Copy sent to SWIFT");
            }

            if (swiftMsg.IsServiceMessage21())
            {
                builder.Append("Message format : Service message for ");

                if (swiftMsg.IsAck())
                    builder.Append("Acknowledgement");
                else
                    builder.Append("N-Acknowledgement");
            }

            else if (swiftMsg.Block2.Priority != null)
            {
                builder.Append("Priority/Delivery : ");
                builder.Append(swiftMsg.Block2.GetPriorityLabel());
                if (swiftMsg.IsOutgoing())
                {
                    if (swiftMsg.Block2.GetDeliveryMonitoringLabel() != null)
                    {
                        builder.Append("/");
                        builder.Append(swiftMsg.Block2.GetDeliveryMonitoringLabel());
                    }
                }
            }
            builder.AppendLine();
            if (swiftMsg.IsIncoming())
            {
                builder.AppendLine("Message Input Reference : " + swiftMsg.Block2.GetMessageInputReference());
                //builder.AppendLine("Input Date and Time : " + swiftMsg.Block2.GetSenderInputDateAndTime());
                //builder.AppendLine("Output Date and Time : " + swiftMsg.Block2.GetSenderOutputDateAndTime());
            }

            builder.AppendLine("------------------------- Message Header -----------------------------------------");
            builder.AppendLine(string.Format("Swift    : MT {0}", swiftMsg.MessageType));
            builder.AppendLine(string.Format("Sender   : {0}", swiftMsg.GetSender()));
            builder.AppendLine(string.Format("Receiver : {0}", swiftMsg.GetReceiver()));
            if (!string.IsNullOrWhiteSpace(swiftMsg.GetMUR()))
            {
                builder.AppendLine(string.Format("MUR      : {0}", swiftMsg.GetMUR()));
            }

            builder.AppendLine("------------------------- Message Text -------------------------------------------");

            builder.Append(GetSimpleFormatted(swiftMsg));

            if (swiftMsg.Block5.GetFields().Count > 0)
            {
                builder.AppendLine("---------------------------- Message Trailer -------------------------------------");
                foreach (Field field in swiftMsg.Block5.GetFields())
                {
                    builder.AppendLine(string.Format("{0}: {1}", field.Name, field.Value.Trim()));
                }
            }

            builder.AppendLine("------------------------------ End Of Message ------------------------------------");
            return builder.ToString();
        }
    }
}

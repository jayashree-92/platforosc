using System;
using System.Globalization;

namespace HedgeMark.SwiftMessageHandler.Model.Blocks
{
    public class SwiftBlock2 : SwiftBlock
    {
        public SwiftBlock2() : base("2", "Application Header")
        {
            SetDefaults();
        }
        public SwiftBlock2(string messageType) : base("2", "Application Header")
        {
            MessageType = messageType.Replace("MT", string.Empty).Trim().Substring(0, 3);
            SetDefaults();
        }

        private void SetDefaults()
        {
            InputOrOutputId = "I";
            Receiver = "TESTUS0000XXXX";
            Priority = "N";
            DeliveryMonitoring = "";
            ObsolescencePeriod = string.Empty;
            SenderInputTime = string.Empty;
            SenderInputDate = string.Empty;
        }

        /// <summary>
        /// 	For an input message, the Input/Output Identifier consists of the single letter 'I'
        ///     For an output message, the Input/Output Identifier consists of the single letter 'O'
        /// </summary>
        public string InputOrOutputId { get; set; }
        public string MessageType { get; set; }
        public string Receiver { get; set; }

        /// <summary>
        /// This character, used within FIN Application Headers only, defines the priority with which a message is delivered. The possible values are:
        ///     S = System
        ///     U = Urgent
        ///     N = Normal
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Delivery monitoring options apply only to FIN user-to-user messages. The chosen option is expressed as a single digit:
        ///     1 = Non-Delivery Warning
        ///     2 = Delivery Notification
        ///     3 = Non-Delivery Warning and Delivery Notification
        ///If the message has priority 'U', the user must request delivery monitoring option '1' or '3'. If the message has priority 'N', the user can request delivery monitoring option '2' or, by leaving the option blank, no delivery monitoring.
        /// </summary>
        public string DeliveryMonitoring { get; set; }

        /// <summary>
        /// The obsolescence period defines the period of time after which a Delayed Message (DLM) trailer is added to a FIN user-to-user message when the message is delivered. 
        /// For urgent priority messages, it is also the period of time after which, if the message remains undelivered, a Non-Delivery Warning is generated. 
        /// The values for the obsolescence period are: 
        ///     003 (15 minutes) for 'U' priority, 
        ///     and 020 (100 minutes) for 'N' priority.
        /// </summary>
        public string ObsolescencePeriod { get; set; }

        /// <summary>
        /// No info available - but few swift message has this format - need to check
        /// </summary>
        public string SenderInputTime { get; set; }

        /// <summary>
        /// No info available - but few swift message has this format - need to check
        /// </summary>
        public string SenderInputDate { get; set; }


        /// <summary>
        /// No info available - but few swift message has this format - need to check
        /// </summary>
        public string SenderOutputTime { get; set; }

        /// <summary>
        /// No info available - but few swift message has this format - need to check
        /// </summary>
        public string SenderOutputDate { get; set; }

        public string SenderSessionNo { get; set; }
        public string SenderSequenceNo { get; set; }


        public string GetDeliveryMonitoringLabel()
        {
            switch (DeliveryMonitoring)
            {
                case "1":
                    return "Non-Delivery Warning";
                case "2":
                    return "Delivery Notification";
                case "3":
                    return "Non-Delivery Warning and Delivery Notification";
            }
            return null;
        }

        public string GetPriorityLabel()
        {
            switch (Priority)
            {
                case "S":
                    return "System";
                case "U":
                    return "Urgent";
                case "N":
                    return "Normal";
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType">Swift message Type</param>
        /// <returns></returns>
        public string GetBlock(string messageType)
        {
            MessageType = messageType.Replace("MT", string.Empty).Trim().Substring(0, 3);
            return GetBlock();
        }

        internal void SetReceiver(string receiver)
        {
            Receiver = receiver.PadRight(12, 'X');
        }

        internal string GetReceiver()
        {
            return Receiver;
        }

        public string GetSenderInputDateAndTime()
        {
            DateTime senderInputTime;
            DateTime.TryParseExact(SenderInputDate + SenderInputTime, "yyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out senderInputTime);
            return senderInputTime.Year == 1 ? null : senderInputTime.ToString("dddd, dd MMM yyyy hh:mm tt");
        }

        public string GetSenderOutputDateAndTime()
        {
            DateTime senderInputTime;
            DateTime.TryParseExact(SenderOutputDate + SenderOutputTime, "yyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out senderInputTime);
            return senderInputTime.Year == 1 ? null : senderInputTime.ToString("dddd, dd MMM yyyy hh:mm tt");
        }

        public string GetMessageInputReference()
        {
            return string.Format("{0}{1}{2}", Receiver, SenderSessionNo, SenderSequenceNo);
        }

        public string GetBlockValue()
        {
            MessageType = MessageType.Replace("MT", string.Empty).Trim().Substring(0, 3);
            return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", InputOrOutputId, MessageType, SenderInputTime, SenderInputDate, Receiver, SenderSessionNo, SenderSequenceNo, SenderOutputDate, SenderOutputTime, Priority, DeliveryMonitoring, ObsolescencePeriod);
        }

        public override string GetBlock()
        {
            return string.Format("{{{0}:{1}}}", Name, GetBlockValue());
        }

        public void SetBlock(SwiftBlock2 block2)
        {
            DeliveryMonitoring = block2.DeliveryMonitoring;
            InputOrOutputId = block2.InputOrOutputId;
            MessageType = block2.MessageType;
            ObsolescencePeriod = block2.ObsolescencePeriod;
            Priority = block2.Priority;
            Receiver = block2.Receiver;
            SenderInputDate = block2.SenderInputDate;
            SenderInputTime = block2.SenderInputTime;
        }
    }
}

using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.MT
{
    public static class MTDirectory
    {
        public const string MT_103 = "103";
        public const string MT_103STP = "103STP";
        public const string MT_103REMIT = "103REMIT";
        public const string MT_202 = "202";
        public const string MT_202COV = "202COV";
        public const string MT_205 = "205";
        public const string MT_205COV = "205COV";
        public const string MT_210 = "210";
        public const string MT_192 = "192";
        public const string MT_292 = "292";
        public const string MT_540 = "540";
        public const string MT_542 = "542";


        //Inbound Messages
        public const string MT_548 = "548";
        public const string MT_900 = "900";
        public const string MT_940 = "940";
        public const string MT_910 = "910";
        public const string MT_196 = "196";
        public const string MT_296 = "296";
        public const string MT_094 = "094";


        public static readonly List<string> InBoundMessageTypes = new List<string>()
        {
            MT_548,MT_900,MT_910,MT_196,MT_296,MT_940
        };

        public static bool IsOutBoundMessage(SwiftMessage message)
        {
            return InBoundMessageTypes.Contains(message.MessageType);
        }

        public static Dictionary<string, string> Labels = new Dictionary<string, string>()
        {
            //Outbound messages
            {MT_103,"Single Customer Credit Transfer" },
            {MT_202,"General Financial Institution Transfer" },
            {MT_202COV,"General Financial Institution Transfer" },
            {MT_210,"Single Customer Credit Transfer" },
            {MT_192,"Request for cancelation" },
            {MT_292,"Request for cancelation" },
            {MT_540,"Receive Free" },
            {MT_542,"Deliver Free" },

            //Inbound messages
            {MT_548,"Status of original 540/542 e.g.matched, settled, partial settlement, etc."},
            {MT_900,"Confirmation of Debit"},
            {MT_910,"Confirmation of Credit"},
            {MT_196,"Confirmation of Cancellation"},
            {MT_296,"Confirmation of Cancellation"},
            {MT_094,"Broadcast"}
        };
    }
}

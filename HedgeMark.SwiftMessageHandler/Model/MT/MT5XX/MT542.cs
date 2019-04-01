namespace HedgeMark.SwiftMessageHandler.Model.MT.MT5XX
{
    public class MT542 : AbstractMT
    {
        public MT542() : base(MTDirectory.MT_542)
        {
        }

        public MT542(string sender, string receiver) : base(MTDirectory.MT_542)
        {
            setSenderAndReceiver(sender, receiver);
        }
    }
}

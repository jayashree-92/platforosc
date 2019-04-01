namespace HedgeMark.SwiftMessageHandler.Model.MT.MT5XX
{
    public class MT540 : AbstractMT
    {
        public MT540() : base(MTDirectory.MT_540)
        {
        }

        public MT540(string sender, string receiver) : base(MTDirectory.MT_540)
        {
            setSenderAndReceiver(sender, receiver);
        }
    }
}

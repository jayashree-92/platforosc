namespace HedgeMark.SwiftMessageHandler.Model.MT.MT5XX
{
    public class MT548 : AbstractMT
    {
        public MT548() : base(MTDirectory.MT_548)
        {
        }

        public MT548(string sender, string receiver) : base(MTDirectory.MT_548)
        {
            setSenderAndReceiver(sender, receiver);
        }
    }
}

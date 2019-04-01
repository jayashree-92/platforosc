namespace HedgeMark.SwiftMessageHandler.Model.MT.MT1XX
{
    public class MT192 : AbstractMT
    {
        public MT192() : base(MTDirectory.MT_192)
        {
        }

        public MT192(string sender, string receiver) : base(MTDirectory.MT_192)
        {
            setSenderAndReceiver(sender, receiver);
        }
    }
}

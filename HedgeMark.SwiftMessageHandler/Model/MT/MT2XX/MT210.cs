namespace HedgeMark.SwiftMessageHandler.Model.MT.MT2XX
{
    public class MT210 : AbstractMT
    {
        public MT210() : base(MTDirectory.MT_210)
        {
        }

        public MT210(string sender, string receiver) : base(MTDirectory.MT_210)
        {
            setSenderAndReceiver(sender, receiver);
        }
    }
}

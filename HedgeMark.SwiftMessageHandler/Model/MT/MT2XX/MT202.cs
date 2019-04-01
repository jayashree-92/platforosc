namespace HedgeMark.SwiftMessageHandler.Model.MT.MT2XX
{
    public class MT202 : AbstractMT
    {
        public MT202() : base(MTDirectory.MT_202)
        {
        }

        public MT202(string sender, string receiver) : base(MTDirectory.MT_202)
        {
            setSenderAndReceiver(sender, receiver);
        }
    }
}

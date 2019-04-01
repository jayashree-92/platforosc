namespace HedgeMark.SwiftMessageHandler.Model.MT.MT2XX
{
    public class MT202COV : AbstractMT
    {
        public MT202COV() : base(MTDirectory.MT_202COV)
        {
        }

        public MT202COV(string sender, string receiver) : base(MTDirectory.MT_202COV)
        {
            setSenderAndReceiver(sender, receiver);
        }
    }
}

namespace HedgeMark.SwiftMessageHandler.Model.MT.MT2XX
{
    public class MT292 : AbstractMT
    {
        public MT292() : base(MTDirectory.MT_292)
        {
        }

        public MT292(string sender, string receiver) : base(MTDirectory.MT_292)
        {
            setSenderAndReceiver(sender, receiver);
        }
    }
}

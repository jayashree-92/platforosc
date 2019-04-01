namespace HedgeMark.SwiftMessageHandler.Model.Blocks
{
    public abstract class SwiftBlock
    {
        public string Name { get; private set; }
        public string Label { get; private set; }

        protected SwiftBlock(string name, string label)
        {
            Name = name;
            Label = label;
        }

        public abstract string GetBlock();
    }
}

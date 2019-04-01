namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field16S : Field
    {
        public Field16S() : base(FieldDirectory.FIELD_16S)
        {
        }

        public Field16S setBlockName(string blockName)
        {
            this.Value = blockName;
            return this;
        }
    }
}

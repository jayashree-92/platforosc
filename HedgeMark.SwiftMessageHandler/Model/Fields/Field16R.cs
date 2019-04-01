namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field16R : Field
    {
        public Field16R() : base(FieldDirectory.FIELD_16R)
        {
        }

        public Field16R setBlockName(string value)
        {
            this.Value = value;
            return this;
        }
    }
}

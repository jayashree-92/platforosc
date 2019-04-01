namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field25 : Field
    {
        public Field25() : base(FieldDirectory.FIELD_25)
        {
        }

        public Field25 setAccount(string value)
        {
            this.Value = value;
            return this;
        }
    }
}

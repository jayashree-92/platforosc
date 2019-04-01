namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field119 : Field
    {
        public Field119() : base(FieldDirectory.FIELD_119)
        {
        }

        public Field119(string validationFlag) : base(FieldDirectory.FIELD_119)
        {
            Value = validationFlag;
        }

        public Field119 setValidationFlag(string validationFlag)
        {
            Value = validationFlag;
            return this;
        }
    }
}

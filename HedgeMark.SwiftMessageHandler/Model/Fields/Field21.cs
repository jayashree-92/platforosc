using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field21 : Field
    {
        public Field21() : base(FieldDirectory.FIELD_21)
        {
        }

        public Field21 setReference(string value)
        {
            this.Value = value;
            return this;
        }
        public override List<string> Components
        {
            get
            {
                return new List<string>() { FieldConstants.REFERENCE };
            }
        }
    }
}

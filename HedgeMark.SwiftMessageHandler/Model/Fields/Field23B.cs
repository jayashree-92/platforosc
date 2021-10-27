using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field23B : Field
    {
        public Field23B() : base(FieldDirectory.FIELD_23B)
        {

        }
        public Field23B(string code) : base(FieldDirectory.FIELD_23B)
        {
            this.Value = code;
        }

        public Field23B setType(string code)
        {
            this.Value = code;
            return this;
        }

        public override List<string> Components => new List<string>() { FieldConstants.TYPE };
    }
}

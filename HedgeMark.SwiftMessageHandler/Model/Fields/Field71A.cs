using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public sealed class Field71A : Field
    {
        public Field71A() : base(FieldDirectory.FIELD_71A)
        {
        }

        public Field71A(string code) : base(FieldDirectory.FIELD_71A)
        {
            setCode(code);
        }
        public Field71A setCode(string code)
        {
            Value = code;
            return this;
        }

        public override List<string> Components => new List<string>() { FieldConstants.CODE };

        public override string GetComponentValue(string component)
        {
            return Value;
        }
    }
}

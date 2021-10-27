using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field20 : Field
    {
        public Field20() : base(FieldDirectory.FIELD_20)
        {
        }

        public Field20(string transactionId) : base(FieldDirectory.FIELD_20)
        {
            this.Value = transactionId;
        }

        public override List<string> Components => new List<string>() { FieldConstants.REFERENCE };
    }
}

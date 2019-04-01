using System;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field121 : Field
    {
        public Field121() : base(FieldDirectory.FIELD_121)
        {
            //Setting default value
            Value = Guid.NewGuid().ToString();
        }

        public Field121 setUniqueReference(string uniqueReference)
        {
            Value = uniqueReference;
            return this;
        }
    }
}

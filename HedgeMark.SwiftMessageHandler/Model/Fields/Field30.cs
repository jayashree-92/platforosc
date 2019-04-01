using System;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field30 : FieldWithDate
    {
        public Field30() : base(FieldDirectory.FIELD_30)
        {
        }

        public Field30 setDate(DateTime dateTime)
        {
            return setDate(this, dateTime);
        }
    }
}

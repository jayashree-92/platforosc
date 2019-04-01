using System;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
   public  class Field98C: FieldWithQualifierDateAndTime
    {
        public Field98C() : base(FieldDirectory.FIELD_98C)
        {
        }

        public Field98C setDate(DateTime dateTime)
        {
            return setDate(this, dateTime);
        }
        
        public Field98C setQualifier(string qualifier)
        {
            return setQualifier(this, qualifier);
        }
    }
}

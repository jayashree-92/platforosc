using System;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field98A : FieldWithQualifierDateAndTime
    {
        //	:4!c//8!n	(Qualifier)(Date)and(Time)
        //  :SETT//20190228
        public Field98A() : base(FieldDirectory.FIELD_98A)
        {
        }

        public Field98A setDate(DateTime dateTime)
        {
            return setDate(this, dateTime);
        }

        public Field98A setQualifier(string qualifier)
        {
            return setQualifier(this, qualifier);
        }

    }
}

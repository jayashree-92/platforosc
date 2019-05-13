using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public sealed class Field70 : FieldWithNarrative
    {
        public Field70() : base(FieldDirectory.FIELD_70)
        {

        }
        public Field70 setNarrative(string narrative)
        {
            return setNarrative(this, narrative);
        }
        public Field70 setNarrativeLine1(string narrativeLine1)
        {
            return setNarrativeLine1(this, narrativeLine1);
        }
        public Field70 setNarrativeLine2(string narrativeLine2)
        {
            return setNarrativeLine2(this, narrativeLine2);
        }
        public Field70 setNarrativeLine3(string narrativeLine3)
        {
            return setNarrativeLine3(this, narrativeLine3);
        }
        public Field70 setNarrativeLine4(string narrativeLine4)
        {
            return setNarrativeLine4(this, narrativeLine4);
        }
        //public Field70 setNarrativeLine5(string narrativeLine5)
        //{
        //    return setNarrativeLine5(this, narrativeLine5);
        //}
        //public Field70 setNarrativeLine6(string narrativeLine6)
        //{
        //    return setNarrativeLine6(this, narrativeLine6);
        //}
    }
}

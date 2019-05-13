using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public sealed class Field72 : FieldWithNarrative
    {
        public Field72() : base(FieldDirectory.FIELD_72)
        {

        }
        public Field72 setNarrative(string narrative)
        {
            return setNarrative(this, narrative);
        }
        public Field72 setNarrativeLine1(string narrativeLine1)
        {
            return setNarrativeLine1(this, narrativeLine1);
        }
        public Field72 setNarrativeLine2(string narrativeLine2)
        {
            return setNarrativeLine2(this, narrativeLine2);
        }
        public Field72 setNarrativeLine3(string narrativeLine3)
        {
            return setNarrativeLine3(this, narrativeLine3);
        }
        public Field72 setNarrativeLine4(string narrativeLine4)
        {
            return setNarrativeLine4(this, narrativeLine4);
        }
        public Field72 setNarrativeLine5(string narrativeLine5)
        {
            return setNarrativeLine5(this, narrativeLine5);
        }
        public Field72 setNarrativeLine6(string narrativeLine6)
        {
            return setNarrativeLine6(this, narrativeLine6);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public sealed class Field72 : Field
    {
        public Field72() : base(FieldDirectory.FIELD_72)
        {

        }

        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.NARRATIVE
                };
            }
        }

        public override string GetComponentValue(string component)
        {
            return !string.IsNullOrWhiteSpace(Narrative) ? Narrative : Value;
        }

        public string Narrative { get; set; }
        public string NarrativeLine1 { get; set; }
        public string NarrativeLine2 { get; set; }
        public string NarrativeLine3 { get; set; }
        public string NarrativeLine4 { get; set; }
        public string NarrativeLine5 { get; set; }

        public override string GetValue()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Narrative))
                return Narrative;

            if (!string.IsNullOrWhiteSpace(NarrativeLine1))
                builder.Append(NarrativeLine1);
            if (!string.IsNullOrWhiteSpace(NarrativeLine2))
                builder.AppendFormat("{0}{1}", Environment.NewLine, NarrativeLine2);
            if (!string.IsNullOrWhiteSpace(NarrativeLine3))
                builder.AppendFormat("{0}{1}", Environment.NewLine, NarrativeLine3);
            if (!string.IsNullOrWhiteSpace(NarrativeLine4))
                builder.AppendFormat("{0}{1}", Environment.NewLine, NarrativeLine4);
            if (!string.IsNullOrWhiteSpace(NarrativeLine5))
                builder.AppendFormat("{0}{1}", Environment.NewLine, NarrativeLine5);

            return builder.ToString();
        }

        public Field72 setNarrative(string narrative)
        {
            Narrative = narrative;
            return this;
        }

        public Field72 setNarrativeLine1(string narrativeLine1)
        {
            NarrativeLine1 = narrativeLine1;
            return this;
        }
        public Field72 setNarrativeLine2(string narrativeLine2)
        {
            NarrativeLine2 = narrativeLine2;
            return this;
        }
        public Field72 setNarrativeLine3(string narrativeLine3)
        {
            NarrativeLine3 = narrativeLine3;
            return this;
        }
        public Field72 setNarrativeLine4(string narrativeLine4)
        {
            NarrativeLine4 = narrativeLine4;
            return this;
        }
        public Field72 setNarrativeLine5(string narrativeLine5)
        {
            NarrativeLine5 = narrativeLine5;
            return this;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HedgeMark.SwiftMessageHandler.Utils;

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
        
        public string NarrativeLine1 { get; set; }
        public string NarrativeLine2 { get; set; }
        public string NarrativeLine3 { get; set; }
        public string NarrativeLine4 { get; set; }
        public string NarrativeLine5 { get; set; }
        public string NarrativeLine6 { get; set; }


        private string Narrative
        {
            get
            {
                var builder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(NarrativeLine1))
                    builder.AppendFormat("{0}", NarrativeLine1.TrimToLength());
                if (!string.IsNullOrWhiteSpace(NarrativeLine2))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, NarrativeLine2.TrimToLength());
                if (!string.IsNullOrWhiteSpace(NarrativeLine3))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, NarrativeLine3.TrimToLength());
                if (!string.IsNullOrWhiteSpace(NarrativeLine4))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, NarrativeLine4.TrimToLength());
                if (!string.IsNullOrWhiteSpace(NarrativeLine5))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, NarrativeLine5.TrimToLength());
                if (!string.IsNullOrWhiteSpace(NarrativeLine6))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, NarrativeLine6.TrimToLength());
                return builder.ToString();
            }
        }


        public override string GetValue()
        {
            var builder = new StringBuilder();
            
            if (!string.IsNullOrWhiteSpace(Narrative))
                builder.Append(Narrative);
            

            return builder.ToString();
        }

        public Field72 setNarrative(string narrative)
        {
            narrative = Regex.Replace(narrative, @"\t|\r", "");

            var narrativeLines = narrative.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (narrativeLines.Length > 0)
                setNarrativeLine1(narrativeLines[0].Trim());
            if (narrativeLines.Length > 1)
                setNarrativeLine2(narrativeLines[1].Trim());
            if (narrativeLines.Length > 2)
                setNarrativeLine3(narrativeLines[2].Trim());
            if (narrativeLines.Length > 3)
                setNarrativeLine4(narrativeLines[3].Trim());
            if (narrativeLines.Length > 4)
                setNarrativeLine5(narrativeLines[4].Trim());
            if (narrativeLines.Length > 5)
                setNarrativeLine6(narrativeLines[5].Trim());
            
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
        public Field72 setNarrativeLine6(string narrativeLine6)
        {
            NarrativeLine6 = narrativeLine6;
            return this;
        }
    }
}

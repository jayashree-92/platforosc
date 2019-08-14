using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithNarrative : Field
    {
        public FieldWithNarrative(string name) : base(name)
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

        public T setNarrative<T>(T callingMethod, string narrative)
        {
            if (string.IsNullOrWhiteSpace(narrative))
                return callingMethod;

            narrative = Regex.Replace(narrative, @"\t|\r", string.Empty);
            var narrativeLines = narrative.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (narrativeLines.Length > 0)
                setNarrativeLine1(callingMethod, narrativeLines[0].Trim());
            if (narrativeLines.Length > 1)
                setNarrativeLine2(callingMethod, narrativeLines[1].Trim());
            if (narrativeLines.Length > 2)
                setNarrativeLine3(callingMethod, narrativeLines[2].Trim());
            if (narrativeLines.Length > 3)
                setNarrativeLine4(callingMethod, narrativeLines[3].Trim());
            if (narrativeLines.Length > 4)
                setNarrativeLine5(callingMethod, narrativeLines[4].Trim());
            if (narrativeLines.Length > 5)
                setNarrativeLine6(callingMethod, narrativeLines[5].Trim());

            return callingMethod;
        }

        public T setNarrativeLine1<T>(T callingMethod, string narrativeLine1)
        {
            if (!string.IsNullOrWhiteSpace(narrativeLine1))
                narrativeLine1 = RemoveInvalidXCharacterSet(narrativeLine1);

            NarrativeLine1 = narrativeLine1;
            return callingMethod;
        }

        public T setNarrativeLine2<T>(T callingMethod, string narrativeLine2)
        {
            if (!string.IsNullOrWhiteSpace(narrativeLine2))
                narrativeLine2 = RemoveInvalidXCharacterSet(narrativeLine2);

            NarrativeLine2 = narrativeLine2;
            return callingMethod;
        }

        public T setNarrativeLine3<T>(T callingMethod, string narrativeLine3)
        {
            if (!string.IsNullOrWhiteSpace(narrativeLine3))
                narrativeLine3 = RemoveInvalidXCharacterSet(narrativeLine3);

            NarrativeLine3 = narrativeLine3;
            return callingMethod;
        }

        public T setNarrativeLine4<T>(T callingMethod, string narrativeLine4)
        {
            if (!string.IsNullOrWhiteSpace(narrativeLine4))
                narrativeLine4 = RemoveInvalidXCharacterSet(narrativeLine4);

            NarrativeLine4 = narrativeLine4;
            return callingMethod;
        }
        public T setNarrativeLine5<T>(T callingMethod, string narrativeLine5)
        {
            if (!string.IsNullOrWhiteSpace(narrativeLine5))
                narrativeLine5 = RemoveInvalidXCharacterSet(narrativeLine5);

            NarrativeLine5 = narrativeLine5;
            return callingMethod;
        }
        public T setNarrativeLine6<T>(T callingMethod, string narrativeLine6)
        {
            if (!string.IsNullOrWhiteSpace(narrativeLine6))
                narrativeLine6 = RemoveInvalidXCharacterSet(narrativeLine6);

            NarrativeLine6 = narrativeLine6;
            return callingMethod;
        }
    }
}

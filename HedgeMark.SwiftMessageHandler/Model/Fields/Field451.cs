using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field451 : Field
    {
        public Field451() : base(FieldDirectory.FIELD_451)
        {
        }

        public override string GetComponentValue(string component)
        {
            return Value == "0" ? "Accepted by the SWIFT Network" : "Rejected by the SWIFT Network";
        }

        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    {FieldConstants.STATUS}
                };
            }
        }

    }
}

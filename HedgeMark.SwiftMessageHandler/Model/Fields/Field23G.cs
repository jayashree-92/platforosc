using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field23G : Field
    {
        //	4!c[/4!c]	(Function)(Subfunction)
        // :23G:INST
        public Field23G() : base(FieldDirectory.FIELD_23G)
        {
        }

        public string Function { get; set; }

        public string SubFunction { get; set; }

        public Field23G setFunction(string function)
        {
            Function = function;
            return this;
        }

        public Field23G setSubFunction(string function)
        {
            SubFunction = function;
            return this;
        }

        public override string GetValue()
        {
            return Value = string.Format("{0}{1}", Function, SubFunction);
        }

        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.FUNCTION,FieldConstants.SUB_FUNCTION
                };
            }
        }

        public override string GetComponentValue(string component)
        {
            switch (component)
            {
                case FieldConstants.FUNCTION:
                    return Value.Length >= 4 ? Value.Substring(0, 4) : string.Empty;

                case FieldConstants.SUB_FUNCTION:
                    return Value.Length > 4 ? Value.Substring(4, Value.Length - 4) : string.Empty;

            }

            return string.Empty;
        }

    }
}

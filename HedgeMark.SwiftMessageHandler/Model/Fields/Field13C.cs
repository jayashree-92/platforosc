using System.Collections.Generic;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field13C : Field
    {
        /// <summary>
        /// /8c/4!n1!s4!n
        /// </summary>
        public Field13C() : base(FieldDirectory.FIELD_13C)
        {
        }

        public override List<string> Components
        {
            get
            {
                return new List<string>() { FieldConstants.CODE, FieldConstants.TIME_INDICATION, FieldConstants.SIGN, FieldConstants.TIME_OFFSET };
            }
        }


        public string Code { get; set; }
        public string Time { get; set; }
        public string Sign { get; set; }
        public string OffSet { get; set; }


        public override string GetValue()
        {
            return Value = string.Format("/{0}/{1}{2}{3}", Code, Time, Sign ?? "+", OffSet ?? "0000");
        }

        public override string GetComponentValue(string component)
        {
            var componentValue = string.Empty;
            var editedVal = Value.Replace("/", string.Empty);

            switch (component)
            {
                case FieldConstants.CODE:
                    componentValue = !string.IsNullOrWhiteSpace(Code) ? Code : editedVal.Length >= 7 ? editedVal.Substring(0, 7) : string.Empty;
                    break;
                case FieldConstants.TIME_INDICATION:
                    componentValue = !string.IsNullOrWhiteSpace(Time) ? Time : editedVal.Length >= 11 ? editedVal.Substring(7, 4) : string.Empty;
                    componentValue = componentValue.Length == 4 ? string.Format("{0}{1}:{2}{3}", componentValue[0], componentValue[1], componentValue[2], componentValue[3]) : string.Empty;
                    break;
                case FieldConstants.SIGN:
                    componentValue = !string.IsNullOrWhiteSpace(Sign) ? Sign : editedVal.Length >= 12 ? editedVal.Substring(11, 1) : string.Empty;
                    break;
                case FieldConstants.TIME_OFFSET:
                    componentValue = !string.IsNullOrWhiteSpace(Sign) ? Sign : editedVal.Length >= 16 ? editedVal.Substring(12, 4) : string.Empty;
                    break;
            }

            return component.GetComponentValue(componentValue);
        }
    }
}

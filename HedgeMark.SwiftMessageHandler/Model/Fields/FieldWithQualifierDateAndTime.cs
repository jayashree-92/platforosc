using System;
using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithQualifierDateAndTime : Field
    {
        public FieldWithQualifierDateAndTime(string name) : base(name)
        {

        }

        public string Qualifier { get; set; }
        public string DateString { get; set; }
        public string TimeString { get; set; }


        public T setDate<T>(T callingClass, DateTime dateTime)
        {
            this.DateString = dateTime.ToString("yyyyMMdd");
            this.TimeString = dateTime.ToString("hhmmss");
            return callingClass;
        }

        public T setQualifier<T>(T callingClass, string qualifier)
        {
            this.Qualifier = qualifier;
            FieldValidator.Validate(qualifier, "4!c");
            return callingClass;
        }


        public override string GetValue()
        {
            return string.Format(":{0}//{1}{2}", Qualifier, DateString, TimeString);
        }

        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.QUALIFIER,FieldConstants.DATE
                };
            }
        }

        public override string GetComponentValue(string component)
        {
            var splitUps = Value.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
            switch (component)
            {
                case FieldConstants.QUALIFIER:
                    if (!string.IsNullOrWhiteSpace(Qualifier)) return Qualifier;
                    return splitUps[0].Replace(":", string.Empty);

                case FieldConstants.DATE:

                    if (splitUps.Length > 1 && splitUps[1].Length >= 8)
                        return splitUps[1].Substring(0, 8);
                    return string.Empty;

                case FieldConstants.TIME_INDICATION:
                    if (splitUps.Length > 1 && splitUps[1].Length > 8)
                        return splitUps[1].Substring(8);
                    return string.Empty;
            }

            return string.Empty;
        }
    }
}

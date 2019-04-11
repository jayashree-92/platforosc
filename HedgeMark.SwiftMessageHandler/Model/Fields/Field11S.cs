using System;
using System.Collections.Generic;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field11S : FieldWithDate
    {
        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    {FieldConstants.MT},
                    {FieldConstants.DATE},
                    {FieldConstants.SESSION},
                    {FieldConstants.ISN},
                };
            }
        }

        public override string GetComponentValue(string component)
        {
            var derivedValue = string.Empty;
            switch (component)
            {
                case FieldConstants.MT:
                    derivedValue = !string.IsNullOrWhiteSpace(MT) ? MT : (Value.Length >= 3 ? Value.Substring(0, 3) : MT);
                    break;
                case FieldConstants.DATE:
                    derivedValue = !string.IsNullOrWhiteSpace(DateString) ? DateString : Value.Substring(3, Value.Length - 3);
                    break;
            }

            return component.GetComponentValue(derivedValue);
        }

        //private const string Name = "11S";

        public Field11S() : base(FieldDirectory.FIELD_11S)
        {

        }

        /// <summary>
        /// Value will be MT$DateString
        /// </summary>
        /// <param name="value"></param>
        public Field11S(string value) : base(FieldDirectory.FIELD_11S)
        {
            this.Value = value;
        }

        public string MT { get; set; }



        public string SessionNo { get; set; }
        public string ISN { get; set; }

        private static int _sessionNumber = 1000;
        private static int _isn = 100000;


        public Field11S setMT(string name)
        {
            MT = name.Replace(FieldConstants.MT, string.Empty).Trim().Substring(0, 3);
            return this;
        }

        public Field11S setDate(DateTime dateTime)
        {
            return setDate(this, dateTime);
        }

        public override string GetValue()
        {
            _sessionNumber++;
            if (_sessionNumber == 9999)
                _sessionNumber = 1000;

            _isn++;
            if (_isn == 999999)
                _isn = 100000;

            return string.Format("{0}\r\n{1}\r\n{2}{3}", MT ?? string.Empty, DateString ?? string.Empty, _sessionNumber, _isn);
        }
    }
}

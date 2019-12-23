using System.Collections.Generic;
using System.Globalization;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field32B : FieldWithCurrencyAndAmount
    {
        public Field32B() : base(FieldDirectory.FIELD_32B)
        {
        }

        public Field32B setCurrency(string currency)
        {
            return setCurrency(this, currency);
        }

        public Field32B setAmount(decimal amount)
        {
            return setAmount(this, amount);
        }

    }
}

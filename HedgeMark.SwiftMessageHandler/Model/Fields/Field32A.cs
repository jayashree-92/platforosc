using System;
using System.Collections.Generic;
using System.Globalization;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field32A : FieldWithDateCurrencyAndAmount
    {
        public Field32A() : base(FieldDirectory.FIELD_32A)
        {
        }

        public Field32A setDate(DateTime dateTime)
        {
            return setDate(this, dateTime);
        }

        public Field32A setCurrency(string currency)
        {
            return setCurrency(this, currency);
        }

        public Field32A setAmount(decimal amount)
        {
            return setAmount(this, amount);
        }
    }
}


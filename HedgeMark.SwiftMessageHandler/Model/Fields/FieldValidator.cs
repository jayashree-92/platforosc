using System.Collections.Generic;
using System.IO;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldValidator
    {
        public static void Validate(string value, string pattern, List<string> validCodes = null)
        {
            //Example 4!c

            if (pattern.Contains("!"))
            {
                var validLength = pattern.Split('!')[0].ToInt();
                if (value.Length != validLength)
                    throw new InvalidDataException($"{value} should have a length of {validLength}");
            }
            if (pattern.Contains("!c") && validCodes != null)
            {
                if (!validCodes.Contains(value))
                    throw new InvalidDataException($"{value} should be one among {string.Join(",", validCodes)}");
            }
        }
    }
}

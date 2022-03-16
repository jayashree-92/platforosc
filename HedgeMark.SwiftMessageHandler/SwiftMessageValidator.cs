using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HedgeMark.SwiftMessageHandler.Model;

namespace HedgeMark.SwiftMessageHandler
{
    public class SwiftMessageValidator
    {
        private class FieldElements
        {
            public FieldElements()
            {
                BreakDowns = new Dictionary<string, string>();
            }

            public string Name { get; set; }
            public string Label { get; set; }
            public string Value { get; set; }
            public Dictionary<string, string> BreakDowns { get; private set; }
        }

        private static readonly Dictionary<string, List<string>> ManadatoryFields = new Dictionary<string, List<string>>()
        {
            {"103", new List<string> {"20","23B","32A","50K","59","71A" } },
            {"202", new List<string> {"20","21","32A","58A"} },
            {"202 COV", new List<string> {"20","21","32A","58A"} },

            {"192", new List<string> {"20","21","11S"} },
            {"292", new List<string> {"20","21","11S"} },

            {"210", new List<string> {"20","21","30","32B"} },

            {"900", new List<string> {"20","21","25A","32A"} },
            {"910", new List<string> {"20","21","25A","32A"} },

            {"196", new List<string> {"20","21","76"} },
            {"296", new List<string> {"20","21","76"} },
        };



        public static void Validate(string message)
        {
            //Get field elements
            var fieldElements = GetFieldElements(message, out var messageType);
            var mandatoryFields = ManadatoryFields.ContainsKey(messageType) ? ManadatoryFields[messageType] : new List<string>();
            var builder = new StringBuilder();
            //Check if the mandatory fields has values
            foreach (var fieldElement in fieldElements)
            {
                var thisFieldDetails = string.Empty;

                if (!string.IsNullOrWhiteSpace(fieldElement.Value))
                    continue;

                //No break down availabel equals, no value present
                if (!fieldElement.BreakDowns.Any())
                {
                    builder.AppendFormat("{0}:{1}", fieldElement.Name, fieldElement.Label);
                    continue;
                }

                foreach (var breakDown in fieldElement.BreakDowns)
                {
                    if (!string.IsNullOrWhiteSpace(breakDown.Value))
                        continue;

                    //Check if the field is mandatory 
                    if (!mandatoryFields.Any() && !mandatoryFields.Any(s => s.Equals(fieldElement.Name)))
                        continue;

                    thisFieldDetails += $"{breakDown.Key}:{breakDown.Value}\n";
                }

                if (string.IsNullOrWhiteSpace(thisFieldDetails))
                    continue;

                builder.AppendFormat("{0}:{1}", fieldElement.Name, fieldElement.Label);
                builder.AppendLine(thisFieldDetails);
            }

            var exceptionDetails = builder.ToString();

            if (!string.IsNullOrWhiteSpace(exceptionDetails))
                throw new InvalidDataException($"Missing Mandatory field details: {exceptionDetails}");
        }

        private static IEnumerable<FieldElements> GetFieldElements(string message, out string messageType)
        {
            var swiftMessage = SwiftMessage.Parse(message);
            var fieldElements = new List<FieldElements>();
            messageType = swiftMessage.MessageType;

            //var builder = new StringBuilder();
            foreach (var field in swiftMessage.Block4.GetFields())
            {
                var thisField = new FieldElements()
                {
                    Name = field.Name,
                    Label = field.Label,
                    Value = field.GetValue()
                };

                foreach (var component in field.Components)
                {
                    thisField.BreakDowns.Add(component, field.GetComponentValue(component));
                }

                fieldElements.Add(thisField);
            }

            return fieldElements;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HedgeMark.SwiftMessageHandler.Model.Fields;

namespace HedgeMark.SwiftMessageHandler.Utils
{
    public class FieldInstantiator
    {
        public static List<Field> InstantiateAndGetFields(List<KeyValuePair<string, string>> listOfFields)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var fields = new List<Field>();
            foreach (var fldMap in listOfFields)
            {
                if (string.IsNullOrWhiteSpace(fldMap.Key))
                    continue;

                var type = assembly.GetTypes().FirstOrDefault(t => t.Name == string.Format("Field{0}", fldMap.Key));

                Field field;
                if (type != null)
                {
                    field = (Field) Activator.CreateInstance(type);
                }
                else
                {
                    type = assembly.GetTypes().First(t => t.Name == "Field");
                    field = (Field) Activator.CreateInstance(type, fldMap.Key);
                }

                field.SetValue(fldMap.Value);
                fields.Add(field);
            }

            return fields;
        }

    }
}

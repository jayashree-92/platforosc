using System.Collections.Generic;
using System.Linq;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT;

namespace HedgeMark.SwiftMessageHandler.Model.Blocks
{
    public class SwiftBlock3 : SwiftBlockWithFields
    {
        private static readonly List<string> MTsWhereF212IsMandatory = new List<string>()
        {
            MTDirectory.MT_103,MTDirectory.MT_103STP,MTDirectory.MT_103REMIT,MTDirectory.MT_202,MTDirectory.MT_202COV, MTDirectory.MT_205,MTDirectory.MT_205COV
        };
        public SwiftBlock3() : base("3", "User Header")
        {
        }

        public SwiftBlock3(string messageType) : base("3", "User Header")
        {
            MessageType = messageType;
            
            if (MessageType.Contains("COV"))
                Fields.Add(new Field119().setValidationFlag("COV"));

            if (MTsWhereF212IsMandatory.Contains(messageType))
                Fields.Add(new Field121());
        }

        public new void AddField(Field field)
        {
            if (field.Name == FieldDirectory.FIELD_111 && Fields.All(s => s.Name != FieldDirectory.FIELD_121))
                AddField(new Field121());

            SetField(field);
        }

    }
}

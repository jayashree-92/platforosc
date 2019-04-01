using System.Collections.Generic;
using System.Linq;
using System.Text;
using HedgeMark.SwiftMessageHandler.Model.Fields;

namespace HedgeMark.SwiftMessageHandler.Model.Blocks
{
    public class SwiftBlockWithFields : SwiftBlock
    {
        public SwiftBlockWithFields(string name, string label) : base(name, label)
        {
            Fields = new List<Field>();
        }

        public string MessageType { get; set; }

        internal List<Field> Fields { get; set; }

        public void SetField(Field field)
        {
            var thisField = Fields.FirstOrDefault(s => s.Name == field.Name);

            if (thisField!=null)
                thisField.Value = field.Value;
            else
                Fields.Add(field); ;
        }

        public void AddField(Field field)
        {
            //if (field.Name == FieldDirectory.FIELD_111 && Fields.All(s => s.Name != FieldDirectory.FIELD_121))
            //    AddField(new Field121());

            //if (Fields.ContainsKey(field.Name))
            //    Fields[field.Name] = field;
            //else
            Fields.Add(field); ;
        }

        public void RemoveField(Field field)
        {
            if (HasField(field.Name))
                Fields.Remove(Fields.First(s => s.Name == field.Name));
        }
        public List<Field> GetFields()
        {
            return Fields;
        }

        public bool HasField(string fieldName)
        {
            return Fields.Any(s => s.Name == fieldName);
        }

        public string GetFieldValue(string fieldName)
        {
            return HasField(fieldName) ? Fields.First(s => s.Name == fieldName).Value : string.Empty;
        }

        public Field GetField(string fieldName)
        {
            return HasField(fieldName) ? Fields.First(s => s.Name == fieldName) : null;
        }

        public override string GetBlock()
        {
            var thisBlock = new StringBuilder();

            if (Fields.Count == 0)
                return string.Empty;

            Fields.ForEach(fld =>
              {
                  thisBlock.AppendFormat("{{{0}}}", fld.GetFieldAndValue);
              });

            return string.Format("{{{0}:{1}}}", Name, thisBlock);
        }

        public void SetBlock(SwiftBlockWithFields block)
        {
            this.Fields = block.Fields;
        }
    }
}

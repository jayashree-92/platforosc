using System;
using System.Linq;
using System.Text;

namespace HedgeMark.SwiftMessageHandler.Model.Blocks
{
    public class SwiftBlock4 : SwiftBlockWithFields
    {
        public SwiftBlock4() : base("4", "Message Block")
        {
        }

        public override string GetBlock()
        {
            if (Fields.Count == 0)
                return string.Empty;

            var thisBlock = new StringBuilder();

            Fields.ToList().ForEach(fld =>
            {
                thisBlock.Append("\r\n:");
                thisBlock.Append(fld.GetFieldAndValue);
            });

            return string.Format("{{{0}:{1}{2}-}}", Name, thisBlock, Environment.NewLine);
        }
    }
}

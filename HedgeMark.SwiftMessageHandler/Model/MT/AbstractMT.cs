using System.Security.Policy;
using HedgeMark.SwiftMessageHandler.Model.Fields;

namespace HedgeMark.SwiftMessageHandler.Model.MT
{
    public abstract class AbstractMT : SwiftMessage
    {
        protected AbstractMT(string messageType) : base(messageType)
        {

        }

        public AbstractMT parse(string finMessage)
        {
            SetMessage(Parse(finMessage));
            return this;
        }

        public AbstractMT setSenderAndReceiver(string sender, string receiver)
        {
            setSender(sender);
            setReceiver(receiver);
            return this;
        }

        public AbstractMT setSender(string sender)
        {
            Block1.SetSender(sender);
            return this;
        }

        public AbstractMT setReceiver(string receiver)
        {
            Block2.SetReceiver(receiver);
            return this;
        }

        public AbstractMT addField(Field field)
        {
            if (FieldDirectory.IsBlock3Field(field))
                Block3.AddField(field);
            else
                Block4.AddField(field);

            return this;
        }

        public AbstractMT removeField(string fieldName)
        {
            if (FieldDirectory.IsBlock3Field(fieldName))
                Block3.RemoveField(fieldName);
            else
                Block4.RemoveField(fieldName);

            return this;
        }

        public AbstractMT updateFieldValue(string fieldName, string fieldValue)
        {
            if (FieldDirectory.IsBlock3Field(fieldName))
                Block3.UpdateField(fieldName, fieldValue);
            else
                Block4.UpdateField(fieldName, fieldValue);

            return this;
        }
    }
}

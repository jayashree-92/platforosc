using System.Linq;
using System.Text;
using HedgeMark.SwiftMessageHandler.Model.Blocks;
using HedgeMark.SwiftMessageHandler.Model.Fields;

namespace HedgeMark.SwiftMessageHandler.Model
{
    public class SwiftMessage
    {
        public SwiftBlock1 Block1 { get; private set; }

        public SwiftBlock2 Block2 { get; private set; }

        public SwiftBlock3 Block3 { get; private set; }

        public SwiftBlock4 Block4 { get; private set; }

        public SwiftBlock5 Block5 { get; private set; }

        public SwiftBlockUser BlockUser { get; private set; }

        public string MessageType { get; protected set; }
        public string UnderlyingOriginalFINMessage { get; set; }
        public SwiftMessage UnderlyingOriginalSwiftMessage { get; set; }

        public string GetMessageType()
        {
            return MessageType;
        }

        public SwiftMessage(string messageType)
        {
            MessageType = messageType;

            Block1 = new SwiftBlock1();
            Block2 = new SwiftBlock2(MessageType);
            Block3 = new SwiftBlock3(MessageType);
            Block4 = new SwiftBlock4();
            Block5 = new SwiftBlock5();
        }

        public void AddBlock(SwiftBlock block)
        {
            if (block.Name == "1")
                Block1 = (SwiftBlock1)block;
            else if (block.Name == "2")
                Block2 = (SwiftBlock2)block;
            else if (block.Name == "3")
                Block3 = (SwiftBlock3)block;
            else if (block.Name == "4")
                Block4 = (SwiftBlock4)block;
            else if (block.Name == "5")
                Block5 = (SwiftBlock5)block;
            else
                BlockUser = (SwiftBlockUser)block;
        }

        public void SetMessage(SwiftMessage message)
        {
            var originalMessage = message.IsServiceMessage21() ? message.UnderlyingOriginalSwiftMessage : message;

            if (originalMessage.Block1 != null)
                this.Block1.SetBlock(originalMessage.Block1);
            if (originalMessage.Block2 != null)
                this.Block2.SetBlock(originalMessage.Block2);
            if (originalMessage.Block3 != null)
                this.Block3.SetBlock(originalMessage.Block3);
            if (originalMessage.Block4 != null)
                this.Block4.SetBlock(originalMessage.Block4);
            if (originalMessage.Block5 != null)
                this.Block5.SetBlock(originalMessage.Block5);
            if (originalMessage.BlockUser != null)
            {
                this.BlockUser = new SwiftBlockUser(originalMessage.BlockUser.Name);
                BlockUser.SetBlock(originalMessage.BlockUser);
            }
        }

        public string GetSender()
        {
            return IsIncoming() ? Block2.GetReceiver() : Block1.GetLogicalTerminal();
        }

        public string GetReceiver()
        {
            return IsIncoming() ? Block1.GetLogicalTerminal() : Block2.GetReceiver();
        }

        public bool IsType(string messageType)
        {
            return Block2.MessageType.Equals(messageType);
        }

        public string GetMTType()
        {
            return Block2.MessageType;
        }

        public string GetFullMessageType()
        {
            return string.Format("MT{0}{1}", Block2.MessageType, Block3.Fields.Any(s => s.Name == FieldDirectory.FIELD_119) ? string.Format(" {0}", Block3.GetFieldValue(FieldDirectory.FIELD_119)) : string.Empty).Trim();
        }

        public bool IsServiceMessage21()
        {
            return Block1.ServiceId == "21";
        }

        public bool IsAck()
        {
            return Block4.HasField(FieldDirectory.FIELD_451) && Block4.GetFieldValue(FieldDirectory.FIELD_451) == "0";
        }

        public bool IsNack()
        {
            return Block4.HasField(FieldDirectory.FIELD_451) && Block4.GetFieldValue(FieldDirectory.FIELD_451) == "1";
        }

        public bool IsIncoming()
        {
            return Block2.InputOrOutputId == "O";
        }

        public bool IsOutgoing()
        {
            return Block2.InputOrOutputId == "I";
        }

        public string GetNackReasonCode()
        {
            return Block4.GetFieldValue(FieldDirectory.FIELD_405);
        }

        /// <summary>
        /// Message User Reference
        /// </summary>
        /// <returns></returns>
        public string GetMUR()
        {
            return Block3.GetFieldValue(FieldDirectory.FIELD_108);
        }


        public string GetFieldValue(string fieldName)
        {
            if (!IsServiceMessage21())
                return FieldDirectory.IsBlock3Field(fieldName) ? Block3.GetFieldValue(fieldName) : Block4.GetFieldValue(fieldName);

            var fieldValue = FieldDirectory.IsBlock3Field(fieldName) ? UnderlyingOriginalSwiftMessage.Block3.GetFieldValue(fieldName) : UnderlyingOriginalSwiftMessage.Block4.GetFieldValue(fieldName);

            if (!string.IsNullOrWhiteSpace(fieldValue))
                return fieldValue;

            return FieldDirectory.IsBlock3Field(fieldName) ? Block3.GetFieldValue(fieldName) : Block4.GetFieldValue(fieldName);
        }

        public Field GetField(string fieldName)
        {
            return FieldDirectory.IsBlock3Field(fieldName) ? Block3.GetField(fieldName) : Block4.GetField(fieldName);
        }

        public string GetMessage()
        {
            var swiftMsg = new StringBuilder();

            if (Block1 != null)
                swiftMsg.Append(Block1.GetBlock());
            if (Block2 != null)
                swiftMsg.Append(Block2.GetBlock(MessageType));
            if (Block3 != null)
                swiftMsg.Append(Block3.GetBlock());
            if (Block4 != null)
                swiftMsg.Append(Block4.GetBlock());
            if (Block5 != null)
                swiftMsg.Append(Block5.GetBlock());
            if (BlockUser != null)
                swiftMsg.Append(BlockUser.GetBlock());

            return swiftMsg.ToString();
        }
        public static SwiftMessage Parse(string finMessage)
        {
            if (string.IsNullOrWhiteSpace(finMessage))
                return null;

            return SwiftMessageParser.Parse(finMessage);
        }
    }
}

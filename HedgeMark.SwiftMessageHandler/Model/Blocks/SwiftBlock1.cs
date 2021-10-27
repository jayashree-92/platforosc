using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Blocks
{
    public class LogicalTerminalAddress
    {
        public string BIC { get; set; }
        public string TerminalCode { get; set; }
        public string BICBranchCode { get; set; }

        public string GetLTAddress => $"{BIC}{TerminalCode.Substring(0, 1).ToUpper()}{BICBranchCode}";
    }

    public class SwiftBlock1 : SwiftBlock
    {
        private static int _sessionNumber = 1000;
        private static int _sequenceNumber = 100000;

        public SwiftBlock1() : base("1", "Basic Header")
        {
            AppId = "F";
            ServiceId = "01";

            LTAddress = new LogicalTerminalAddress()
            {
                BIC = "TESTUS00",
                TerminalCode = "X",
                BICBranchCode = "XXX"
            };

            _sessionNumber++;
            if (_sessionNumber == 9999)
                _sessionNumber = 1000;

            _sequenceNumber++;
            if (_sequenceNumber == 999999)
                _sequenceNumber = 100000;
        }

        /// <summary>
        /// The Application Identifier identifies the application within which the message is being sent or received. 
        /// The available options are: 
        ///     F = FIN All user-to-user, FIN system and FIN service messages 
        ///     A = GPA (General Purpose Application) Most GPA system and service messages 
        ///     L = GPA Certain GPA service messages, for example, LOGIN, LAKs, ABORT These values are automatically assigned by the SWIFT system and the user's CBT
        /// By default its "F"
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// he Service Identifier consists of two numeric characters. 
        /// It identifies the type of data that is being sent or received and, in doing so, whether the message which follows is one of the following: a user-to-user message, a system message, a service message, 
        /// for example, a session control command, such as SELECT, or a logical acknowledgment, such as ACK/SAK/UAK. 
        /// Possible values are 
        ///     01 = FIN/GPA
        ///     21 = ACK/NAK.
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// The Logical Termial (LT) Address is a 12-character FIN address. It is the address of the sending LT for input messages or of the receiving LT for output messages, and includes the Branch Code. 
        /// It consists of: 
        ///     - the BIC 8 CODE (8 characters) 
        ///     - the Logical Terminal Code (1 upper case alphabetic character) 
        ///     - the BIC Branch Code (3 characters)
        /// </summary>
        public LogicalTerminalAddress LTAddress { get; set; }
        public string SessionNo { private get; set; }
        public string SequenceNo { private get; set; }

        public string GetSessionNo()
        {
            return SessionNo ?? _sessionNumber.ToString();
        }

        public string GetSequenceNo()
        {
            return SequenceNo ?? _sequenceNumber.ToString();
        }

        public void SetSender(string sender)
        {
            LTAddress = new LogicalTerminalAddress()
            {
                BIC = sender.Length >= 8 ? sender.Substring(0, 8) : "TESTUS00",
                TerminalCode = sender.Length >= 9 ? sender.Substring(8, 1) : "X",
                BICBranchCode = sender.Length >= 12 ? sender.Substring(9, 3) : "XXX",
            };
        }

        internal string GetLogicalTerminal()
        {
            return LTAddress.GetLTAddress;
        }

        public override string GetBlock()
        {
            return
                $"{{{Name}:{AppId}{ServiceId}{LTAddress.GetLTAddress}{SessionNo ?? _sessionNumber.ToString()}{SequenceNo ?? _sequenceNumber.ToString()}}}";
        }

        public void SetBlock(SwiftBlock1 block1)
        {
            this.AppId = block1.AppId;
            this.ServiceId = block1.ServiceId;
            //this.LTAddress  =new LogicalTerminalAddress()
            //{
            //   BIC = block1.LTAddress.BIC,
            //    TerminalCode = block1.LTAddress.TerminalCode,
            //    BICBranchCode = block1.LTAddress.BICBranchCode
            //};

            this.LTAddress = block1.LTAddress.DeepCopy();
            this.SequenceNo = block1.SequenceNo;
            this.SessionNo = block1.SessionNo;
        }
    }
}

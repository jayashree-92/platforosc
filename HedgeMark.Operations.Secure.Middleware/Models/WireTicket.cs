using HedgeMark.Operations.Secure.DataModel;
using System.Collections.Generic;
using HMOSecureMiddleware.SwiftMessageManager;
using System.Linq;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.SwiftMessageHandler;
using HedgeMark.SwiftMessageHandler.Model;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT;

namespace HMOSecureMiddleware.Models
{
    public class WireTicket
    {
        public long WireId
        {
            get
            {
                return HMWire == null ? 0 : HMWire.hmsWireId;
            }
        }
        public hmsWire HMWire { get; set; }
        public string WireCreatedBy { get; set; }
        public string WireLastUpdatedBy { get; set; }
        public List<string> AttachmentUsers { get; set; }
        public List<string> WorkflowUsers { get; set; }
        public dmaAgreementOnBoarding Agreement { get; set; }
        public onBoardingAccount Account { get; set; }
        public onBoardingAccount ReceivingAccount { get; set; }
        public onBoardingSSITemplate SSITemplate { get; set; }
        public string FundName { get; set; }
        public string ReceivingAccountName
        {
            get
            {
                return HMWire.IsBookTransfer ? ReceivingAccount.AccountName : SSITemplate.TemplateName;
            }
        }
        public string ReceivingAccountCurrency
        {
            get
            {
                return HMWire.IsBookTransfer ? ReceivingAccount.Currency : SSITemplate.Currency;
            }
        }
        public Dictionary<string, string> SwiftMessages { get; set; }
    }

    public class WireInBoundMessage
    {
        public WireInBoundMessage(string swiftMessage)
        {
            FinMessage = swiftMessage;
            SwiftMessage = SwiftMessage.Parse(swiftMessage);
            MessageType = SwiftMessage.MessageType;
            IsAckOrNack = SwiftMessage.IsAck() || SwiftMessage.IsNack();

            WireId = IsAckOrNack 
                ? SwiftMessage.UnderlyingOriginalSwiftMessage.GetFieldValue(FieldDirectory.FIELD_20).Replace(string.Format("{0}DMOTRN", Utility.Environment.ToUpper()), string.Empty).ToInt() 
                : SwiftMessage.GetFieldValue(FieldDirectory.FIELD_20).Replace(string.Format("{0}DMOTRN", Utility.Environment.ToUpper()), string.Empty).ToInt();
        }

        public bool IsAckOrNack { get; private set; }
        public string FinMessage { get; private set; }
        public SwiftMessage SwiftMessage { get; private set; }
        public long WireId { get; private set; }
        public string MessageType { get; private set; }
        public bool IsConfirmed { get; set; }
        public bool IsAcknowledged { get; set; }
        public bool IsNegativeAcknowledged { get; set; }
        public string ExceptionMessage { get; set; }
        public string ConfirmationMessage { get; set; }
    }
}

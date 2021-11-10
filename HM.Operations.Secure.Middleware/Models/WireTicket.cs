using System;
using System.Collections.Generic;
using System.Linq;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.SwiftMessageHandler.Model;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware.Util;

namespace HM.Operations.Secure.Middleware.Models
{
    public class WireTicket
    {
        public long WireId => HMWire?.hmsWireId ?? 0;

        public bool IsFundTransfer => HMWire.WireTransferTypeId == 2;

        public bool Is3rdPartyTransfer => HMWire.WireTransferTypeId == 1;

        public bool IsNotice => HMWire.hmsWireTransferTypeLKup != null && HMWire.hmsWireTransferTypeLKup.TransferType == "Notice";

        public string TransferType => HMWire.hmsWireTransferTypeLKup == null ? "3rd Party Transfer" : HMWire.hmsWireTransferTypeLKup.TransferType;

        public bool IsSenderInformationRequired
        {
            get
            {
                var senderInvolvedMessageTypes = new List<string> { "MT103", "MT202" };
                return HMWire.hmsWireMessageType != null && senderInvolvedMessageTypes.Contains(HMWire.hmsWireMessageType.MessageType);
            }
        }

        private string _counterparty;
        public string Counterparty
        {
            get
            {
                switch (TransferType)
                {
                    case "3rd Party Transfer": return _counterparty;
                    case "Fund Transfer": return "BNY";
                    case "Fee/Expense Payment": return SSITemplate.ServiceProvider;
                    default: return _counterparty;
                }
            }
            set => _counterparty = value;
        }


        public string SendingAccountNumber
        {
            get
            {
                if (WireId == 0)
                    return string.Empty;

                return !string.IsNullOrEmpty(SendingAccount.FFCNumber) ? SendingAccount.FFCNumber : SendingAccount.UltimateBeneficiaryAccountNumber;
            }
        }

        //public string BeneficiaryBank
        //{
        //    get
        //    {
        //        if (WireId == 0)
        //            return string.Empty;

        //        if (IsFundTransfer)
        //            return ReceivingAccount.UltimateBeneficiaryType == "Account Name" ? ReceivingAccount.Beneficiary.BankName : ReceivingAccount.UltimateBeneficiary.BankName;

        //        if (!IsNotice)
        //            return SSITemplate.UltimateBeneficiaryType == "Account Name" ? SSITemplate.Beneficiary.BankName : SSITemplate.UltimateBeneficiary.BankName;

        //        return "N/A";
        //    }
        //}

        public string UltimateBeneficiary
        {
            get
            {
                if (WireId == 0)
                    return string.Empty;

                if (IsFundTransfer)
                    return ReceivingAccount.AccountName;

                //if (!IsNotice)
                //    return SSITemplate.UltimateBeneficiaryType == "Account Name" ? SSITemplate.UltimateBeneficiaryAccountName : SSITemplate.UltimateBeneficiary.BICorABA;

                if (!IsNotice)
                    return !string.IsNullOrEmpty(SSITemplate.FFCName) ? SSITemplate.FFCName : SSITemplate.UltimateBeneficiaryAccountNumber;

                return "N/A";
            }
        }


        public string UltimateBeneficiaryAccountNumber
        {
            get
            {
                if (WireId == 0)
                    return string.Empty;

                if (IsFundTransfer)
                    return !string.IsNullOrEmpty(ReceivingAccount.FFCNumber) ? ReceivingAccount.FFCNumber : ReceivingAccount.UltimateBeneficiaryAccountNumber;

                if (!IsNotice)
                    return !string.IsNullOrEmpty(SSITemplate.FFCNumber) ? SSITemplate.FFCNumber : SSITemplate.UltimateBeneficiaryAccountNumber;

                return "N/A";
            }
        }

        public string ReceivingAccountName
        {
            get
            {
                if (WireId == 0)
                    return string.Empty;

                var accName = IsFundTransfer ? ReceivingAccount.AccountName : SSITemplate.TemplateName;
                return string.IsNullOrWhiteSpace(accName) ? "N/A" : accName;
            }
        }

        public string ReceivingAccountCurrency => IsNotice ? SendingAccount.Currency : IsFundTransfer ? ReceivingAccount.Currency : SSITemplate.Currency;

        public bool ShouldIncludeWirePurpose => (SendingAccount.SwiftGroup ?? new hmsSwiftGroup()).SwiftGroup == "Credit Suisse";

        private hmsWire _hmWire;
        public hmsWire HMWire
        {
            get => _hmWire ?? new hmsWire();
            set => _hmWire = value;
        }

        public string WireCreatedBy { get; set; }
        public string WireLastUpdatedBy { get; set; }
        public string WireApprovedBy { get; set; }
        public List<string> AttachmentUsers { get; set; }
        public List<string> WorkflowUsers { get; set; }
        public onBoardingAccount SendingAccount { get; set; }
        public onBoardingAccount ReceivingAccount { get; set; }
        public onBoardingSSITemplate SSITemplate { get; set; }
        public string PreferredFundName { get; set; }
        public string ShortFundName { get; set; }
        public string ClientLegalName { get; set; }
        public string AdminName { get; set; }
        public string CustodianName { get; set; }
        public string FundRegisterAddress { get; set; }

        //Populated from  hmsBankAccountAddress
        public string ReceivingSsiUltimateBeneAccountAddress => !string.IsNullOrWhiteSpace(SSITemplate.UltimateBeneficiaryType) && SSITemplate.UltimateBeneficiaryType == "Account Name" ? SSITemplate.UltimateBeneficiary.BankAddress : string.Empty;

        public List<FormattedSwiftMessage> SwiftMessages { get; set; }

        public hmsWireMessageType CancellationWireMessageType { get; set; }

        // Default Properties
        public string CollateralPaymentReason => "Collateral Payment";

        public string DefaultSenderInformation => HMWire.hmsWireSenderInformation == null ? "BNF" : HMWire.hmsWireSenderInformation.SenderInformation;

        public TimeSpan Deadline { get; set; }
    }

    public class WireInBoundMessage
    {
        private const string FEACK = "FEACK";
        private static readonly List<string> MessageWithHMTransRefInField21 = new List<string>() { MTDirectory.MT_196, MTDirectory.MT_296, MTDirectory.MT_900 };
        public WireInBoundMessage Parse(string swiftMessage)
        {
            OriginalFinMessage = swiftMessage;
            IsFeAck = swiftMessage.Trim().EndsWith(FEACK);

            if (IsFeAck)
                OriginalFinMessage = OriginalFinMessage.Replace(FEACK, string.Empty);

            SwiftMessage = SwiftMessage.Parse(OriginalFinMessage);
            MessageType = SwiftMessage.MessageType;
            IsAckOrNack = SwiftMessage.IsAck() || SwiftMessage.IsNack();

            //There is couple of scenarios here
            //For Ack/Nack and FEACK we should look for field 20:
            //Please note here, Field 21 is the related reference number of corresponding transaction

            var referenceTag = string.Empty;

            //Should use the original message
            if (IsFeAck)
                WireId = GetWireIdFromTransaction(SwiftMessage.GetFieldValue(FieldDirectory.FIELD_20), out referenceTag);

            //Should use the underlying message
            else if (IsAckOrNack)
                WireId = GetWireIdFromTransaction(SwiftMessage.UnderlyingOriginalSwiftMessage.GetFieldValue(FieldDirectory.FIELD_20), out referenceTag);

            //For inbound message type - 196,296 and 900 - we will receive
            else if (MessageWithHMTransRefInField21.Any(s => s.Equals(SwiftMessage.GetMTType())))
                WireId = GetWireIdFromTransaction(SwiftMessage.GetFieldValue(FieldDirectory.FIELD_21), out referenceTag);


            //Special Handling for MT 910 as We will not be able to derive using field 21.
            //As of now we are using field 32A - value date , currency and amount in MT 910            
            //:32A: 180830USD367574,90
            else if (SwiftMessage.IsType(MTDirectory.MT_910))
            {
                string currency;
                decimal amount;
                DateTime valueDate;

                var is32APresent = SwiftMessage.HasField(FieldDirectory.FIELD_32A);

                if (is32APresent)
                {
                    //get wire Id for the given combination
                    var field32A = (Field32A)SwiftMessage.GetField(FieldDirectory.FIELD_32A);
                    valueDate = field32A.GetComponentValue(FieldConstants.DATE).ToDateTime("MMM dd, yyyy");
                    currency = field32A.GetComponentValue(FieldConstants.CURRENCY);
                    amount = field32A.GetComponentValue(FieldConstants.AMOUNT).ToDecimal();
                }
                else
                {
                    var field32B = (Field32B)SwiftMessage.GetField(FieldDirectory.FIELD_32B);
                    currency = field32B.GetComponentValue(FieldConstants.CURRENCY);
                    amount = field32B.GetComponentValue(FieldConstants.AMOUNT).ToDecimal();

                    var field30 = (Field30)SwiftMessage.GetField(FieldDirectory.FIELD_30);
                    valueDate = field30.DateString.ToDateTime("yyMMdd");
                }

                using (var context = new OperationsSecureContext())
                {
                    var allPendingWires = context.hmsWires.Where(s => s.WireStatusId == 3 && s.SwiftStatusId != 5 && s.ValueDate == valueDate.Date).ToList();
                    var wire = allPendingWires.FirstOrDefault(s => s.Currency == currency && s.Amount == amount);

                    if (wire != null)
                        WireId = wire.hmsWireId;
                }
            }


            ReferenceTag = referenceTag;

            return this;
        }


        private long GetWireIdFromTransaction(string fieldValue, out string referenceTag)
        {
            var wireIdString = fieldValue.Replace("DMO", string.Empty).Replace(Utility.Environment.ToUpper()[0].ToString(), string.Empty);
            var referenceStrSplits = wireIdString.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            referenceTag = string.Empty;

            if (wireIdString.Contains("/") && referenceStrSplits.Length > 1)
            {
                referenceTag = referenceStrSplits[1];
            }

            return referenceStrSplits[0].ToLong();
        }

        public bool IsFeAck { get; private set; }
        public bool IsAckOrNack { get; private set; }
        public string OriginalFinMessage { get; private set; }
        public SwiftMessage SwiftMessage { get; private set; }
        public long WireId { get; private set; }
        public string ReferenceTag { get; private set; }
        public string ValueDateCurrencyAndAmount { get; private set; }
        public string MessageType { get; private set; }
        public bool IsConfirmed { get; set; }
        public bool IsAcknowledged { get; set; }
        public bool IsNegativeAcknowledged { get; set; }
        public string ExceptionMessage { get; set; }
        public string ConfirmationMessage { get; set; }
    }
}

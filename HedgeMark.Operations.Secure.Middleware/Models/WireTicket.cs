﻿using System;
using HedgeMark.Operations.Secure.DataModel;
using System.Collections.Generic;
using System.Linq;
using Com.HedgeMark.Commons.Extensions;
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

        public bool IsBookTransfer
        {
            get { return HMWire.WireTransferTypeId == 2; }
        }

        public string TransferType
        {
            get { return HMWire.hmsWireTransferTypeLKup.TransferType; }
        }

        public hmsWire HMWire { get; set; }
        public string WireCreatedBy { get; set; }
        public string WireLastUpdatedBy { get; set; }
        public List<string> AttachmentUsers { get; set; }
        public List<string> WorkflowUsers { get; set; }
        public dmaAgreementOnBoarding Agreement { get; set; }
        public onBoardingAccount SendingAccount { get; set; }
        public onBoardingAccount ReceivingAccount { get; set; }
        public onBoardingSSITemplate SSITemplate { get; set; }
        public string FundName { get; set; }
        public string ReceivingAccountName
        {
            get
            {
                return IsBookTransfer ? ReceivingAccount.AccountName : SSITemplate.TemplateName;
            }
        }
        public string ReceivingAccountCurrency
        {
            get
            {
                return IsBookTransfer ? ReceivingAccount.Currency : SSITemplate.Currency;
            }
        }
        public List<KeyValuePair<string, string>> SwiftMessages { get; set; }

        public hmsWireMessageType CancellationWireMessageType { get; set; }
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

                //var is32APresent = SwiftMessage.HasField(FieldDirectory.FIELD_32A);

                //if (is32APresent)
                //{
                //    //get wire Id for the given combination
                //    var field32A = (Field32A)SwiftMessage.GetField(FieldDirectory.FIELD_32A);
                //    valueDate = field32A.GetComponentValue(FieldConstants.DATE).ToDateTime("MMM dd, yyyy");
                //    currency = field32A.GetComponentValue(FieldConstants.CURRENCY);
                //    amount = field32A.GetComponentValue(FieldConstants.AMOUNT).ToDecimal();
                //}

                var field32B = (Field32B)SwiftMessage.GetField(FieldDirectory.FIELD_32B);
                var currency = field32B.GetComponentValue(FieldConstants.CURRENCY);
                var amount = field32B.GetComponentValue(FieldConstants.AMOUNT).ToDecimal();

                var field30 = (Field30)SwiftMessage.GetField(FieldDirectory.FIELD_30);
                var valueDate = field30.DateString.ToDateTime("yyMMdd");


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
            var wireIdString = fieldValue.Replace(string.Format("{0}DMO", Utility.Environment.ToUpper()[0].ToString()), string.Empty);
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

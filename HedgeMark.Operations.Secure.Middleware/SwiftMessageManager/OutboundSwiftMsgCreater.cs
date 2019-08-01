using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text;
using HMOSecureMiddleware.Models;
using Com.HedgeMark.Commons;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT;
using HedgeMark.SwiftMessageHandler.Model.MT.MT1XX;
using HedgeMark.SwiftMessageHandler.Model.MT.MT2XX;
using HedgeMark.SwiftMessageHandler.Model.MT.MT5XX;
using System.Linq;

namespace HMOSecureMiddleware.SwiftMessageManager
{
    public class OutboundSwiftMsgCreator
    {
        protected static readonly string HMBIC = ConfigurationManagerWrapper.StringSetting("HMBIC", "HMRKUS30");
        protected static readonly string HMBICSender = string.Format("{0}{1}", HMBIC, ConfigurationManagerWrapper.StringSetting("HMBICSender", "XXXX"));
        public static AbstractMT CreateMessage(WireTicket wire, string messageType, string originalMessageType, string referenceTag = "")
        {
            switch (messageType)
            {
                //MT103 - Single customer credit transfer
                case "MT103":
                    return CreateMt103(wire, messageType, referenceTag);
                //MT202 - General Financial inst Transfer
                case "MT202":
                    return CreateMt202(wire, messageType);
                //MT202 COV - General Financial inst Transfer
                case "MT202 COV":
                case "MT202COV":
                    return CreateMt202Cov(wire);
                //MT210 - Notice to Receive
                case "MT210":
                    return CreateMt210(wire);
                //MT192 - request for cancellation
                case "MT192":
                    return CreateMt192(wire, originalMessageType);
                //MT292 - Request for Cancellation
                case "MT292":
                    return CreateMt292(wire, originalMessageType);
                //MT540 - Receive Free-- > To cancel, a new 540 with function code of CANC must be used.
                case "MT540":
                    return CreateMt540(wire);
                //MT542 - Deliver Free-- > To cancel, a new 542 with function code of CANC must be used.
                case "MT542":
                    return CreateMt542(wire);
            }

            throw new InvalidDataException(string.Format("Wire Message type {0} not handled", messageType));
        }

        private static Field11S GetField11S(WireTicket wire)
        {
            var f11S = new Field11S()
                .setDate(wire.HMWire.ValueDate)
                .setMT(wire.HMWire.hmsWireMessageType.MessageType.Replace(" ", string.Empty).Replace("COV", string.Empty));
            return f11S;
        }

        private static Field20 GetField20(WireTicket wire, string referenceTag = "")
        {
            var transactionId = WireDataManager.GetWireTransactionId(wire.WireId);
            return new Field20(string.Format("{0}{1}{2}", transactionId, string.IsNullOrWhiteSpace(referenceTag) ? string.Empty : "/", referenceTag));
        }

        private static Field21 GetField21ForCancellation(WireTicket wire)
        {
            var transactionId = WireDataManager.GetWireTransactionId(wire.WireId);
            return new Field21().setReference(transactionId);
        }

        private static Field21 GetField21(WireTicket wire)
        {
            return new Field21().setReference("NONREF");
        }

        private static Field23B GetField23B()
        {
            return new Field23B("CRED");
        }

        private static Field25 GetField25(WireTicket wire)
        {
            return new Field25().setAccount(!string.IsNullOrWhiteSpace(wire.SendingAccount.FFCNumber) ? wire.SendingAccount.FFCNumber : wire.SendingAccount.AccountNumber);
        }

        private static Field30 GetField30(WireTicket wire)
        {
            var f30 = new Field30().setDate(wire.HMWire.ValueDate);
            return f30;
        }

        private static Field32A GetField32A(WireTicket wire)
        {
            var f32A = new Field32A()
                .setDate(wire.HMWire.ValueDate)
                .setCurrency(wire.IsBookTransfer ? wire.ReceivingAccount.Currency : wire.SSITemplate.Currency)
                .setAmount(wire.HMWire.Amount);
            return f32A;
        }

        private static Field32B GetField32B(WireTicket wire)
        {
            var f32B = new Field32B()
                .setAmount(wire.HMWire.Amount)
                .setCurrency(wire.SendingAccount.Currency);
            return f32B;
        }

        /// <summary>
        /// Ordering Customer
        /// </summary>
        /// <param name="wire"></param>
        /// <returns></returns>
        private static Field50K GetField50K(WireTicket wire)
        {
            var isFFCAvailable = !string.IsNullOrWhiteSpace(wire.SendingAccount.FFCNumber);

            var f50K = new Field50K().setAccount(isFFCAvailable ? wire.SendingAccount.FFCNumber : wire.SendingAccount.AccountNumber)
                .setNameAndAddressLine1(isFFCAvailable ? wire.SendingAccount.UltimateBeneficiaryAccountName : wire.SendingAccount.UltimateBeneficiaryBankName)
                .setNameAndAddressLine2(wire.SendingAccount.UltimateBeneficiaryBankAddress);

            return f50K;
        }


        private static void SetField52X(AbstractMT mtMessage, WireTicket wire)
        {
            if (wire.SendingAccount.UltimateBeneficiaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.SendingAccount.UltimateBeneficiaryBICorABA))
                mtMessage.addField(GetField52A(wire));
            else
                mtMessage.addField(GetField52D(wire));
        }


        /// <summary>
        /// "Ordering Customer"
        /// </summary>
        /// <param name="wire"></param>
        /// <returns></returns>
        //private static Field50A GetField50A(WireTicket wire)
        //{
        //    var f50A = new Field50A()
        //        .setAccount(wire.)
        //        .setBIC(HMBIC);
        //    return f50A;
        //}

        private static Field52A GetField52A(WireTicket wire)
        {
            var f52A = new Field52A()
                .setAccount(wire.SendingAccount.AccountNumber)
                .setBIC(wire.SendingAccount.UltimateBeneficiaryType == "ABA" ? string.Empty : wire.SendingAccount.UltimateBeneficiaryBICorABA);
            return f52A;
        }


        private static Field52D GetField52D(WireTicket wire)
        {
            var f52D = new Field52D()
                .setAccount(wire.SendingAccount.AccountNumber)
                .setNameAndAddressLine1(wire.SendingAccount.UltimateBeneficiaryAccountName);
            return f52D;
        }


        /// <summary>
        /// Sender's Correspondence
        /// </summary>
        /// <param name="wire"></param>
        /// <returns></returns>
        private static Field53A GetField53A(WireTicket wire)
        {
            var f53A = new Field53A()
                .setAccount(wire.SendingAccount.BeneficiaryAccountNumber)
                .setBIC(wire.SendingAccount.BeneficiaryType == "ABA" ? string.Empty : wire.SendingAccount.BeneficiaryBICorABA);
            return f53A;
        }

        private static Field53B GetField53B(WireTicket wire)
        {
            var ffcOrUltimateAccount = !string.IsNullOrWhiteSpace(wire.SendingAccount.FFCNumber)
                ? wire.SendingAccount.FFCNumber
                : !string.IsNullOrWhiteSpace(wire.SendingAccount.AccountNumber) ? wire.SendingAccount.AccountNumber : string.Empty;

            var f53B = new Field53B().setAccount(ffcOrUltimateAccount);
            return f53B;
        }

        private static void SetField56X(AbstractMT mtMessage, WireTicket wire)
        {
            var isBicIntermediaryAvailable = wire.SSITemplate.IntermediaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.SSITemplate.IntermediaryBICorABA);
            if (isBicIntermediaryAvailable)
                mtMessage.addField(GetField56A(wire));
            else
                mtMessage.addField(GetField56D(wire));
        }

        private static void SetField57X(AbstractMT mtMessage, WireTicket wire)
        {
            var isBicBeneficiaryAvailable = wire.SSITemplate.BeneficiaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.SSITemplate.BeneficiaryBICorABA);
            if (isBicBeneficiaryAvailable)
                mtMessage.addField(GetField57A(wire));
            else
                mtMessage.addField(GetField57D(wire));
        }

        private static void SetField58X(AbstractMT mtMessage, WireTicket wire)
        {
            var isBicUltimateAvailable = wire.SSITemplate.UltimateBeneficiaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.SSITemplate.UltimateBeneficiaryBICorABA);
            if (isBicUltimateAvailable)
                mtMessage.addField(GetField58A(wire));
            else
                mtMessage.addField(GetField58D(wire));
        }


        private static Field56A GetField56A(WireTicket wire)
        {
            var f56A = new Field56A()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.IntermediaryAccountNumber : wire.SSITemplate.IntermediaryAccountNumber)
                .setBIC(wire.IsBookTransfer ? wire.ReceivingAccount.IntermediaryType == "ABA" ? string.Empty :
                        wire.ReceivingAccount.IntermediaryBICorABA : wire.SSITemplate.IntermediaryType == "ABA" ? string.Empty : wire.SSITemplate.IntermediaryBICorABA);
            return f56A;
        }

        private static Field56D GetField56D(WireTicket wire)
        {
            var interBicOrAba = wire.IsBookTransfer
                ? wire.ReceivingAccount.IntermediaryType == "ABA" ? wire.ReceivingAccount.IntermediaryBICorABA : string.Empty
                : wire.SSITemplate.IntermediaryType == "ABA" ? wire.SSITemplate.IntermediaryBICorABA : string.Empty;

            var f56D = new Field56D().setAccount(!string.IsNullOrWhiteSpace(interBicOrAba) ? string.Format("/FW{0}", interBicOrAba) : string.Empty);

            if (string.IsNullOrWhiteSpace(interBicOrAba))
                return f56D;

            var nameAndAddressed = wire.IsBookTransfer
                ? string.Format("{0}\n{1}", wire.ReceivingAccount.IntermediaryBankName, wire.ReceivingAccount.IntermediaryBankAddress)
                : string.Format("{0}\n{1}", wire.SSITemplate.IntermediaryBankName, wire.SSITemplate.IntermediaryBankAddress);

            f56D.setNameAndAddress(nameAndAddressed);
            return f56D;
        }

        private static Field57A GetField57A(WireTicket wire)
        {
            var f57A = new Field57A()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.BeneficiaryAccountNumber : wire.SSITemplate.BeneficiaryAccountNumber)
                .setBIC(wire.IsBookTransfer ? wire.ReceivingAccount.BeneficiaryType == "ABA" ? string.Empty :
                    wire.ReceivingAccount.BeneficiaryBICorABA : wire.SSITemplate.BeneficiaryType == "ABA" ? string.Empty : wire.SSITemplate.BeneficiaryBICorABA);
            return f57A;
        }

        private static Field57D GetField57D(WireTicket wire)
        {
            var beneficiaryBicOrAba = wire.IsBookTransfer
                ? wire.ReceivingAccount.BeneficiaryType == "ABA" ? wire.ReceivingAccount.BeneficiaryBICorABA : string.Empty
                : wire.SSITemplate.BeneficiaryType == "ABA" ? wire.SSITemplate.BeneficiaryBICorABA : string.Empty;

            var f57D = new Field57D().setAccount(!string.IsNullOrWhiteSpace(beneficiaryBicOrAba) ? string.Format("/FW{0}", beneficiaryBicOrAba) : string.Empty);

            if (string.IsNullOrWhiteSpace(beneficiaryBicOrAba))
                return f57D;

            var nameAndAddressed = wire.IsBookTransfer
                ? string.Format("{0}\n{1}", wire.ReceivingAccount.BeneficiaryBankName, wire.ReceivingAccount.BeneficiaryBankAddress)
                : string.Format("{0}\n{1}", wire.SSITemplate.BeneficiaryBankName, wire.SSITemplate.BeneficiaryBankAddress);

            f57D.setNameAndAddress(nameAndAddressed);

            return f57D;
        }

        private static Field58A GetField58A(WireTicket wire)
        {
            var f58A = new Field58A()
                .setAccount(wire.IsBookTransfer ? wire.SendingAccount.AccountNumber : wire.SSITemplate.AccountNumber)
                .setBIC(wire.IsBookTransfer ? wire.ReceivingAccount.UltimateBeneficiaryType == "ABA" ? string.Empty :
                    wire.ReceivingAccount.UltimateBeneficiaryBICorABA : wire.SSITemplate.UltimateBeneficiaryType == "ABA" ? string.Empty : wire.SSITemplate.UltimateBeneficiaryBICorABA);
            return f58A;
        }

        private static Field58D GetField58D(WireTicket wire)
        {
            var isAbaAvailable = wire.IsBookTransfer
                ? wire.ReceivingAccount.UltimateBeneficiaryType == "ABA" && !string.IsNullOrWhiteSpace(wire.ReceivingAccount.UltimateBeneficiaryBICorABA)
                : wire.SSITemplate.UltimateBeneficiaryType == "ABA" && !string.IsNullOrWhiteSpace(wire.SSITemplate.UltimateBeneficiaryBICorABA);

            var f58D = new Field58D()
                .setAccount(isAbaAvailable ? string.Format("/FW{0}", wire.IsBookTransfer ? wire.ReceivingAccount.UltimateBeneficiaryBICorABA : wire.SSITemplate.UltimateBeneficiaryBICorABA)
                    : wire.IsBookTransfer ? wire.ReceivingAccount.AccountNumber : wire.SSITemplate.AccountNumber);

            if (!isAbaAvailable)
                f58D.setNameAndAddress(wire.IsBookTransfer ? wire.ReceivingAccount.UltimateBeneficiaryAccountName : wire.SSITemplate.UltimateBeneficiaryAccountName);

            if (!isAbaAvailable)
                return f58D;

            var nameAndAddressed = wire.IsBookTransfer
                ? string.Format("{0}\n{1}", wire.ReceivingAccount.UltimateBeneficiaryBankName, wire.ReceivingAccount.UltimateBeneficiaryBankAddress)
                : string.Format("{0}\n{1}", wire.SSITemplate.UltimateBeneficiaryBankName, wire.SSITemplate.UltimateBeneficiaryBankAddress);

            f58D.setNameAndAddress(nameAndAddressed);

            return f58D;
        }

        private static Field59 GetField59(WireTicket wire)
        {
            var f59 = new Field59()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.AccountNumber : wire.SSITemplate.AccountNumber)
                .setNameAndAddressLine1(wire.IsBookTransfer ? wire.ReceivingAccount.UltimateBeneficiaryAccountName : wire.SSITemplate.UltimateBeneficiaryAccountName);

            return f59;
        }

        private static Field70 GetField70(WireTicket wire)
        {
            var f70 = new Field70()
                .setNarrativeLine1("/RFB/" + (wire.HMWire.hmsWirePurposeLkup.Purpose));
            return f70;
        }

        private static Field71A GetField71A(WireTicket wire)
        {
            return new Field71A("OUR");
        }

        private static Field72 GetField72(WireTicket wire, string messageType)
        {
            var f72 = new Field72();

            //var ffcNumber = wire.IsBookTransfer
            //        ? !string.IsNullOrWhiteSpace(wire.ReceivingAccount.FFCNumber)
            //            ? wire.ReceivingAccount.FFCNumber
            //            : string.Empty
            //        : !string.IsNullOrWhiteSpace(wire.SSITemplate.FFCNumber)
            //            ? wire.SSITemplate.FFCNumber
            //            : string.Empty;

            //if (string.IsNullOrWhiteSpace(ffcNumber))
            //    return f72;

            //if (messageType == "MT103")
            //    f72.setNarrativeLine1("/ACC/" + ffcNumber);
            //else
            //    f72.setNarrativeLine1("/BNF/" + ffcNumber);

            //var fccName = wire.IsBookTransfer ? wire.ReceivingAccount.FFCName : wire.SSITemplate.FFCName;
            //if (!string.IsNullOrWhiteSpace(fccName))
            //    f72.setNarrativeLine2("//" + fccName);

            //var reference = wire.IsBookTransfer ? wire.ReceivingAccount.Reference : wire.SSITemplate.Reference;
            //if (!string.IsNullOrWhiteSpace(reference))
            //    f72.setNarrativeLine3("//" + reference);

            //var noOfNarrativeLinesRequired = wire.HMWire.SenderDescription.Length / 30;
            //if (noOfNarrativeLinesRequired == 0)
            //{
            //    f72.setNarrativeLine1(wire.HMWire.hmsWireSenderInformation.SenderInformation + "//" + wire.HMWire.SenderDescription);
            //    return f72;
            //}
            var noOfNarrativeLinesRequired = wire.HMWire.SenderDescription.Length / 30;
            var senderDescriptionInfo = Enumerable.Range(0, noOfNarrativeLinesRequired).Select(s => wire.HMWire.SenderDescription.Substring(s * 30, 30)).ToList();
            if ((noOfNarrativeLinesRequired * 30) < wire.HMWire.SenderDescription.Length)
                senderDescriptionInfo.Add(wire.HMWire.SenderDescription.Substring(noOfNarrativeLinesRequired * 30));

            for (int i = 0; i < senderDescriptionInfo.Count(); i++)
            {
                switch (i)
                {
                    case 0:
                        f72.setNarrativeLine1("/" + wire.HMWire.hmsWireSenderInformation.SenderInformation + "/" + senderDescriptionInfo[i]);
                        break;
                    case 1:
                        f72.setNarrativeLine2(senderDescriptionInfo[i]);
                        break;
                    case 2:
                        f72.setNarrativeLine3(senderDescriptionInfo[i]);
                        break;
                    case 3:
                        f72.setNarrativeLine4(senderDescriptionInfo[i]);
                        break;
                    case 4:
                        f72.setNarrativeLine5(senderDescriptionInfo[i]);
                        break;
                    case 5:
                        f72.setNarrativeLine6(senderDescriptionInfo[i]);
                        break;
                    default: break;
                }
            }

            return f72;
        }

        private static void SetSenderAndReceiverFromHM(AbstractMT callingMethod, WireTicket wire)
        {
            callingMethod.setSenderAndReceiver(HMBICSender, wire.SendingAccount.SendersBIC);
        }

        private static MT103 CreateMt103(WireTicket wire, string messageType, string referenceTag)
        {
            var mt103 = new MT103();
            SetSenderAndReceiverFromHM(mt103, wire);

            mt103.addField(GetField20(wire, referenceTag));

            mt103.addField(GetField23B());

            mt103.addField(GetField32A(wire));

            mt103.addField(GetField50K(wire));

            ////Optional
            //mt103.addField(GetField52A(wire));

            //Optional
            mt103.addField(GetField53B(wire));

            //Optional
            SetField56X(mt103, wire);

            //Optional
            SetField57X(mt103, wire);

            mt103.addField(GetField59(wire));

            mt103.addField(GetField70(wire));

            mt103.addField(GetField71A(wire));

            mt103.addField(GetField72(wire, messageType));

            return mt103;
        }

        private static MT202 CreateMt202(WireTicket wire, string messageType)
        {
            var mt202 = new MT202();

            SetSenderAndReceiverFromHM(mt202, wire);

            mt202.addField(GetField20(wire));

            mt202.addField(GetField21(wire));

            mt202.addField(GetField32A(wire));

            SetField52X(mt202, wire);

            ////Optional
            mt202.addField(GetField53B(wire));

            //Optional
            SetField56X(mt202, wire);

            //Optional
            SetField57X(mt202, wire);

            //Optional
            SetField58X(mt202, wire);

            //Optional
            mt202.addField(GetField72(wire, messageType));

            return mt202;
        }

        private static MT202COV CreateMt202Cov(WireTicket wire)
        {
            var mt202Cov = new MT202COV();

            SetSenderAndReceiverFromHM(mt202Cov, wire);

            mt202Cov.addField(GetField20(wire));

            mt202Cov.addField(GetField21(wire));

            mt202Cov.addField(GetField32A(wire));

            ////Optional
            SetField52X(mt202Cov, wire);

            ////Optional
            mt202Cov.addField(GetField53B(wire));

            //Optional
            SetField56X(mt202Cov, wire);

            //Optional
            SetField57X(mt202Cov, wire);

            //Optional
            SetField58X(mt202Cov, wire);

            mt202Cov.addField(GetField50K(wire));

            mt202Cov.addField(GetField59(wire));

            return mt202Cov;
        }


        private static MT210 CreateMt210(WireTicket wire)
        {
            var mt210 = new MT210();
            SetSenderAndReceiverFromHM(mt210, wire);

            mt210.addField(GetField20(wire));

            mt210.addField(GetField25(wire));

            mt210.addField(GetField30(wire));

            mt210.addField(GetField21(wire));

            mt210.addField(GetField32B(wire));
            //Optional
            //mt210.addField(GetField50A(wire));

            //Optional
            SetField52X(mt210, wire);

            //Optional
            //mt210.addField(GetField56A(wire));

            return mt210;
        }

        private static MT192 CreateMt192(WireTicket wire, string originalMessageType)
        {
            var mt192 = new MT192();
            SetSenderAndReceiverFromHM(mt192, wire);

            mt192.addField(GetField20(wire, "CANC"));

            mt192.addField(GetField21ForCancellation(wire));

            mt192.addField(GetField11S(wire));

            // We need the original message 
            var originalMessage = CreateMessage(wire, originalMessageType, string.Empty);

            foreach (var originalField in originalMessage.Block4.GetFields())
            {
                mt192.Block4.AddField(originalField);
            }

            return mt192;
        }

        private static MT292 CreateMt292(WireTicket wire, string originalMessageType)
        {
            var mt292 = new MT292();
            SetSenderAndReceiverFromHM(mt292, wire);

            mt292.addField(GetField20(wire, "CANC"));

            mt292.addField(GetField21ForCancellation(wire));

            mt292.addField(GetField11S(wire));

            // We need the original message 
            var originalMessage = CreateMessage(wire, originalMessageType, string.Empty);

            foreach (var originalField in originalMessage.Block4.GetFields())
            {
                mt292.Block4.AddField(originalField);
            }


            return mt292;
        }

        private static MT540 CreateMt540(WireTicket wire)
        {
            var mt540 = new MT540();
            SetSenderAndReceiverFromHM(mt540, wire);

            mt540.addField(GetField20(wire));

            return mt540;
        }

        private static MT542 CreateMt542(WireTicket wire)
        {
            var mt542 = new MT542();
            SetSenderAndReceiverFromHM(mt542, wire);

            mt542.addField(GetField20(wire));

            return mt542;
        }
    }
}

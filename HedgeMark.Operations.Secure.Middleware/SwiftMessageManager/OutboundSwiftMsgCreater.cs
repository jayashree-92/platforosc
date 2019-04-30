using System.IO;
using HMOSecureMiddleware.Models;
using Com.HedgeMark.Commons;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT;
using HedgeMark.SwiftMessageHandler.Model.MT.MT1XX;
using HedgeMark.SwiftMessageHandler.Model.MT.MT2XX;
using HedgeMark.SwiftMessageHandler.Model.MT.MT5XX;

namespace HMOSecureMiddleware.SwiftMessageManager
{
    public class OutboundSwiftMsgCreater
    {
        protected static readonly string HMBIC = ConfigurationManagerWrapper.StringSetting("HMBIC", "IRVTBEB0");
        protected static readonly string HMBICSender = string.Format("{0}{1}", HMBIC, ConfigurationManagerWrapper.StringSetting("HMBICSender", "XXXX"));
        public static string CreateMessage(WireTicket wire,string messageType)
        {
            switch (messageType)
            {
                //MT103 - Single customer credit transfer
                case "MT103":
                    return CreateMt103(wire).GetMessage(); 
                //MT202 - General Financial inst Transfer
                case "MT202":
                    return CreateMt202(wire).GetMessage();
                //MT202 COV - General Financial inst Transfer
                case "MT202 COV":
                case "MT202COV":
                    return CreateMt202Cov(wire).GetMessage();
                //MT210 - Notice to Receive
                case "MT210":
                    return CreateMt210(wire);
                //MT192 - request for cancelation
                case "MT192":
                    return CreateMt192(wire);
                //MT292 - Request for Cancelation
                case "MT292":
                    return CreateMt292(wire);
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

        private static Field20 GetField20(WireTicket wire, bool isCancellation = false)
        {
            var environmentStr = Utility.Environment.ToUpper() == "PROD" ? string.Empty : Utility.Environment.ToUpper();
            return new Field20(string.Format("{0}{1}DMO{2}", isCancellation ? "C" : string.Empty, environmentStr, wire.WireId));
        }

        private static Field21 GetField21ForCancellation(WireTicket wire)
        {
            var environmentStr = Utility.Environment.ToUpper() == "PROD" ? string.Empty : Utility.Environment.ToUpper();
            return new Field21().setReference(string.Format("{0}DMO{1}", environmentStr, wire.WireId));
        }

        private static Field21 GetField21(WireTicket wire)
        {
            return new Field21().setReference(wire.Account.FFCNumber);
        }

        private static Field23B GetField23B()
        {
            return new Field23B("CRED");
        }

        private static Field25 GetField25(WireTicket wire)
        {
            return new Field25().setAccount(wire.Account.AccountNumber);
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
                .setCurrency(wire.Account.Currency);
            return f32B;
        }

        private static Field50K GetField50K(WireTicket wire)
        {
            var f50K = new Field50K()
                .setAccount(wire.Account.AccountNumber)
                .setNameAndAddress(wire.Account.UltimateBeneficiaryBankName)
                .setNameAndAddressLine1(wire.Account.UltimateBeneficiaryBankAddress);
            return f50K;
        }

        private static Field50A GetField50A(WireTicket wire)
        {
            var f50A = new Field50A()
                .setAccount(wire.Account.AccountNumber)
                .setBIC(HMBIC);
            return f50A;
        }

        private static Field52A GetField52A(WireTicket wire)
        {
            var f52A = new Field52A()
                .setAccount(wire.Account.AccountNumber)
                .setBIC(HMBIC);
            return f52A;
        }

        private static Field56A GetField56A(WireTicket wire)
        {
            var f56A = new Field56A()
                .setAccount(wire.Account.IntermediaryAccountNumber)
                .setBIC(wire.Account.IntermediaryBICorABA);
            return f56A;
        }

        private static Field57A GetField57A(WireTicket wire)
        {
            var f57A = new Field57A()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.IntermediaryAccountNumber : wire.SSITemplate.IntermediaryAccountNumber)
                .setBIC(wire.IsBookTransfer ? wire.ReceivingAccount.IntermediaryBICorABA : wire.SSITemplate.IntermediaryBICorABA);
            return f57A;
        }

        private static Field57D GetField57D(WireTicket wire)
        {
            var f57D = new Field57D()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.BeneficiaryAccountNumber : wire.SSITemplate.BeneficiaryAccountNumber)
                .setNameAndAddress(wire.IsBookTransfer ? wire.ReceivingAccount.BeneficiaryBankName : wire.SSITemplate.BeneficiaryBankName)
                .setNameAndAddressLine1(wire.IsBookTransfer ? wire.ReceivingAccount.BeneficiaryBankAddress : wire.SSITemplate.BeneficiaryBankAddress);
            return f57D;
        }

        private static Field58A GetField58A(WireTicket wire)
        {
            var f58A = new Field58A()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.BeneficiaryAccountNumber : wire.SSITemplate.AccountNumber)
                .setBIC(wire.IsBookTransfer ? wire.ReceivingAccount.BeneficiaryBICorABA : wire.SSITemplate.UltimateBeneficiaryBICorABA);
            return f58A;
        }

        private static Field59 GetField59(WireTicket wire)
        {
            var f59 = new Field59()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.AccountNumber : wire.SSITemplate.AccountNumber)
                .setNameAndAddress(wire.IsBookTransfer ? wire.ReceivingAccount.UltimateBeneficiaryBankAddress : wire.SSITemplate.UltimateBeneficiaryBankName)
                .setNameAndAddressLine1(wire.IsBookTransfer ? wire.ReceivingAccount.UltimateBeneficiaryBankAddress : wire.SSITemplate.UltimateBeneficiaryBankAddress);
            return f59;
        }

        private static Field71A GetField71A(WireTicket wire)
        {
            return new Field71A(wire.HMWire.DeliveryCharges);
        }

        private static Field72 GetField72(WireTicket wire)
        {
            var f72 = new Field72()
                .setNarrative("/INS/" + (wire.IsBookTransfer ?
                    wire.ReceivingAccount.FFCName : wire.SSITemplate.FFCName))
                .setNarrativeLine1("/ACC/" + (wire.IsBookTransfer ? wire.ReceivingAccount.FFCNumber : wire.SSITemplate.FFCNumber));
            return f72;
        }

        private static void SetSenderAndReceiverFromSSI(AbstractMT callingMethod, WireTicket wire)
        {
            callingMethod.setSenderAndReceiver(wire.Account.SendersBIC, wire.IsBookTransfer ? wire.ReceivingAccount.UltimateBeneficiaryBICorABA : wire.SSITemplate.UltimateBeneficiaryBICorABA);
        }
        private static void SetSenderAndReceiverFromAccountSender(AbstractMT callingMethod, WireTicket wire)
        {
            callingMethod.setSenderAndReceiver(wire.Account.SendersBIC, wire.Account.UltimateBeneficiaryBICorABA);
        }

        private static void SetSenderAndReceiverFromHM(AbstractMT callingMethod, WireTicket wire)
        {
            callingMethod.setSenderAndReceiver(HMBICSender, wire.Account.BeneficiaryBICorABA);
        }

        private static MT103 CreateMt103(WireTicket wire)
        {
            var mt103 = new MT103();
            SetSenderAndReceiverFromSSI(mt103, wire);

            mt103.addField(GetField20(wire));

            mt103.addField(GetField23B());

            mt103.addField(GetField32A(wire));

            mt103.addField(GetField50K(wire));
            //Optional
            mt103.addField(GetField52A(wire));
            
            //Optional
            //mt103.addField(GetField57D(wire));

            mt103.addField(GetField59(wire));

            mt103.addField(GetField71A(wire));

            return mt103;
        }

        private static MT202 CreateMt202(WireTicket wire)
        {
            var mt202 = new MT202();
            SetSenderAndReceiverFromSSI(mt202, wire);

            mt202.addField(GetField20(wire));

            mt202.addField(GetField21(wire));

            mt202.addField(GetField32A(wire));
            //Optional
            // mt202.addField(GetField56A(wire));
            //Optional
            // mt202.addField(GetField57A(wire));

            mt202.addField(GetField58A(wire));
            //Optional
            mt202.addField(GetField72(wire));

            return mt202;
        }

        private static MT202COV CreateMt202Cov(WireTicket wire)
        {
            var mt202Cov = new MT202COV();
            SetSenderAndReceiverFromSSI(mt202Cov, wire);

            mt202Cov.addField(GetField20(wire));

            mt202Cov.addField(GetField21(wire));

            mt202Cov.addField(GetField32A(wire));
            //Optional
            //mt202Cov.addField(GetField56A(wire));

            mt202Cov.addField(GetField58A(wire));

            mt202Cov.addField(GetField50A(wire));

            mt202Cov.addField(GetField59(wire));

            return mt202Cov;
        }


        private static string CreateMt210(WireTicket wire)
        {
            var mt210 = new MT210();
            SetSenderAndReceiverFromHM(mt210, wire);

            //mt210.setReceiver("IRVTBEB0XXXX");

            mt210.addField(GetField20(wire));

            mt210.addField(GetField25(wire));

            mt210.addField(GetField30(wire));

            mt210.addField(GetField21(wire));

            mt210.addField(GetField32B(wire));
            //Optional
            //mt210.addField(GetField50A(wire));
            //Optional
            mt210.addField(GetField52A(wire));
            //Optional
            //mt210.addField(GetField56A(wire));

            return mt210.GetMessage();
        }

        private static string CreateMt192(WireTicket wire)
        {
            var mt192 = new MT192();
            SetSenderAndReceiverFromSSI(mt192, wire);

            mt192.addField(GetField20(wire, true));

            mt192.addField(GetField21ForCancellation(wire));

            mt192.addField(GetField11S(wire));

            // We need the original M103 message 
            var mt103 = CreateMt103(wire);
            foreach (var mt103Field in mt103.Block4.GetFields())
            {
                mt192.Block4.AddField(mt103Field);
            }

            return mt192.GetMessage();
        }

        private static string CreateMt292(WireTicket wire)
        {
            var mt292 = new MT292();
            SetSenderAndReceiverFromSSI(mt292, wire);

            mt292.addField(GetField20(wire, true));

            mt292.addField(GetField21ForCancellation(wire));

            mt292.addField(GetField11S(wire));

            // We need the original M103 message 
            var mt202 = CreateMt202(wire);
            foreach (var mt103Field in mt202.Block4.GetFields())
            {
                mt292.Block4.AddField(mt103Field);
            }

            return mt292.GetMessage();
        }

        private static string CreateMt540(WireTicket wire)
        {
            var mt540 = new MT540();
            SetSenderAndReceiverFromSSI(mt540, wire);

            mt540.addField(GetField20(wire));

            return mt540.GetMessage();
        }

        private static string CreateMt542(WireTicket wire)
        {
            var mt542 = new MT542();
            SetSenderAndReceiverFromSSI(mt542, wire);

            mt542.addField(GetField20(wire));

            return mt542.GetMessage();
        }
    }
}

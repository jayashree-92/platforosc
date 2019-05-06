using System.IO;
using System.Text;
using HMOSecureMiddleware.Models;
using Com.HedgeMark.Commons;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT;
using HedgeMark.SwiftMessageHandler.Model.MT.MT1XX;
using HedgeMark.SwiftMessageHandler.Model.MT.MT2XX;
using HedgeMark.SwiftMessageHandler.Model.MT.MT5XX;

namespace HMOSecureMiddleware.SwiftMessageManager
{
    public class OutboundSwiftMsgCreator
    {
        protected static readonly string HMBIC = ConfigurationManagerWrapper.StringSetting("HMBIC", "HMRKUS30");
        protected static readonly string HMBICSender = string.Format("{0}{1}", HMBIC, ConfigurationManagerWrapper.StringSetting("HMBICSender", "XXXX"));
        public static string CreateMessage(WireTicket wire, string messageType)
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
                //MT192 - request for cancellation
                case "MT192":
                    return CreateMt192(wire);
                //MT292 - Request for Cancellation
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
            var transactionId = WireDataManager.GetWireTransactionId(wire.WireId);
            return new Field20(string.Format("{0}{1}", isCancellation ? "C" : string.Empty, transactionId));
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
            return new Field25().setAccount(wire.SendingAccount.AccountNumber);
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
            var f50K = new Field50K()
                .setAccount(!string.IsNullOrWhiteSpace(wire.SendingAccount.FFCNumber) ? wire.SendingAccount.FFCNumber : wire.SendingAccount.AccountNumber)
                .setNameAndAddress(wire.SendingAccount.AccountName);
            return f50K;
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
                .setBIC(wire.SendingAccount.IsUltimateBeneficiaryABA ? string.Empty : wire.SendingAccount.UltimateBeneficiaryBICorABA);
            return f52A;
        }


        private static Field52D GetField52D(WireTicket wire)
        {
            var f52D = new Field52D()
                .setAccount(wire.SendingAccount.AccountNumber)
                .setNameAndAddressLine1(wire.SendingAccount.AccountName);
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
                .setBIC(wire.SendingAccount.IsBeneficiaryABA ? string.Empty : wire.SendingAccount.BeneficiaryBICorABA);
            return f53A;
        }

        //need to check if 53B is required or not

        private static Field56A GetField56A(WireTicket wire)
        {
            var f56A = new Field56A()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.IntermediaryAccountNumber : wire.SSITemplate.IntermediaryAccountNumber)
                .setBIC(wire.IsBookTransfer ? wire.ReceivingAccount.IsIntermediaryABA ? string.Empty :
                        wire.ReceivingAccount.IntermediaryBICorABA : wire.SSITemplate.IsIntermediaryABA ? string.Empty : wire.SSITemplate.IntermediaryBICorABA);
            return f56A;
        }

        private static Field56D GetField56D(WireTicket wire)
        {
            var interBicOrAba = wire.IsBookTransfer
                ? wire.ReceivingAccount.IsIntermediaryABA ? wire.ReceivingAccount.IntermediaryBICorABA : string.Empty
                : wire.SSITemplate.IsIntermediaryABA ? wire.SSITemplate.IntermediaryBICorABA : string.Empty;

            var f56D = new Field56D().setAccount(!string.IsNullOrWhiteSpace(interBicOrAba) ? string.Format("/FW{0}", interBicOrAba) : string.Empty);

            if (!string.IsNullOrWhiteSpace(interBicOrAba))
            {
                var nameAndAddressed = wire.IsBookTransfer
                    ? string.Format("{0}\n{1}", wire.ReceivingAccount.IntermediaryBankName, wire.ReceivingAccount.IntermediaryBankAddress)
                    : string.Format("{0}\n{1}", wire.SSITemplate.IntermediaryBankName, wire.SSITemplate.IntermediaryBankAddress);

                f56D.setNameAndAddress(nameAndAddressed);
            }
            return f56D;
        }

        private static Field57A GetField57A(WireTicket wire)
        {
            var f57A = new Field57A()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.BeneficiaryAccountNumber : wire.SSITemplate.BeneficiaryAccountNumber)
                .setBIC(wire.IsBookTransfer ? wire.ReceivingAccount.IsBeneficiaryABA ? string.Empty :
                    wire.ReceivingAccount.BeneficiaryBICorABA : wire.SSITemplate.IsBeneficiaryABA ? string.Empty : wire.SSITemplate.BeneficiaryBICorABA);
            return f57A;
        }

        private static Field57D GetField57D(WireTicket wire)
        {
            var beneBicOrAba = wire.IsBookTransfer
                ? wire.ReceivingAccount.IsBeneficiaryABA ? wire.ReceivingAccount.BeneficiaryBICorABA : string.Empty
                : wire.SSITemplate.IsBeneficiaryABA ? wire.SSITemplate.BeneficiaryBICorABA : string.Empty;

            var f57D = new Field57D().setAccount(!string.IsNullOrWhiteSpace(beneBicOrAba) ? string.Format("/FW{0}", beneBicOrAba) : string.Empty);

            if (!string.IsNullOrWhiteSpace(beneBicOrAba))
            {
                var nameAndAddressed = wire.IsBookTransfer
                    ? string.Format("{0}\n{1}", wire.ReceivingAccount.BeneficiaryBankName, wire.ReceivingAccount.BeneficiaryBankAddress)
                    : string.Format("{0}\n{1}", wire.SSITemplate.BeneficiaryBankName, wire.SSITemplate.BeneficiaryBankAddress);

                f57D.setNameAndAddress(nameAndAddressed);
            }

            return f57D;
        }

        private static Field58A GetField58A(WireTicket wire)
        {
            var f58A = new Field58A()
                .setAccount(wire.IsBookTransfer ? wire.SendingAccount.AccountNumber : wire.SSITemplate.AccountNumber)
                .setBIC(wire.IsBookTransfer ? wire.ReceivingAccount.IsUltimateBeneficiaryABA ? string.Empty :
                    wire.ReceivingAccount.UltimateBeneficiaryBICorABA : wire.SSITemplate.IsUltimateBeneficiaryABA ? string.Empty : wire.SSITemplate.UltimateBeneficiaryBICorABA);
            return f58A;
        }

        private static Field58D GetField58D(WireTicket wire)
        {
            var isAbaAvailable = wire.IsBookTransfer ? wire.ReceivingAccount.IsUltimateBeneficiaryABA && !string.IsNullOrWhiteSpace(wire.ReceivingAccount.UltimateBeneficiaryBICorABA)
                                                          : wire.SSITemplate.IsUltimateBeneficiaryABA && !string.IsNullOrWhiteSpace(wire.SSITemplate.UltimateBeneficiaryBICorABA);

            var f58D = new Field58D()
                .setAccount(isAbaAvailable ? string.Format("/FW{0}", wire.IsBookTransfer ? wire.ReceivingAccount.UltimateBeneficiaryBICorABA : wire.SSITemplate.UltimateBeneficiaryBICorABA)
                    : wire.IsBookTransfer ? wire.ReceivingAccount.AccountNumber : wire.SSITemplate.AccountNumber);

            if(!isAbaAvailable)
                f58D.setNameAndAddress(wire.IsBookTransfer ? wire.ReceivingAccount.Reference : wire.SSITemplate.Reference);
            
            if (isAbaAvailable)
            {
                var nameAndAddressed = wire.IsBookTransfer
                    ? string.Format("{0}\n{1}", wire.ReceivingAccount.UltimateBeneficiaryBankName, wire.ReceivingAccount.UltimateBeneficiaryBankAddress)
                    : string.Format("{0}\n{1}", wire.SSITemplate.UltimateBeneficiaryBankName, wire.SSITemplate.UltimateBeneficiaryBankAddress);

                f58D.setNameAndAddress(nameAndAddressed);
            }

            return f58D;
        }

        private static Field59 GetField59(WireTicket wire)
        {
            var f59 = new Field59()
                .setAccount(wire.IsBookTransfer ? wire.ReceivingAccount.AccountNumber : wire.SSITemplate.AccountNumber)
                .setNameAndAddressLine1(wire.IsBookTransfer ? wire.ReceivingAccount.Reference : wire.SSITemplate.Reference);

            return f59;
        }

        private static Field71A GetField71A(WireTicket wire)
        {
            return new Field71A(wire.HMWire.DeliveryCharges);
        }

        private static Field72 GetField72(WireTicket wire)
        {
            var f72 = new Field72()
                .setNarrative("/BNF/" + (wire.IsBookTransfer ? wire.ReceivingAccount.FFCNumber : wire.SSITemplate.FFCNumber));
            return f72;
        }

        private static void SetSenderAndReceiverFromHM(AbstractMT callingMethod, WireTicket wire)
        {
            callingMethod.setSenderAndReceiver(HMBICSender, wire.SendingAccount.SendersBIC);
        }

        private static MT103 CreateMt103(WireTicket wire)
        {
            var mt103 = new MT103();
            SetSenderAndReceiverFromHM(mt103, wire);

            mt103.addField(GetField20(wire));

            mt103.addField(GetField23B());

            mt103.addField(GetField32A(wire));

            mt103.addField(GetField50K(wire));

            ////Optional
            //mt103.addField(GetField52A(wire));

            //Optional
            // mt103.addField(GetField53A(wire));

            //Optional
            var isBicIntermediaryAvailable = !wire.SSITemplate.IsIntermediaryABA && !string.IsNullOrWhiteSpace(wire.SSITemplate.IntermediaryBICorABA);
            if (isBicIntermediaryAvailable)
                mt103.addField(GetField56A(wire));
            else
                mt103.addField(GetField56D(wire));

            //Optional
            var isBicBeneficiaryAvailable = !wire.SSITemplate.IsBeneficiaryABA && !string.IsNullOrWhiteSpace(wire.SSITemplate.BeneficiaryBICorABA);
            if (isBicBeneficiaryAvailable)
                mt103.addField(GetField57A(wire));
            else
                mt103.addField(GetField57D(wire));

            mt103.addField(GetField59(wire));

            mt103.addField(GetField71A(wire));

            return mt103;
        }

        private static MT202 CreateMt202(WireTicket wire)
        {
            var mt202 = new MT202();

            SetSenderAndReceiverFromHM(mt202, wire);

            mt202.addField(GetField20(wire));

            mt202.addField(GetField21(wire));

            mt202.addField(GetField32A(wire));


            if (!wire.SendingAccount.IsUltimateBeneficiaryABA && !string.IsNullOrWhiteSpace(wire.SendingAccount.UltimateBeneficiaryBICorABA))
                mt202.addField(GetField52A(wire));
            else
                mt202.addField(GetField52D(wire));

            ////Optional
            //mt202.addField(GetField53A(wire));

            //Optional
            var isBicIntermediaryAvailable = !wire.SSITemplate.IsIntermediaryABA && !string.IsNullOrWhiteSpace(wire.SSITemplate.IntermediaryBICorABA);
            if (isBicIntermediaryAvailable)
                mt202.addField(GetField56A(wire));
            else
                mt202.addField(GetField56D(wire));

            //Optional
            var isBicBeneficiaryAvailable = !wire.SSITemplate.IsBeneficiaryABA && !string.IsNullOrWhiteSpace(wire.SSITemplate.BeneficiaryBICorABA);
            if (isBicBeneficiaryAvailable)
                mt202.addField(GetField57A(wire));
            else
                mt202.addField(GetField57D(wire));

            //Optional
            var isBicUltimateAvailable = !wire.SSITemplate.IsUltimateBeneficiaryABA && !string.IsNullOrWhiteSpace(wire.SSITemplate.UltimateBeneficiaryBICorABA);
            if (isBicUltimateAvailable)
                mt202.addField(GetField58A(wire));
            else
                mt202.addField(GetField58D(wire));

            //Optional
            mt202.addField(GetField72(wire));

            return mt202;
        }

        private static MT202COV CreateMt202Cov(WireTicket wire)
        {
            var mt202Cov = new MT202COV();

            SetSenderAndReceiverFromHM(mt202Cov, wire);

            mt202Cov.addField(GetField20(wire));

            mt202Cov.addField(GetField21(wire));

            mt202Cov.addField(GetField32A(wire));

            mt202Cov.addField(GetField50K(wire));

            if (!wire.SendingAccount.IsUltimateBeneficiaryABA && !string.IsNullOrWhiteSpace(wire.SendingAccount.UltimateBeneficiaryBICorABA))
                mt202Cov.addField(GetField52A(wire));
            else
                mt202Cov.addField(GetField52D(wire));

            ////Optional
            //mt202Cov.addField(GetField53A(wire));


            //Optional
            var isBicIntermediaryAvailable = !wire.SSITemplate.IsIntermediaryABA && !string.IsNullOrWhiteSpace(wire.SSITemplate.IntermediaryBICorABA);
            if (isBicIntermediaryAvailable)
                mt202Cov.addField(GetField56A(wire));
            else
                mt202Cov.addField(GetField56D(wire));

            //Optional
            var isBicBeneficiaryAvailable = !wire.SSITemplate.IsBeneficiaryABA && !string.IsNullOrWhiteSpace(wire.SSITemplate.BeneficiaryBICorABA);
            if (isBicBeneficiaryAvailable)
                mt202Cov.addField(GetField57A(wire));
            else
                mt202Cov.addField(GetField57D(wire));

            //Optional
            var isBicUltimateAvailable = !wire.SSITemplate.IsUltimateBeneficiaryABA && !string.IsNullOrWhiteSpace(wire.SSITemplate.UltimateBeneficiaryBICorABA);
            if (isBicUltimateAvailable)
                mt202Cov.addField(GetField58A(wire));
            else
                mt202Cov.addField(GetField58D(wire));


            mt202Cov.addField(GetField59(wire));

            return mt202Cov;
        }


        private static string CreateMt210(WireTicket wire)
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
            mt210.addField(GetField52A(wire));
            //Optional
            //mt210.addField(GetField56A(wire));

            return mt210.GetMessage();
        }

        private static string CreateMt192(WireTicket wire)
        {
            var mt192 = new MT192();
            SetSenderAndReceiverFromHM(mt192, wire);

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
            SetSenderAndReceiverFromHM(mt292, wire);

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
            SetSenderAndReceiverFromHM(mt540, wire);

            mt540.addField(GetField20(wire));

            return mt540.GetMessage();
        }

        private static string CreateMt542(WireTicket wire)
        {
            var mt542 = new MT542();
            SetSenderAndReceiverFromHM(mt542, wire);

            mt542.addField(GetField20(wire));

            return mt542.GetMessage();
        }
    }
}

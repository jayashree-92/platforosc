using System;
using System.IO;
using System.Linq;
using Com.HedgeMark.Commons;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT;
using HedgeMark.SwiftMessageHandler.Model.MT.MT1XX;
using HedgeMark.SwiftMessageHandler.Model.MT.MT2XX;
using HedgeMark.SwiftMessageHandler.Model.MT.MT5XX;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Middleware.Util;
using Extensions = HedgeMark.SwiftMessageHandler.Utils.Extensions;

namespace HM.Operations.Secure.Middleware.SwiftMessageManager
{
    public class OutboundSwiftMsgCreator
    {
        protected static readonly string HMBIC = ConfigurationManagerWrapper.StringSetting("HMBIC", "HMRKUS30");
        protected static readonly string HMBICSender = $"{HMBIC}{ConfigurationManagerWrapper.StringSetting("HMBICSender", "XXXX")}";
        public static AbstractMT CreateMessage(WireTicket wire, string messageType, string originalMessageType, WireReferenceTag referenceTag = WireReferenceTag.NA)
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

            throw new InvalidDataException($"Wire Message type {messageType} not handled");
        }

        private static Field11S GetField11S(WireTicket wire)
        {
            var f11S = new Field11S()
                .setDate(wire.HMWire.ValueDate)
                .setMT(wire.HMWire.hmsWireMessageType.MessageType.Replace(" ", string.Empty).Replace("COV", string.Empty));
            return f11S;
        }

        private static Field20 GetField20(WireTicket wire, WireReferenceTag referenceTag = WireReferenceTag.NA)
        {
            var transactionId = WireDataManager.GetWireTransactionId(wire.WireId);
            return new Field20($"{transactionId}{(referenceTag != WireReferenceTag.NA ? string.Empty : "/")}{referenceTag}");
        }

        private static Field21 GetField21ForCancellation(WireTicket wire)
        {
            var transactionId = WireDataManager.GetWireTransactionId(wire.WireId);
            return new Field21().setReference(transactionId);
        }

        private static Field21 GetField21(WireTicket wire)
        {
            if (wire.HMWire.hmsWireField != null && wire.HMWire.hmsWireField.hmsCollateralCashPurposeLkupId > 0)
                return new Field21().setReference(wire.HMWire.hmsWireField.hmsCollateralCashPurposeLkup.PurposeCode);

            return new Field21().setReference("NONREF");
        }

        private static Field23B GetField23B()
        {
            return new Field23B("CRED");
        }

        private static Field25 GetField25(WireTicket wire)
        {
            return new Field25().setAccount(!string.IsNullOrWhiteSpace(wire.SendingAccount.FFCNumber) ? wire.SendingAccount.FFCNumber : wire.SendingAccount.UltimateBeneficiaryAccountNumber);
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
                .setCurrency(wire.IsFundTransfer ? wire.ReceivingAccount.Currency : wire.SSITemplate.Currency)
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


        private static void SetField50X(AbstractMT mtMessage, WireTicket wire)
        {
            if (!wire.IsFundTransfer && wire.SSITemplate != null
                    ? wire.SSITemplate.UltimateBeneficiaryType == "BIC" && !string.IsNullOrWhiteSpace(wire.SSITemplate.UltimateBeneficiary.BICorABA)
                    : wire.SendingAccount.UltimateBeneficiaryType == "BIC" && !string.IsNullOrWhiteSpace(wire.SendingAccount.UltimateBeneficiary.BICorABA))
                mtMessage.addField(GetField50C(wire));
            else
                mtMessage.addField(GetField50F(wire));
        }


        private static Field50C GetField50C(WireTicket wire)
        {
            var shouldUseSSI = !wire.IsFundTransfer && wire.SSITemplate != null;
            var f50A = new Field50C()
                .setAccount(shouldUseSSI ? wire.SSITemplate.UltimateBeneficiaryAccountNumber : wire.SendingAccount.UltimateBeneficiaryAccountNumber)
                .setBIC(shouldUseSSI
                    ? wire.SSITemplate.UltimateBeneficiaryType == "ABA" ? string.Empty : wire.SSITemplate.UltimateBeneficiary.BICorABA
                    : wire.SendingAccount.UltimateBeneficiaryType == "ABA" ? string.Empty : wire.SendingAccount.UltimateBeneficiary.BICorABA);
            return f50A;
        }


        private static Field50F GetField50F(WireTicket wire)
        {
            var shouldUseSSI = !wire.IsFundTransfer && wire.SSITemplate != null;
            var ffcNumber = shouldUseSSI ? wire.SSITemplate.FFCNumber : wire.SendingAccount.FFCNumber;
            var ffcName = shouldUseSSI ? wire.SSITemplate.FFCName : wire.SendingAccount.FFCName;

            var nameAndAddress = !string.IsNullOrWhiteSpace(ffcName) ? ffcName : shouldUseSSI ? wire.SSITemplate.UltimateBeneficiary.BankName : wire.SendingAccount.UltimateBeneficiaryAccountName;
            nameAndAddress += $"\n{wire.FundRegisterAddress}";

            var f50F = new Field50F()
                .setAccount(!string.IsNullOrWhiteSpace(ffcNumber)
                    ? ffcNumber
                    : shouldUseSSI ? wire.SSITemplate.UltimateBeneficiaryAccountNumber : wire.SendingAccount.UltimateBeneficiaryAccountNumber)
                .setNameAndAddress(nameAndAddress);

            return f50F;
        }

        /// <summary>
        /// Ordering Customer
        /// </summary>
        /// <param name="wire"></param>
        /// <returns></returns>
        private static Field50K GetField50K(WireTicket wire)
        {
            var isFFCAvailable = !string.IsNullOrWhiteSpace(wire.SendingAccount.FFCNumber);
            var isFFCNameAvailable = !string.IsNullOrWhiteSpace(wire.SendingAccount.FFCName);

            var nameAndAddress = isFFCNameAvailable ? wire.SendingAccount.FFCName : wire.SendingAccount.UltimateBeneficiaryAccountName;
            nameAndAddress += $"\n{wire.SendingAccount.UltimateBeneficiary.BankAddress}";
            nameAndAddress += $"\n{wire.FundRegisterAddress}";

            var f50K = new Field50K()
                .setAccount(isFFCAvailable ? wire.SendingAccount.FFCNumber : wire.SendingAccount.UltimateBeneficiaryAccountNumber)
                .setNameAndAddress(nameAndAddress);

            return f50K;
        }

        private static void SetField52X(AbstractMT mtMessage, WireTicket wire, bool shouldUseSSIBeneficiary)
        {
            if (shouldUseSSIBeneficiary
                    ? !wire.IsFundTransfer && wire.SSITemplate != null
                        ? wire.SSITemplate.BeneficiaryType == "BIC" && !string.IsNullOrWhiteSpace(wire.SSITemplate.Beneficiary.BICorABA)
                        : wire.SendingAccount.BeneficiaryType == "BIC" && !string.IsNullOrWhiteSpace(wire.SendingAccount.Beneficiary.BICorABA)
                    : wire.SendingAccount.UltimateBeneficiaryType == "BIC" && !string.IsNullOrWhiteSpace(wire.SendingAccount.UltimateBeneficiary.BICorABA))
                mtMessage.addField(GetField52A(wire, shouldUseSSIBeneficiary));
            else
                mtMessage.addField(GetField52D(wire, shouldUseSSIBeneficiary));
        }

        /// <summary>
        /// "Ordering Customer"
        /// </summary>
        /// <param name="wire"></param>
        /// <param name="shouldUseBeneficiary"></param>
        /// <returns></returns>
        //private static Field50A GetField50A(WireTicket wire)
        //{
        //    var f50A = new Field50A()
        //        .setAccount(wire.)
        //        .setBIC(HMBIC);
        //    return f50A;
        //}

        private static Field52A GetField52A(WireTicket wire, bool shouldUseBeneficiary)
        {
            var shouldUseSSI = !wire.IsFundTransfer && wire.SSITemplate != null;
            var f52A = new Field52A()
                .setAccount(shouldUseBeneficiary
                    ? shouldUseSSI
                        ? wire.SSITemplate.BeneficiaryAccountNumber
                        : wire.SendingAccount.BeneficiaryAccountNumber
                        : shouldUseSSI ? wire.SSITemplate.UltimateBeneficiaryAccountNumber : wire.SendingAccount.UltimateBeneficiaryAccountNumber)
                .setBIC(shouldUseBeneficiary
                    ? shouldUseSSI
                        ? wire.SSITemplate.BeneficiaryType == "BIC" ? wire.SSITemplate.Beneficiary.BICorABA : string.Empty
                        : wire.SendingAccount.BeneficiaryType == "BIC" ? wire.SendingAccount.Beneficiary.BICorABA : string.Empty
                    : wire.SendingAccount.UltimateBeneficiaryType == "BIC" ? wire.SendingAccount.UltimateBeneficiary.BICorABA : string.Empty);
            return f52A;
        }

        private static Field52D GetField52D(WireTicket wire, bool shouldUseBeneficiary)
        {
            var shouldUseSSI = !wire.IsFundTransfer && wire.SSITemplate != null;

            var nameAndAddress = shouldUseBeneficiary
                    ? shouldUseSSI 
                        ? wire.SSITemplate.Beneficiary.BankName 
                        : wire.SendingAccount.Beneficiary.BankName
                    : !string.IsNullOrWhiteSpace(wire.SendingAccount.FFCName)
                        ? wire.SendingAccount.FFCName
                        : wire.SendingAccount.UltimateBeneficiaryAccountName;

            nameAndAddress += $"\n{wire.FundRegisterAddress}";

            var f52D = new Field52D()
                .setAccount(shouldUseBeneficiary
                        ? shouldUseSSI ? wire.SSITemplate.Beneficiary.BICorABA : wire.SendingAccount.Beneficiary.BICorABA
                        : !string.IsNullOrWhiteSpace(wire.SendingAccount.FFCNumber)
                            ? wire.SendingAccount.FFCNumber
                            : wire.SendingAccount.UltimateBeneficiaryAccountNumber)
                .setNameAndAddress(nameAndAddress);

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
                .setBIC(wire.SendingAccount.BeneficiaryType == "ABA" ? string.Empty : wire.SendingAccount.Beneficiary.BICorABA);
            return f53A;
        }

        private static Field53B GetField53B(WireTicket wire)
        {
            var ffcOrUltimateAccount = !string.IsNullOrWhiteSpace(wire.SendingAccount.FFCNumber)
                ? wire.SendingAccount.FFCNumber
                : !string.IsNullOrWhiteSpace(wire.SendingAccount.UltimateBeneficiaryAccountNumber) ? wire.SendingAccount.UltimateBeneficiaryAccountNumber : string.Empty;

            var f53B = new Field53B().setAccount(ffcOrUltimateAccount);
            return f53B;
        }

        private static void SetField56X(AbstractMT mtMessage, WireTicket wire)
        {
            var isBicIntermediaryAvailable = wire.IsFundTransfer ? (wire.ReceivingAccount.IntermediaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.ReceivingAccount.Intermediary.BICorABA)) : (wire.SSITemplate.IntermediaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.SSITemplate.Intermediary.BICorABA));
            if (isBicIntermediaryAvailable)
                mtMessage.addField(GetField56A(wire));
            else
                mtMessage.addField(GetField56D(wire));
        }

        private static void SetField57X(AbstractMT mtMessage, WireTicket wire)
        {
            var isBicBeneficiaryAvailable = wire.IsFundTransfer ? (wire.ReceivingAccount.BeneficiaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.ReceivingAccount.Beneficiary.BICorABA)) : (wire.SSITemplate.BeneficiaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.SSITemplate.Beneficiary.BICorABA));
            if (isBicBeneficiaryAvailable)
                mtMessage.addField(GetField57A(wire));
            else
                mtMessage.addField(GetField57D(wire));
        }

        private static void SetField58X(AbstractMT mtMessage, WireTicket wire)
        {
            var isBicUltimateAvailable = wire.IsFundTransfer ? (wire.ReceivingAccount.UltimateBeneficiaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.ReceivingAccount.UltimateBeneficiary.BICorABA)) : (wire.SSITemplate.UltimateBeneficiaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.SSITemplate.UltimateBeneficiary.BICorABA));
            if (isBicUltimateAvailable)
                mtMessage.addField(GetField58A(wire));
            else
                mtMessage.addField(GetField58D(wire));
        }

        private static void SetField59X(AbstractMT mtMessage, WireTicket wire)
        {
            var isBicUltimateAvailable = wire.IsFundTransfer ? (wire.ReceivingAccount.UltimateBeneficiaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.ReceivingAccount.UltimateBeneficiary.BICorABA)) : (wire.SSITemplate.UltimateBeneficiaryType != "ABA" && !string.IsNullOrWhiteSpace(wire.SSITemplate.UltimateBeneficiary.BICorABA));
            if (isBicUltimateAvailable)
                mtMessage.addField(GetField59A(wire));
            else
                mtMessage.addField(GetField59(wire));
        }


        private static Field56A GetField56A(WireTicket wire)
        {
            var f56A = new Field56A()
                .setAccount(wire.IsFundTransfer ? wire.ReceivingAccount.IntermediaryAccountNumber : wire.SSITemplate.IntermediaryAccountNumber)
                .setBIC(wire.IsFundTransfer ? wire.ReceivingAccount.IntermediaryType == "ABA" ? string.Empty :
                        wire.ReceivingAccount.Intermediary.BICorABA : wire.SSITemplate.IntermediaryType == "ABA" ? string.Empty : wire.SSITemplate.Intermediary.BICorABA);
            return f56A;
        }

        private static Field56D GetField56D(WireTicket wire)
        {
            var interBicOrAba = wire.IsFundTransfer
                ? wire.ReceivingAccount.IntermediaryType == "ABA" ? wire.ReceivingAccount.Intermediary.BICorABA : string.Empty
                : wire.SSITemplate.IntermediaryType == "ABA" ? wire.SSITemplate.Intermediary.BICorABA : string.Empty;

            var f56D = new Field56D().setAccount(!string.IsNullOrWhiteSpace(interBicOrAba) ? $"/FW{interBicOrAba}" : string.Empty);

            if (string.IsNullOrWhiteSpace(interBicOrAba))
                return f56D;

            var nameAndAddressed = wire.IsFundTransfer
                ? $"{wire.ReceivingAccount.Intermediary.BankName}\n{wire.ReceivingAccount.Intermediary.BankAddress}"
                : $"{wire.SSITemplate.Intermediary.BankName}\n{wire.SSITemplate.Intermediary.BankAddress}";

            f56D.setNameAndAddress(nameAndAddressed);
            return f56D;
        }

        private static Field57A GetField57A(WireTicket wire)
        {
            var f57A = new Field57A()
                .setAccount(wire.IsFundTransfer ? wire.ReceivingAccount.BeneficiaryAccountNumber : wire.SSITemplate.BeneficiaryAccountNumber)
                .setBIC(wire.IsFundTransfer ? wire.ReceivingAccount.BeneficiaryType == "ABA" ? string.Empty :
                    wire.ReceivingAccount.Beneficiary.BICorABA : wire.SSITemplate.BeneficiaryType == "ABA" ? string.Empty : wire.SSITemplate.Beneficiary.BICorABA);
            return f57A;
        }

        private static Field57D GetField57D(WireTicket wire)
        {
            var beneficiaryBicOrAba = wire.IsFundTransfer
                ? wire.ReceivingAccount.BeneficiaryType == "ABA" ? wire.ReceivingAccount.Beneficiary.BICorABA : string.Empty
                : wire.SSITemplate.BeneficiaryType == "ABA" ? wire.SSITemplate.Beneficiary.BICorABA : string.Empty;

            var f57D = new Field57D().setAccount(!string.IsNullOrWhiteSpace(beneficiaryBicOrAba) ? $"/FW{beneficiaryBicOrAba}"
                : string.Empty);

            if (string.IsNullOrWhiteSpace(beneficiaryBicOrAba))
                return f57D;

            var nameAndAddressed = wire.IsFundTransfer
                ? $"{wire.ReceivingAccount.Beneficiary.BankName}\n{wire.ReceivingAccount.Beneficiary.BankAddress}"
                : $"{wire.SSITemplate.Beneficiary.BankName}\n{wire.SSITemplate.Beneficiary.BankAddress}";

            f57D.setNameAndAddress(nameAndAddressed);

            return f57D;
        }

        private static Field58A GetField58A(WireTicket wire)
        {
            var f58A = new Field58A()
                .setAccount(wire.IsFundTransfer ? wire.ReceivingAccount.UltimateBeneficiaryAccountNumber : wire.SSITemplate.UltimateBeneficiaryAccountNumber)
                .setBIC(wire.IsFundTransfer ? wire.ReceivingAccount.UltimateBeneficiaryType == "ABA" ? string.Empty :
                    wire.ReceivingAccount.UltimateBeneficiary.BICorABA : wire.SSITemplate.UltimateBeneficiaryType == "ABA" ? string.Empty : wire.SSITemplate.UltimateBeneficiary.BICorABA);


            return f58A;
        }

        private static Field58D GetField58D(WireTicket wire)
        {
            var isAbaAvailable = wire.IsFundTransfer
                ? wire.ReceivingAccount.UltimateBeneficiaryType == "ABA" && !string.IsNullOrWhiteSpace(wire.ReceivingAccount.UltimateBeneficiary.BICorABA)
                : wire.SSITemplate.UltimateBeneficiaryType == "ABA" && !string.IsNullOrWhiteSpace(wire.SSITemplate.UltimateBeneficiary.BICorABA);

            var f58D = new Field58D()
                .setAccount(isAbaAvailable ? $"/FW{(wire.IsFundTransfer ? wire.ReceivingAccount.UltimateBeneficiary.BICorABA : wire.SSITemplate.UltimateBeneficiary.BICorABA)}"
                    : wire.IsFundTransfer ? wire.ReceivingAccount.UltimateBeneficiaryAccountNumber : wire.SSITemplate.UltimateBeneficiaryAccountNumber);

            if (!isAbaAvailable)
            {
                f58D.setNameAndAddress(wire.IsFundTransfer
                    ? $"{wire.ReceivingAccount.UltimateBeneficiaryAccountName}\n{wire.FundRegisterAddress}"
                    : $"{wire.SSITemplate.UltimateBeneficiaryAccountName}\n{wire.ReceivingSsiUltimateBeneAccountAddress}");
            }

            if (!isAbaAvailable)
                return f58D;

            var nameAndAddressed = wire.IsFundTransfer
                ? $"{wire.ReceivingAccount.UltimateBeneficiary.BankName}\n{wire.ReceivingAccount.UltimateBeneficiary.BankAddress}"
                : $"{wire.SSITemplate.UltimateBeneficiary.BankName}\n{wire.SSITemplate.UltimateBeneficiary.BankAddress}";

            f58D.setNameAndAddress(nameAndAddressed);

            return f58D;
        }

        private static Field59 GetField59(WireTicket wire)
        {
            var f59 = new Field59()
                .setAccount(wire.IsFundTransfer ? wire.ReceivingAccount.UltimateBeneficiaryAccountNumber : wire.SSITemplate.UltimateBeneficiaryAccountNumber)
                .setNameAndAddress(wire.IsFundTransfer
                    ? $"{wire.ReceivingAccount.UltimateBeneficiaryAccountName}\n{wire.FundRegisterAddress}"
                    : $"{wire.SSITemplate.UltimateBeneficiaryAccountName}\n{wire.ReceivingSsiUltimateBeneAccountAddress}");

            return f59;
        }

        private static Field59A GetField59A(WireTicket wire)
        {
            var f59A = new Field59A()
                .setAccount(wire.IsFundTransfer ? wire.ReceivingAccount.UltimateBeneficiaryAccountNumber : wire.SSITemplate.UltimateBeneficiaryAccountNumber)
                .setBIC(wire.IsFundTransfer ? wire.ReceivingAccount.UltimateBeneficiaryType == "ABA" ? string.Empty :
                    wire.ReceivingAccount.UltimateBeneficiary.BICorABA : wire.SSITemplate.UltimateBeneficiaryType == "ABA" ? string.Empty : wire.SSITemplate.UltimateBeneficiary.BICorABA);
            return f59A;
        }

        private static Field70 GetField70(WireTicket wire)
        {
            var f70 = new Field70();
            if (!wire.ShouldIncludeWirePurpose)
                return f70;

            f70.setNarrativeLine1("/RFB/" + (wire.HMWire.hmsWirePurposeLkup.ReportName == ReportName.Collateral ? wire.CollateralPaymentReason : wire.HMWire.hmsWirePurposeLkup.Purpose));
            return f70;
        }

        private static Field71A GetField71A(WireTicket wire)
        {
            return new Field71A("OUR");
        }

        public static Field72 GetField72(WireTicket wire, string messageType)
        {
            var f72 = new Field72();

            var ffcNumber = wire.IsFundTransfer
                    ? !string.IsNullOrWhiteSpace(wire.ReceivingAccount.FFCNumber)
                        ? wire.ReceivingAccount.FFCNumber
                        : string.Empty
                    : !string.IsNullOrWhiteSpace(wire.SSITemplate.FFCNumber)
                        ? wire.SSITemplate.FFCNumber
                        : string.Empty;


            var ffcName = wire.IsFundTransfer ? wire.ReceivingAccount.FFCName : wire.SSITemplate.FFCName;
            var reference = wire.IsFundTransfer ? wire.ReceivingAccount.Reference : wire.SSITemplate.Reference;

            if (string.IsNullOrWhiteSpace(ffcNumber) && string.IsNullOrWhiteSpace(wire.HMWire.SenderDescription) && (messageType == "MT103" || !wire.ShouldIncludeWirePurpose))
                return f72;

            var senderDescriptionInfo = (wire.HMWire.SenderDescription ?? string.Empty).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var descIndex = 0;
            senderDescriptionInfo.Where(s => s.Length > 33).ForEach(s => s = s.Substring(0, 33));
            var descriptionCount = senderDescriptionInfo.Count;

            var narrativeLine1 = wire.HMWire.hmsWirePurposeLkup.ReportName == ReportName.Collateral
                   ? $"/{wire.DefaultSenderInformation}/{(wire.ShouldIncludeWirePurpose ? wire.CollateralPaymentReason : wire.ShortFundName)}"
                   : $"/{wire.DefaultSenderInformation}/{(wire.ShouldIncludeWirePurpose ? wire.HMWire.hmsWirePurposeLkup.Purpose : ffcNumber)}";

            f72.setNarrativeLine1(narrativeLine1.Length > 30 ? narrativeLine1.Substring(0, 30) : narrativeLine1);

            string narrativeLine2;
            if (wire.HMWire.hmsWirePurposeLkup.ReportName == ReportName.Collateral)
                narrativeLine2 = $"{(wire.ShouldIncludeWirePurpose ? wire.ShortFundName : ffcNumber)}";
            else if (wire.ShouldIncludeWirePurpose)
                narrativeLine2 = $"{(!string.IsNullOrEmpty(ffcNumber) ? ffcNumber : descIndex < descriptionCount ? senderDescriptionInfo[descIndex++] : string.Empty)}";
            else
                narrativeLine2 = $"{(!string.IsNullOrEmpty(ffcName) ? ffcName : descIndex < descriptionCount ? senderDescriptionInfo[descIndex++] : string.Empty)}";

            if (!string.IsNullOrWhiteSpace(narrativeLine2.Trim()))
                f72.setNarrativeLine2($"//{(narrativeLine2.Length > 33 ? narrativeLine2.Substring(0, 33) : narrativeLine2)}");

            var narrativeLine3 = wire.ShouldIncludeWirePurpose
                 ? $"{(!string.IsNullOrEmpty(ffcName) ? ffcName : descIndex < descriptionCount ? senderDescriptionInfo[descIndex++] : string.Empty)}"
                 : $"{(!string.IsNullOrEmpty(reference) ? reference : descIndex < descriptionCount ? senderDescriptionInfo[descIndex++] : string.Empty)}";

            if (!string.IsNullOrWhiteSpace(narrativeLine3.Trim()))
                f72.setNarrativeLine3($"//{narrativeLine3}");

            var narrativeLine4 = wire.ShouldIncludeWirePurpose
                ? $"{(!string.IsNullOrEmpty(reference) ? ffcName : descIndex < descriptionCount ? senderDescriptionInfo[descIndex++] : string.Empty)}"
                : $"{(descIndex < descriptionCount ? senderDescriptionInfo[descIndex++] : string.Empty)}";

            if (!string.IsNullOrWhiteSpace(narrativeLine4))
                f72.setNarrativeLine4($"//{narrativeLine4}");

            var narrativeLine5 = $"{(descIndex < descriptionCount ? senderDescriptionInfo[descIndex++] : string.Empty)}";

            if (!string.IsNullOrWhiteSpace(narrativeLine5.Trim()))
                f72.setNarrativeLine5($"//{narrativeLine5}");

            var narrativeLine6 = $"{(descIndex < descriptionCount ? senderDescriptionInfo[descIndex++] : string.Empty)}";

            if (!string.IsNullOrWhiteSpace(narrativeLine6.Trim()))
                f72.setNarrativeLine6($"//{narrativeLine6}");

            return f72;
        }

        private static void SetSenderAndReceiverFromHM(AbstractMT callingMethod, WireTicket wire)
        {
            callingMethod.setSenderAndReceiver(HMBICSender, wire.SendingAccount.SwiftGroup.SendersBIC);
        }

        private static MT103 CreateMt103(WireTicket wire, string messageType, WireReferenceTag referenceTag)
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

            SetField59X(mt103, wire);

            //mt103.addField(GetField59(wire));

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

            SetField52X(mt202, wire, false);

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
            SetField52X(mt202Cov, wire, false);

            ////Optional
            mt202Cov.addField(GetField53B(wire));

            //Optional
            SetField56X(mt202Cov, wire);

            //Optional
            SetField57X(mt202Cov, wire);

            //Optional
            SetField58X(mt202Cov, wire);

            mt202Cov.addField(GetField50K(wire));

            //mt202Cov.addField(GetField59(wire));
            SetField59X(mt202Cov, wire);

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

            //SetField50X(mt210, wire);
         
            SetField52X(mt210, wire, true);

            //Optional
            //mt210.addField(GetField56A(wire));

            return mt210;
        }

        private static MT192 CreateMt192(WireTicket wire, string originalMessageType)
        {
            var mt192 = new MT192();
            SetSenderAndReceiverFromHM(mt192, wire);

            mt192.addField(GetField20(wire, WireReferenceTag.CANC));

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

            mt292.addField(GetField20(wire, WireReferenceTag.CANC));

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

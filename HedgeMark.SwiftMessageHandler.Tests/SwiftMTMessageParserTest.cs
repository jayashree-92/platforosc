using System;
using HedgeMark.SwiftMessageHandler.Model;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT.MT9XX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HedgeMark.SwiftMessageHandler.Tests
{
    [TestClass]
    public class SwiftMtMessageParserTest
    {

        [TestMethod]
        public void ParseMt940FromStringTest1()
        {
            var msg = "{1:F01AAAABB99BSMK3513951576}" +
                         "{2:O9400934081223BBBBAA33XXXX03592332770812230834N}" +
                         "{4:\n" +
                         ":20:0112230000000890\n" +
                         ":25:SAKG800030155USD\n" +
                         ":28C:255/1\n" +
                         ":60F:C011223USD175768,92\n" +
                         ":61:0112201223CD110,92NDIVNONREF//08 IL053309\n" +
                         "/GB/2542049/SHS/312,\n" +
                         ":62F:C011021USD175879,84\n" +
                         ":20:NONREF\n" +
                         ":25:4001400010\n" +
                         ":28C:58/1\n" +
                         ":60F:C140327EUR6308,75\n" +
                         ":61:1403270327C3519,76NTRF50RS201403240008//2014032100037666\n" +
                         "ABC DO BRASIL LTDA\n" +
                         ":86:INVOICE NR. 6000012801 \n" +
                         "ORDPRTY : ABC DO BRASIL LTDA RUA LIBERO BADARO,293-SAO \n" +
                         "PAULO BRAZIL }";

            /*
		 * Parse the String content into a SWIFT message object
		 */
            var mt = new MT940().parse(msg);

            Assert.AreEqual(mt.GetReceiver(), "AAAABB99BSMK");
            Assert.AreEqual(mt.GetSender(), "BBBBAA33XXXX");
            Assert.AreEqual(mt.GetMessageType(), "940");

            Field20 f = (Field20)mt.GetField("20");
            var labelInfo = f.Label + ": " + f.Value;

            Assert.AreEqual(labelInfo, "Transaction Reference Number: 0112230000000890");

            var f61 = (Field61)mt.GetField("61");

            int iteration = 0;
            foreach (var comp in f61.Components)
            {
                var fieldAmount = f61.GetComponentValue(FieldConstants.AMOUNT);
                Assert.AreEqual(fieldAmount, iteration == 0 ? "110,92" : "3519,76");

                var fieldTransaction = f61.GetComponentValue(FieldConstants.TRANSACTION_TYPE);
                Assert.AreEqual(fieldTransaction, "N");

                var fieldIdentification = f61.GetComponentValue(FieldConstants.IDENTIFICATION_CODE);
                Assert.AreEqual(fieldIdentification, iteration == 0 ? "DIV" : "TRF");

                var fieldReference = f61.GetComponentValue(FieldConstants.REFERENCE_FOR_THE_ACCOUNT_OWNER);
                Assert.AreEqual(fieldReference, iteration == 0 ? "NONREF" : "50RS201403240008");

                iteration++;
            }
        }

        [TestMethod]
        public void ParseMessageWithAckTest()
        {
            string fin = "{1:F21FOOLHKH0AXXX0304009999}{4:{177:1608140809}{451:0}}{1:F01FOOLHKH0AXXX0304009999}{2:O9401609160814FOOLHKH0AXXX03040027341608141609N}{4:\n" +
                               ":20:USD940NO1\n" +
                               ":21:123456/DEV\n" +
                               ":25:USD234567\n" +
                               ":28C:1/1\n" +
                               ":60F:C160418USD672,\n" +
                               ":61:160827C642,S1032\n" +
                               ":86:ANDY\n" +
                               ":61:160827D42,S1032\n" +
                               ":86:BANK CHARGES\n" +
                               ":62F:C160418USD1872,\n" +
                               ":64:C160418USD1872,\n" +
                               "-}{5:{CHK:0FEC1E4AEC53}{TNG:}}{S:{COP:S}}";

            SwiftMessage sm = SwiftMessage.Parse(fin);

            Assert.IsTrue(sm.IsAck());
            Assert.IsTrue(sm.IsServiceMessage21());

            Assert.AreEqual(sm.MessageType, "940");
            Assert.AreEqual(sm.Block2.MessageType, "940");
            Assert.AreEqual(sm.UnderlyingOriginalSwiftMessage.GetFieldValue("20"), "USD940NO1");
            //  Assert.AreEqual(sm.GetFieldValue("20"), "USD940NO1");
            if (sm.IsType("940"))
            {
                /*
                 * Specialize the message to its specific model representation
                 */
                MT940 mt = new MT940();
                mt.SetMessage(sm);

                Assert.AreEqual(mt.GetFieldValue("20"), "USD940NO1");
            }
        }

        /// <summary>
        /// http://api.prowidesoftware.com/core/com/prowidesoftware/swift/model/SwiftMessage.html
        /// </summary>
        [TestMethod]
        public void ParseMessageWithNAckTest()
        {
            string fin = "{1:F21FOOLHKH0AXXX0304009999}{4:{177:1608140809}{451:1}{405:T27}}{1:F01FOOLHKH0AXXX0304009999}{2:O9401609160814FOOLHKH0AXXX03040027341608141609N}{4:\n" +
                         ":20:USD940NO1\n" +
                         ":21:123456/DEV\n" +
                         ":25:USD234567\n" +
                         ":28C:1/1\n" +
                         ":60F:C160418USD672,\n" +
                         ":61:160827C642,S1032\n" +
                         ":86:ANDY\n" +
                         ":61:160827D42,S1032\n" +
                         ":86:BANK CHARGES\n" +
                         ":62F:C160418USD1872,\n" +
                         ":64:C160418USD1872,\n" +
                         "-}{5:{CHK:0FEC1E4AEC53}{TNG:}}{S:{COP:S}}";

            SwiftMessage sm = SwiftMessage.Parse(fin);

            Assert.IsTrue(sm.IsNack());

            if (sm.IsNack())
                Assert.AreEqual(sm.GetFieldValue("405"), "T27");

            Assert.IsTrue(sm.IsServiceMessage21());

            Assert.AreEqual(sm.MessageType, "940");
            Assert.AreEqual(sm.Block2.MessageType, "940");
            Assert.AreEqual(sm.UnderlyingOriginalSwiftMessage.Block4.GetFieldValue("20"), "USD940NO1");

            if (sm.IsType("940"))
            {
                /*
                 * Specialize the message to its specific model representation
                 */
                MT940 mt = new MT940();
                mt.SetMessage(sm);
                Assert.AreEqual(mt.GetFieldValue("20"), "USD940NO1");
            }
        }

        /// <summary>
        /// http://api.prowidesoftware.com/core/com/prowidesoftware/swift/model/mt/AckSystemMessage.html
        /// </summary>
        [TestMethod]
        public void AckSystemMessageTest()
        {
            var fin = "{1:F21FOOLHKH0AXXX0304009999}{4:{177:1608140809}{451:1}{405:T27}}{1:F01FOOLHKH0AXXX0304009999}{2:O9401609160814FOOLHKH0AXXX03040027341608141609N}{4:\n" +
                         ":20:USD940NO1\n" +
                         ":21:123456/DEV\n" +
                         ":25:USD234567\n" +
                         ":28C:1/1\n" +
                         ":60F:C160418USD672,\n" +
                         ":61:160827C642,S1032\n" +
                         ":86:ANDY\n" +
                         ":61:160827D42,S1032\n" +
                         ":86:BANK CHARGES\n" +
                         ":62F:C160418USD1872,\n" +
                         ":64:C160418USD1872,\n" +
                         "-}{5:{CHK:0FEC1E4AEC53}{TNG:}}{S:{COP:S}}";

            var ackMessgae = SwiftMessage.Parse(fin);
            Assert.AreEqual(ackMessgae.GetNackReasonCode(), "T27");

            Assert.IsTrue(ackMessgae.IsNack());
            Assert.IsFalse(ackMessgae.IsAck());

        }
        [TestMethod]
        public void AckMessageComparerTest()
        {
            var original = SwiftMessage.Parse("{1:F01FOOLHKH0AXXX0304009999}{2:O9401609160814FOOLHKH0AXXX03040027341608141609N}{4:\n" +
                                              ":20:USD940NO1\n" +
                                              ":21:123456/DEV\n" +
                                              ":25:USD234567\n" +
                                              ":28C:1/1\n" +
                                              ":60F:C160418USD672,\n" +
                                              ":61:160827C642,S1032\n" +
                                              ":86:ANDY\n" +
                                              ":61:160827D42,S1032\n" +
                                              ":86:BANK CHARGES\n" +
                                              ":62F:C160418USD1872,\n" +
                                              ":64:C160418USD1872,\n" +
                                              "-}{5:{CHK:0FEC1E4AEC53}{TNG:}}{S:{COP:S}}");

            var acknowledged = SwiftMessage.Parse("{1:F21FOOLHKH0AXXX0304009999}{4:{177:1608140809}{451:1}{405:T27}}{1:F01FOOLHKH0AXXX0304009999}{2:O9401609160814FOOLHKH0AXXX03040027341608141609N}{4:\n" +
                                                  ":20:USD940NO1\n" +
                                                  ":21:123456/DEV\n" +
                                                  ":25:USD234567\n" +
                                                  ":28C:1/1\n" +
                                                  ":60F:C160418USD672,\n" +
                                                  ":61:160827C642,S1032\n" +
                                                  ":86:ANDY\n" +
                                                  ":61:160827D42,S1032\n" +
                                                  ":86:BANK CHARGES\n" +
                                                  ":62F:C160418USD1872,\n" +
                                                  ":64:C160418USD1872,\n" +
                                                  "-}{5:{CHK:0FEC1E4AEC53}{TNG:}}{S:{COP:S}}");

            if (acknowledged.IsAck() || acknowledged.IsNack())
            {
                var originalMessage = SwiftMessage.Parse(acknowledged.UnderlyingOriginalFINMessage).GetMessage();
            }

            //var comparator = new AckMessageComparator();
            //Assert.AreEqual(comparator.compare(acknowledged, original), 0);

        }


        [TestMethod]
        public void AckMessageForEmxTest()
        {
            var message = @"{1:F01IRVTBEBBAXXX0000000000}{2:I950BLKSUS33XANEN}{4: 
:20:1111111111111111
:25:4444444444
:28C:1269/1
:60F:C180717EUR0,
:62F:C180718EUR0,
:64:C180718EUR0,-}{1:F21IRVTBEBBVXXX1234222222}{4:{177:3333333333}{451:0}}";


            var swiftMsg = SwiftMessage.Parse(message);

            Assert.IsTrue(swiftMsg.IsAck());
            Assert.IsTrue(swiftMsg.IsServiceMessage21());
            Assert.IsFalse(swiftMsg.IsNack());


            Assert.AreEqual(swiftMsg.MessageType, "950");
            Assert.AreEqual(swiftMsg.Block2.MessageType, "950");
            Assert.AreEqual(swiftMsg.UnderlyingOriginalSwiftMessage.GetFieldValue("20"), "1111111111111111");
            Assert.AreEqual(swiftMsg.UnderlyingOriginalSwiftMessage.Block4.GetFieldValue("25"), "4444444444");
            Assert.AreEqual(swiftMsg.UnderlyingOriginalSwiftMessage.Block4.GetFieldValue("28C"), "1269/1");
            Assert.AreEqual(swiftMsg.UnderlyingOriginalSwiftMessage.Block4.GetFieldValue("60F"), "C180717EUR0,");
            Assert.AreEqual(swiftMsg.UnderlyingOriginalSwiftMessage.Block4.GetFieldValue("62F"), "C180718EUR0,");
            Assert.AreEqual(swiftMsg.UnderlyingOriginalSwiftMessage.Block4.GetFieldValue("64"), "C180718EUR0,");
        }

        [TestMethod]
        public void AckMessageForEmxTest2()
        {
            var message = @"{1:F01BICFOOYYAXXX1234123456}{2:I103BICFOARXXXXXN1}{3:{119:STP}}{4:
:20:REFERENCE
:23B:CRED
:32A:" + DateTime.Today.ToString("yyMMdd") + @"EUR1234567,
:50A:/12345678901234567890
FOOBANKXXXXX
:59:/12345678901234567890
JOE DOE
:71A:OUR
-}{5:{MAC:00000000}{PDE:}}{S:{SAC:}{COP:P}}{1:F21BICFOOYYAXXX1234222222}{4:{177:3333333333}{451:0}}";

            var swiftMsg = SwiftMessage.Parse(message);
            Assert.IsTrue(swiftMsg.IsServiceMessage21());
            Assert.IsTrue(swiftMsg.IsAck());
            Assert.IsFalse(swiftMsg.IsNack());


            Assert.AreEqual(swiftMsg.MessageType, "103STP");
            Assert.AreEqual(swiftMsg.Block2.MessageType, "103");
            Assert.AreEqual(swiftMsg.UnderlyingOriginalSwiftMessage.Block4.GetFieldValue("20"), "REFERENCE");

        }
        [TestMethod]
        public void AckMessageForEmxTest3()
        {
            var message = @"{1:F01IRVTBEB0XXXX1002100002}{2:I202BNPAUS30PBSXN}{3:{121:cb8ed355-10e8-40b5-a26b-f8f90626f651}}{4:
:20:TESTDMOTRN79
:21:BNP
:32A:190404USD20000,
:58A:/465446564654
BNPAUS30PBS
:72:/INS/BNP FFC
-}{1:F21IRVTBEB07XXX2778158385}{4:{177:1904041219}{451:1}{405:H50}} 
";

            var swiftMsg = SwiftMessage.Parse(message);
            Assert.IsTrue(swiftMsg.IsServiceMessage21());
            Assert.IsFalse(swiftMsg.IsAck());
            Assert.IsTrue(swiftMsg.IsNack());

            Assert.AreEqual(swiftMsg.MessageType, "202");
            Assert.AreEqual(swiftMsg.Block2.MessageType, "202");
            Assert.AreEqual(swiftMsg.UnderlyingOriginalSwiftMessage.Block4.GetFieldValue("20"), "TESTDMOTRN79");

        }


        [TestMethod]
        public void AckMessageInboundParserTest()
        {
            var message = @"{1:F01HMRKUS30XXXX1010100010}{2:I202IRVTBEB0XXXXN}{3:{121:cb38bd34-99bc-4c6e-b464-36eb49386123}}{4:
:20:TESTDMOTRN93
:21:050129287
:32A:190408USD11000,
:58A:/40616408XXXX
GSILGB2XXXX
:72:/INS/Goldman Sachs
-}{1:F21HMRKUS30LXXX0013000006}{4:{177:1904080905}{451:0}} 
";

            var confirmationData = HMOSecureMiddleware.SwiftMessageManager.SwiftMessageParser.ParseMessage(message);
            Assert.IsTrue(confirmationData.IsAckOrNack);

        }

        [TestMethod]
        public void AckMessageInboundParserTest2()
        {
            var message = @"{1:F01HMRKUS30XXXX1001100001}{2:I103IRVTBEB0XXXXN}{3:{121:fc4d9690-ffe6-405d-af33-3b7dc16a4503}}{4:
:20:TESTDMOTRN116
:23B:CRED
:32A:190410USD1000000,
:50K:/314908818400
GS
:52A:/314908818400
IRVTBEB0
:57D:/221000089XXX
CITIBANK N.A.
:59:/314908818400
BNY Mellon
:71A:BEN
-}";

            var formattedMessage = SwiftMessageInterpreter.GetDetailedFormatted(message).Trim();

        }

    }
}

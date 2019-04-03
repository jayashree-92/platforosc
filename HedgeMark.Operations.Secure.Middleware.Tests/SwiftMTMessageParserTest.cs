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
            var labelInfo = f.Label +": " + f.Value;

            Assert.AreEqual(labelInfo, "Transaction Reference Number: 0112230000000890");

            var f61 = (Field61)mt.GetField("61");

            int iteration = 0;
            foreach (var comp in f61.Components)
            {
                var fieldAmount = f61.GetComponentValue(FieldConstants.AMOUNT);
                Assert.AreEqual(fieldAmount, iteration == 0 ? "110,92" : "3519,76");

                var fieldTransaction =  f61.GetComponentValue(FieldConstants.TRANSACTION_TYPE);
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
            //Assert.IsTrue(sm.isServiceMessage());

            //if (sm.isServiceMessage())
            //{
            //    sm = SwiftMessage.parse(sm.getUnparsedTexts().getAsFINString());
            //}
            //at this point the sm variable will contain the actual user to user message, regardless if it was preceded by and ACK.

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
    }
}

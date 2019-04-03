using System;
using HedgeMark.SwiftMessageHandler.Model.Blocks;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT.MT1XX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HedgeMark.SwiftMessageHandler.Tests
{
    [TestClass]
    public class SwiftMtMessageCreationTest
    {
        [TestMethod]
        public void CreateMessageMt103WithFieldsTest1()
        {

            var m = new MT103();
            m.setSender("FOOSEDR0AXXX");
            m.setReceiver("FOORECV0XXXX");

            m.addField(new Field20("REFERENCE"));
            m.addField(new Field23B("CRED"));

            Field32A f32A = new Field32A()
                .setDate(new DateTime(2019, 02, 28))
                .setCurrency("EUR")
                .setAmount((decimal)1234567.89);
            m.addField(f32A);

            Field50A f50A = new Field50A()
                .setAccount("12345678901234567890")
                .setBIC("FOOBANKXXXXX");
            m.addField(f50A);

            Field59 f59 = new Field59()
                .setAccount("12345678901234567890")
                .setNameAndAddress("JOE DOE");
            m.addField(f59);

            m.addField(new Field71A("OUR"));

            m.Block3.AddField(new Field121().setUniqueReference("guid-123-guid"));

            var finalMessage = m.GetMessage();

            Assert.AreEqual(finalMessage, "{1:F01FOOSEDR0AXXX" + m.Block1.GetSessionNo() + m.Block1.GetSequenceNo() + "}{2:I103FOORECV0XXXXN}{3:{121:guid-123-guid}}{4:\r\n:20:REFERENCE\r\n:23B:CRED\r\n:32A:190228EUR1234567,89\r\n:50A:/12345678901234567890\r\nFOOBANKXXXXX\r\n:59:/12345678901234567890\r\nJOE DOE\r\n:71A:OUR\r\n-}");
        }

        //HedgeMark Swift framework currently will not support construction using sequence
        //        [TestMethod]
        //        public void CreateMessageMt542WithSequenceTest2()
        //        {
        //            /*
        //          * Create the MT class, it will be initialized as an outgoing message
        //          * with normal priority
        //          */
        //            MT542 m = new MT542();

        //            /*
        //             * Set sender and receiver BIC codes
        //             */
        //            m.setSender("FOOSEDR0AXXX");
        //            m.setReceiver("FOORECV0XXXX");

        //            /*
        //             * Add a field using comprehensive setters API, will use it later inside
        //             * sequence A
        //             */
        //            Field98A f98A = new Field98A()
        //                .setQualifier("PREP")
        //                .setDate(Calendar.getInstance());

        //            /*
        //             * Start adding the message's fields in correct order, starting with
        //             * general information sequence
        //             */
        //            MT542.SequenceA A = MT542.SequenceA.newInstance(
        //            /*
        //             * Add field using the complete literal value
        //             */
        //            Field20C.tag(":SEME//2005071800000923"),
        //            Field23G.tag("NEWM"),
        //            f98A.asTag());
        //            /*
        //             * Add sequence A to message
        //             */
        //            m.append(A);

        //            /*
        //             * trade details sequence B
        //             */
        //            m.append(MT542.SequenceB.newInstance(
        //                Field98A.tag(":TRAD//20050714"),
        //                Field98A.tag(":SETT//20050719"),
        //                Field90B.tag(":DEAL//ACTU/EUR21,49"),
        //                    Field35B.tag("ISIN FR1234567890" + FINWriterVisitor.SWIFT_EOL + "AXA UAP"),
        //                    Field70E.tag(":SPRO//4042")));

        //            /*
        //             * financial instrument account sequence C
        //             */
        //            m.append(MT542.SequenceC.newInstance(
        //                Field36B.tag(":SETT//UNIT/200,00"),
        //                Field97A.tag(":SAFE//123456789")));

        //            /*
        //             * settlement details: sequence E
        //             */
        //            m.append(MT542.SequenceE.START_TAG); // use constant of Tag that marks
        //                                                 // start of sequence

        //            m.append(Field22F.tag(":SETR//TRAD"));

        //            m.append(MT542.SequenceE1.newInstance(Field95R.tag(":DEAG/SICV/4042")));

        //            m.append(MT542.SequenceE1.newInstance(Field95P.tag(":SELL//CITIFRPP"), Field97A.tag(":SAFE//123456789")));

        //            m.append(MT542.SequenceE1.newInstance(Field95P.tag(":PSET//SICVFRPP")));

        //            m.append(MT542.SequenceE3.newInstance(Field19A.tag(":SETT//EUR123456,50")));

        //            m.append(MT542.SequenceE.END_TAG); // use constant of Tag that marks end
        //                                               // of sequence

        //            m.append(new Field20("REFERENCE"));
        //            m.append(new Field23B("CRED"));

        //            Assert.AreEqual(m.message(), @"{1:F01FOOSEDR0AXXX0000000000}{2:I542FOORECV0XXXXN}{4:
        //:16R:GENL
        //:20C::SEME//2005071800000923
        //:23G:NEWM
        //:98A::PREP//" + DateTime.Today.ToString("yyyyMMdd") + @"
        //:16S:GENL
        //:16R:TRADDET
        //:98A::TRAD//20050714
        //:98A::SETT//20050719
        //:90B::DEAL//ACTU/EUR21,49
        //:35B:ISIN FR1234567890
        //AXA UAP
        //:70E::SPRO//4042
        //:16S:TRADDET
        //:16R:FIAC
        //:36B::SETT//UNIT/200,00
        //:97A::SAFE//123456789
        //:16S:FIAC
        //:16R:SETDET
        //:22F::SETR//TRAD
        //:16R:SETPRTY
        //:95R::DEAG/SICV/4042
        //:16S:SETPRTY
        //:16R:SETPRTY
        //:95P::SELL//CITIFRPP
        //:97A::SAFE//123456789
        //:16S:SETPRTY
        //:16R:SETPRTY
        //:95P::PSET//SICVFRPP
        //:16S:SETPRTY
        //:16R:AMT
        //:19A::SETT//EUR123456,50
        //:16S:AMT
        //:16S:SETDET
        //:20:REFERENCE
        //:23B:CRED
        //-}");


        //        }

        [TestMethod]
        public void CreateMessageMt103WithBlocksTest3()
        {
            /*
		 * Create the MT class, it will be initialized as an outgoing message
		 * with normal priority
		 */
            var m = new MT103();

            /*
             * Create and set a specific header block 1 
             */
            SwiftBlock1 b1 = new SwiftBlock1
            {
                AppId = "F",
                ServiceId = "01",
                SessionNo = "1234",
                SequenceNo = "123456"
            };
            b1.SetSender("BICFOOYYAXXX");
            m.AddBlock(b1);

            /*
             * Create and set a specific header block 2
             * Notice there are two block 2 headers (for input and output messages)
             */
            SwiftBlock2 b2 = new SwiftBlock2();
            b2.Receiver = "BICFOARXXXXX";
            b2.DeliveryMonitoring = "1";
            m.AddBlock(b2);

            /*
             * Add the optional user header block
             */
            SwiftBlock3 block3 = new SwiftBlock3();
            block3.AddField(new Field119("STP"));
            m.AddBlock(block3);


            /*
             * Start adding the message's fields in correct order
             */
            m.addField(new Field20("REFERENCE"));
            m.addField(new Field23B("CRED"));

            /*
             * Add a field using comprehensive setters API
             */
            Field32A f32A = new Field32A()
                .setDate(DateTime.Today)
                .setCurrency("EUR")
                .setAmount((decimal)1234567);
            m.addField(f32A);

            /*
             * Add the orderer field
             */
            Field50A f50A = new Field50A()
                .setAccount("12345678901234567890")
                .setBIC("FOOBANKXXXXX");
            m.addField(f50A);

            /*
             * Add the beneficiary field
             */
            Field59 f59 = new Field59()
                .setAccount("12345678901234567890")
                .setNameAndAddress("JOE DOE");
            m.addField(f59);

            /*
             * Add the commission indication
             */
            m.addField(new Field71A("OUR"));


            /*
             * Add the trailer block (in normal situations this is automatically created by the FIN interface, not by the user/applications)
             */
            SwiftBlock5 block5 = new SwiftBlock5();
            block5.AddField(new Field("MAC").setValue("00000000"));
            block5.AddField(new Field("PDE").setValue(""));

            m.AddBlock(block5);

            /*
             * Add an optional user block
             */
            SwiftBlockUser blockUser = new SwiftBlockUser("S");
            blockUser.AddField(new Field("SAC").setValue(""));
            blockUser.AddField(new Field("COP").setValue("P"));

            m.AddBlock(blockUser);

            Assert.AreEqual(m.GetMessage(), @"{1:F01BICFOOYYAXXX1234123456}{2:I103BICFOARXXXXXN1}{3:{119:STP}}{4:
:20:REFERENCE
:23B:CRED
:32A:" + DateTime.Today.ToString("yyMMdd") + @"EUR1234567,
:50A:/12345678901234567890
FOOBANKXXXXX
:59:/12345678901234567890
JOE DOE
:71A:OUR
-}{5:{MAC:00000000}{PDE:}}{S:{SAC:}{COP:P}}");
        }
    }
}

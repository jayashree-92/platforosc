using System;
using System.Linq;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT.MT1XX;
using NUnit.Framework;

namespace HedgeMark.SwiftMessageHandler.Tests
{
    [TestFixture]
    public class SwiftMTMessageCurrencyUnitsTest
    {
        [Test]
        public void CurrencyUnitTestBasedOnConfiguration()
        {
            Assert.IsTrue(FieldWithCurrencyAndAmount.CurrencyToDecimalPoints.Any());

            foreach (var currencyToDecimalPoint in FieldWithCurrencyAndAmount.CurrencyToDecimalPoints)
            {
                var m = new MT103();
                m.setSender("FOOSEDR0AXXX");
                m.setReceiver("FOORECV0XXXX");

                m.addField(new Field20("REFERENCE"));
                m.addField(new Field23B("CRED"));

                Field32A f32A = new Field32A()
                    .setDate(new DateTime(2019, 02, 28))
                    .setCurrency(currencyToDecimalPoint.Key)
                    .setAmount((decimal)1234567.89);
                m.addField(f32A);

                Field50C f50C = new Field50C()
                    .setAccount("12345678901234567890")
                    .setBIC("FOOBANKXXXXX");
                m.addField(f50C);

                Field59 f59 = new Field59()
                    .setAccount("12345678901234567890")
                    .setNameAndAddress("JOE DOE!@#$%^&");
                m.addField(f59);

                m.addField(new Field71A("OUR"));

                m.Block3.AddField(new Field121().setUniqueReference("guid-123-guid"));

                var finalMessage = m.GetMessage();

                var expectedAmount = $"190228{currencyToDecimalPoint.Key}1234567";

                if (currencyToDecimalPoint.Value == 0)
                    expectedAmount = $"{expectedAmount},";
                if (currencyToDecimalPoint.Value == 1)
                    expectedAmount = $"{expectedAmount},8";
                if (currencyToDecimalPoint.Value == 2)
                    expectedAmount = $"{expectedAmount},89";
                if (currencyToDecimalPoint.Value == 3)
                    expectedAmount = $"{expectedAmount},890";
                if (currencyToDecimalPoint.Value == 4)
                    expectedAmount = $"{expectedAmount},8900";
                if (currencyToDecimalPoint.Value == 5)
                    expectedAmount = $"{expectedAmount},89000";


                Assert.AreEqual(finalMessage, "{1:F01FOOSEDR0AXXX" + m.Block1.GetSessionNo() + m.Block1.GetSequenceNo() + "}{2:I103FOORECV0XXXXN}{3:{121:guid-123-guid}}{4:\r\n:20:REFERENCE\r\n:23B:CRED\r\n:32A:" + expectedAmount + "\r\n:50C:/12345678901234567890\r\nFOOBANKXXXXX\r\n:59:/12345678901234567890\r\nJOE DOE\r\n:71A:OUR\r\n-}");
            }

        }
    }
}

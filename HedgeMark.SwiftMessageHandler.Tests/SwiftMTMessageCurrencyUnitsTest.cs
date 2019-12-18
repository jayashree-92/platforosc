using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HedgeMark.SwiftMessageHandler.Model.Fields;
using HedgeMark.SwiftMessageHandler.Model.MT.MT1XX;
using HedgeMark.SwiftMessageHandler.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HedgeMark.SwiftMessageHandler.Tests
{
    [TestClass]
    public class SwiftMTMessageCurrencyUnitsTest
    {
        [TestMethod]
        public void CurrencyUnitTestBasedOnConfiguration()
        {
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

                Field50A f50A = new Field50A()
                    .setAccount("12345678901234567890")
                    .setBIC("FOOBANKXXXXX");
                m.addField(f50A);

                Field59 f59 = new Field59()
                    .setAccount("12345678901234567890")
                    .setNameAndAddress("JOE DOE!@#$%^&");
                m.addField(f59);

                m.addField(new Field71A("OUR"));

                m.Block3.AddField(new Field121().setUniqueReference("guid-123-guid"));

                var finalMessage = m.GetMessage();

                var expectedAmount = string.Format("190228{0}1234567", currencyToDecimalPoint.Key);

                if (currencyToDecimalPoint.Value == 0)
                    expectedAmount = string.Format("{0},", expectedAmount);
                if (currencyToDecimalPoint.Value == 1)
                    expectedAmount = string.Format("{0},8", expectedAmount);
                if (currencyToDecimalPoint.Value == 2)
                    expectedAmount = string.Format("{0},89", expectedAmount);
                if (currencyToDecimalPoint.Value == 3)
                    expectedAmount = string.Format("{0},890", expectedAmount);
                if (currencyToDecimalPoint.Value == 4)
                    expectedAmount = string.Format("{0},8900", expectedAmount);
                if (currencyToDecimalPoint.Value == 5)
                    expectedAmount = string.Format("{0},89000", expectedAmount);


                Assert.AreEqual(finalMessage, "{1:F01FOOSEDR0AXXX" + m.Block1.GetSessionNo() + m.Block1.GetSequenceNo() + "}{2:I103FOORECV0XXXXN}{3:{121:guid-123-guid}}{4:\r\n:20:REFERENCE\r\n:23B:CRED\r\n:32A:" + expectedAmount + "\r\n:50A:/12345678901234567890\r\nFOOBANKXXXXX\r\n:59:/12345678901234567890\r\nJOE DOE\r\n:71A:OUR\r\n-}");
            }

        }
    }
}

﻿using HedgeMark.SwiftMessageHandler.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HedgeMark.SwiftMessageHandler.Tests
{
    [TestClass]
    public class SwiftMtMessageFormatterTest
    {
        static readonly SwiftMessage SwiftMessage103 = SwiftMessage.Parse("{1:F01BACOARB1A0B20000000000}{2:I103ADRBNL21XXXXU2}{3:{108:FOOB3926BE868XXX}}{4:\n" +
                                              ":20:REFERENCE\n" +
                                              ":23B:CRED\n" +
                                              ":32A:180730USD1234567,89\n" +
                                              ":50A:/12345678901234567890\n" +
                                              "CFIMHKH1XXX\n" +
                                              ":59:/12345678901234567890\n" +
                                              "JOE DOE\n" +
                                              "MyStreet 1234\n" +
                                              ":71A:OUR\n" +
                                              "-}{5:{CHK:3916EF336FF7}}");


        static readonly SwiftMessage SwiftMessage202 = SwiftMessage.Parse("{1:F01ANASCH20AXXX0000000000}{2:O2021300050901IRVTLULXALTA06556102830509011300N}{3:{108:FOOB3926BE868XXX}}{4:\n" +
                                         ":20:RFSAMPPGN0031091\n" +
                                         ":21:RFSAMPPGN0031091\n" +
                                         ":13C:/RNCTIME/1356+0000\n" +
                                         ":13C:/RNCTIME/1410+0000\n" +
                                         ":32A:050901EUR19265,53\n" +
                                         ":52A:IRVTLULXLTA\n" +
                                         ":53A:/D/1234A0123456ABC012345\n" +
                                         "BICFOOYY\n" +
                                         ":54A:BAPPIT21AP8\n" +
                                         ":56A:BMISLBB1025\n" +
                                         ":57A:HLFXGB21L17\n" +
                                         ":58A:/ES12 1234 6789 1234 1111 1234\n" +
                                         "SEICUS33CAI\n" +
                                         ":72:/BNF/00002695 0001 2005083130110\n" +
                                         "-}{5:{CHK:3916EF336FF7}}");

        [TestMethod]
        public void TestSwiftMessageSimpleFormattingTest1()
        {
            var swiftMessage = @"{1:F01TESTUS00XXXX1001100001}{2:I202TESTUS0000XXXXN}{3:{121:84481ee1-ec02-451d-bbf9-a34f17ae8ad8}}{4:
:20:TEST35
:21:NOREF
:32A:190329USD5000
:56A:/5645754754
021001033
:57A:/2354235436
CITIUS33XXX
:58A:/38890774
MSNYUS33XXX
:72:1907 Penso Fund ltd-}";

            var formattedMessage = SwiftMessageInterpreter.GetSimpleFormatted(swiftMessage).Trim();

            Assert.AreEqual(@"20:  Transaction Reference Number
        Reference: TEST35
21:  Related Reference
        Reference: NOREF
32A: Value Date/Currency/Interbank Settled Amount
        Date: Mar 29, 2019
        Currency: USD
        Amount: 5,000.00
56A: Intermediary
        Account: 5645754754
        BIC: 021001033
57A: Account with Institution
        Account: 2354235436
        BIC: CITIUS33XXX
58A: Beneficiary Institution
        Account: 38890774
        BIC: MSNYUS33XXX
72:  Sender to Receiver Information
        Narrative: 1907 Penso Fund ltd", formattedMessage);
        }

        [TestMethod]
        public void TestSwiftMessageSimpleFormattingTest()
        {
            Assert.AreEqual(SwiftMessageInterpreter.GetSimpleFormatted(SwiftMessage103).Trim(), @"
20:  Transaction Reference Number
        Reference: REFERENCE
23B: Bank Operation Code
        Type: CRED
32A: Value Date/Currency/Interbank Settled Amount
        Date: Jul 30, 2018
        Currency: USD
        Amount: 1,234,567.89
50A: Ordering Customer
        Account: 12345678901234567890
        BIC: CFIMHKH1XXX
59:  Beneficiary Customer
        Account: 12345678901234567890
        Name and Address: JOE DOE
        Name and Address 2: MyStreet 1234
71A: Details of Charges
        Code: OUR".Trim());
        }



        [TestMethod]
        public void TestSwiftMessageDetailedFormatting103Test()
        {
            var formattedSwiftMessage = SwiftMessageInterpreter.GetDetailedFormatted(SwiftMessage103).Trim();

            Assert.AreEqual(formattedSwiftMessage, @"
------------------------- Instance Type and Transmission -------------------------
Copy sent to SWIFT
Priority/Delivery : Urgent/Delivery Notification
------------------------- Message Header -----------------------------------------
Swift    : MT 103
Sender   : BACOARB1A0B2
Receiver : ADRBNL21XXXX
MUR      : FOOB3926BE868XXX
------------------------- Message Text -------------------------------------------
20:  Transaction Reference Number
        Reference: REFERENCE
23B: Bank Operation Code
        Type: CRED
32A: Value Date/Currency/Interbank Settled Amount
        Date: Jul 30, 2018
        Currency: USD
        Amount: 1,234,567.89
50A: Ordering Customer
        Account: 12345678901234567890
        BIC: CFIMHKH1XXX
59:  Beneficiary Customer
        Account: 12345678901234567890
        Name and Address: JOE DOE
        Name and Address 2: MyStreet 1234
71A: Details of Charges
        Code: OUR
---------------------------- Message Trailer -------------------------------------
CHK: 3916EF336FF7
------------------------------ End Of Message ------------------------------------".Trim());
        }

        [TestMethod]
        public void TestSwiftMessageDetailedFormatting202Test()
        {
            var formattedSwiftMessage = SwiftMessageInterpreter.GetDetailedFormatted(SwiftMessage202).Trim();
            Assert.AreEqual(formattedSwiftMessage, @"
------------------------- Instance Type and Transmission -------------------------
Copy received from SWIFT
Priority/Delivery : Normal
Message Input Reference : IRVTLULXALTA0655610283
------------------------- Message Header -----------------------------------------
Swift    : MT 202
Sender   : IRVTLULXALTA
Receiver : ANASCH20AXXX
MUR      : FOOB3926BE868XXX
------------------------- Message Text -------------------------------------------
20:  Transaction Reference Number
        Reference: RFSAMPPGN0031091
21:  Related Reference
        Reference: RFSAMPPGN0031091
13C: Time Indication
        Code: RNCTIME
        Time: 13:56
        Sign: +
        Offset: 0000
13C: Time Indication
        Code: RNCTIME
        Time: 14:10
        Sign: +
        Offset: 0000
32A: Value Date/Currency/Interbank Settled Amount
        Date: Sep 01, 2005
        Currency: EUR
        Amount: 19,265.53
52A: Ordering Institution
        BIC: IRVTLULXLTA
53A: Sender's Correspondent
        D/C Mark: D
        Account: 1234A0123456ABC012345
        BIC: BICFOOYY
54A: Receiver's Correspondent
        BIC: BAPPIT21AP8
56A: Intermediary
        BIC: BMISLBB1025
57A: Account with Institution
        BIC: HLFXGB21L17
58A: Beneficiary Institution
        Account: ES12 1234 6789 1234 1111 1234
        BIC: SEICUS33CAI
72:  Sender to Receiver Information
        Narrative: /BNF/00002695 0001 2005083130110
---------------------------- Message Trailer -------------------------------------
CHK: 3916EF336FF7
------------------------------ End Of Message ------------------------------------".Trim());
        }



        //        [TestMethod]
        //        public void TestSwiftMessageFormattingTest()
        //        {
        //            Locale locale = Locale.getDefault();

        //            var builder = new StringBuilder();

        //            /*
        //             * With single value per field
        //             */
        //            builder.AppendLine("Sender: " + SwiftMessage103.getSender());
        //            builder.AppendLine("Receiver: " + SwiftMessage103.getReceiver() + "\n");

        //            foreach (Tag tag in SwiftMessage103.getBlock4().getTags().toArray())
        //            {
        //                var field = tag.asField();
        //                builder.AppendLine(Field.getLabel(field.getName(), "103", null));
        //                builder.AppendLine(field.getValueDisplay(locale) + "\n");
        //            }

        //            var singleValPerField = builder.ToString();

        //            Assert.AreEqual(singleValPerField.Trim(), @"Sender: BACOARB1A0B2
        //Receiver: ADRBNL21XXXX

        //Sender's Reference
        //REFERENCE

        //Bank Operation Code
        //CRED

        //Value Date/Currency/Interbank Settled Amount
        //Jul 30, 2018 USD 1,234,567.89

        //Ordering Customer
        //12345678901234567890 CFIMHKH1XXX

        //Beneficiary Customer
        //12345678901234567890 JOE DOE MyStreet 1234

        //Details of Charges
        //OUR".Trim());

        //            builder = new StringBuilder();
        //            foreach (Tag tag in SwiftMessage103.getBlock4().getTags().toArray())
        //            {
        //                var field = tag.asField();
        //                builder.AppendLine("\n" + Field.getLabel(field.getName(), "103", null));
        //                for (var component = 1; component <= field.componentsSize(); component++)
        //                {
        //                    if (field.getComponent(component) == null)
        //                        continue;

        //                    builder.Append(field.getComponentLabel(component) + ": ");
        //                    builder.AppendLine(field.getValueDisplay(component, locale));
        //                }
        //            }

        //            var detailedValPerField = builder.ToString();

        //            Assert.AreEqual(detailedValPerField.Trim(), @"Sender's Reference
        //Reference: REFERENCE

        //Bank Operation Code
        //Type: CRED

        //Value Date/Currency/Interbank Settled Amount
        //Date: Jul 30, 2018
        //Currency: USD
        //Amount: 1,234,567.89

        //Ordering Customer
        //Account: 12345678901234567890
        //BIC: CFIMHKH1XXX

        //Beneficiary Customer
        //Account: 12345678901234567890
        //Name And Address: JOE DOE
        //Name And Address 2: MyStreet 1234

        //Details of Charges
        //Code: OUR".Trim());
        //        }

    }
}

using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public static class FieldDirectory
    {
        /// <summary>
        /// For any message that the user submits to a FINCopy service, block 3 requires an additional field 103. 
        /// This field contains a 3-character service identifier that is unique to a specific FINCopy service. 
        /// The use of a unique identifier makes it possible to support access to multiple services within the same interface. The format of field tag 103 is 3!a.
        /// Remark for TGT2: If this field is not present, the message will be delivered directly to the receiver without processing in the Payments Module (PM). Present in MT 103, 202, 204. All other MT will not contain field 103.
        /// </summary>
        public const string FIELD_103 = "103";

        /// <summary>
        /// The sender of the message assigns the message user reference (MUR).
        /// If the sender has not defined the message user reference in field 108, then the system uses the transaction reference number for retrievals and associated system messages and acknowledgements.
        /// The transaction reference number is in field 20 or 20C::SEME of the text block of user-to-user FIN messages.Field 108 containing only blanks or spaces will be accepted by the system.
        /// </summary>
        public const string FIELD_108 = "108";

        /// <summary>
        /// The sender of the message assigns this 4-character banking priority. Field 113 containing only blanks or spaces will be accepted by the system.
        /// </summary>
        public const string FIELD_113 = "113";

        /// <summary>
        /// It Indicates whether FIN must perform a special validation.
        /// The following are examples of the values that this field may take :
        /// - REMIT identifies the presence of field 77T.To use only in MT 103.
        /// - RFDD indicates that the message is a request for direct debit.To use only in MT 104. See Error Code C94.
        /// - STP indicates that FIN validates the message according to straight-through processing principles.To use only in MTs 102 and 103.
        /// For more information, see Message Format Validation Rules in the SWIFT MT/MX standards.
        /// </summary>
        public const string FIELD_119 = "119";

        /// <summary>
        /// This field contains the balance checkpoint date and time consisting of a date (YYMMDD) and a time (HHMMSS[ss], where "ss" indicates hundredths of a second). 
        /// The market infrastructure that is subscribed to MIRS will always copy this reference from field tag 13G of the last MT 298/091 that it sent into the related MT 298/093 or MT 097 that it generates.
        /// </summary>
        public const string FIELD_423 = "423";

        /// <summary>
        /// This field contains the MIR of the payment message that is related to the notification messages in which this field is present. 
        /// The market infrastructure that is subscribed to Market Infrastructure Resiliency Service (MIRS) will copy it from the received payment message. In a FINCopy scenario, this field must contain the MIR of the related MT 096.
        /// Tag 106 or 424 must be present in block 3 of all notification messages which are sent by a market infrastructure that has subscribed to MIRS.
        /// Tag 106 must be present in block 3 of an MT 097 sent by a market infrastructure that has subscribed to MIRS.
        /// </summary>
        public const string FIELD_106 = "106";

        /// <summary>
        /// This field contains the reference of the payment that is related to the notification messages in which this field is present. 
        /// The payment itself was not the result of a FIN message. The value of this field must correspond to the related reference (field 21) in the MT 298/093. 
        /// The format of field 424 is 16x. Field 424 containing only blanks or spaces will be accepted by the system.
        /// Tag 106 or 424 must be present in block 3 of all notification messages which are sent by a market infrastructure that has subscribed to MIRS.
        /// </summary>
        public const string FIELD_424 = "424";

        /// <summary>
        /// This field identifies the applicable global payment service type.
        /// Field 121 can be present without field 111. Field 111 can only be present if field 121 is also present.
        /// </summary>
        public const string FIELD_111 = "111";


        /// <summary>
        /// This field provides an end-to-end reference across a payment transaction. 
        /// The format of field 121 is xxxxxxxx-xxxx-4xxx-yxxxxxxxxxxxxxxx where x is any hexadecimal character (lower case only) and y is one of 8, 9, a, or b. 
        /// Field 121 is mandatory on all MTs 103, 103 STP, 103 REMIT, 202, 202 COV, 205, and 205 COV. See the FIN Service Description for additional information.
        /// Field 121 can be present without field 111. Field 111 can only be present if field 121 is also present.
        /// </summary>
        public const string FIELD_121 = "121";

        /// <summary>
        /// The central institution inputs information in the MT 097 FINCopy Message Authorisation/Refusal Notification, in Y-Copy mode. FINCopy copies the information to the receiver of the payment message. Field 115 containing only blanks or spaces will be accepted by the system.
        /// It contains information from the PM:
        ///     - Time of crediting RTGS account of receiver
        ///     - Time of debiting RTGS account of sender
        ///     - Country code of sender
        ///     - SSP internal posting reference for unique identification
        ///
        /// Tag 115 is only valid for output messages.
        /// For more information, see the FINCopy Service Description ot Target 2 documentation.
        /// </summary>
        public const string FIELD_115 = "115";

        /// <summary>
        /// This field contains information from the server destination to the receiver of payment message.It is only available for use in FINInform services that operate in Y-copy mode.
        /// Tag 165 is only valid for output messages.
        /// </summary>
        public const string FIELD_165 = "165";

        /// <summary> 
        /// The screening service inputs information in the MT 097 FINCopy Message Authorisation/Refusal Notification, in Y-Copy mode.FINInform copies the information to the receiver of the screened message.
        /// The following values can be present in this field:
        ///     - AOK: message automatically released by screening service
        ///     - FPO: compliance officer has flagged the screening result as false positive
        ///     - NOK: compliance officer has flagged the screened message as suspect or the message was auto released by the service
        /// 
        /// The code word can optionally be followed by additional information (up to 20 characters from the x character set).
        /// Tag 433 is only valid for output messages.
        /// </summary>
        public const string FIELD_433 = "433";

        /// <summary>
        /// This field provides information to the receiver from the Payment Controls service about the screened message.
        /// Tag 434 is only valid for output messages.
        /// Remark: In previous SWIFT releases, this tag was called sanctions-screeningreconciliation-data
        /// </summary>
        public const string FIELD_434 = "434";

        /// <summary>
        /// "MT and Date of the Original Message"
        /// </summary>
        public const string FIELD_11S = "11S";

        /// <summary>
        /// Start of Block
        /// </summary>
        public const string FIELD_16R = "16R";

        /// <summary>
        /// "Transaction Reference Number"
        /// </summary>
        public const string FIELD_20 = "20";

        /// <summary>
        /// Reference
        /// </summary>
        public const string FIELD_20C = "20C";

        /// <summary>
        ///  "Related Reference"
        /// </summary>
        public const string FIELD_21 = "21";

        /// <summary>
        /// "Bank Operation Code"
        /// </summary>
        public const string FIELD_23B = "23B";

        /// <summary>
        /// Ultimate Fund Account
        /// </summary>
        public const string FIELD_25 = "25";

        /// <summary>
        /// "Value Date"
        /// </summary>
        public const string FIELD_30 = "30";

        /// <summary>
        /// "Value Date, Currency Code, Amount"
        /// </summary>
        public const string FIELD_32A = "32A";

        /// <summary>
        /// "Currency Code, Amount"
        /// </summary>
        public const string FIELD_32B = "32B";

        /// <summary>
        /// "Ordering Customer"
        /// </summary>
        public const string FIELD_50A = "50A";

        /// <summary>
        /// "Ordering Customer"
        /// </summary>
        public const string FIELD_50K = "50K";

        /// <summary>
        /// "Ordering Institution"
        /// </summary>
        public const string FIELD_52A = "52A";


        /// <summary>
        /// "Ordering Institution"
        /// </summary>
        public const string FIELD_52D = "52D";

        /// <summary>
        /// "Sender's Correspondent"
        /// </summary>
        public const string FIELD_53A = "53A";

        /// <summary>
        /// "Sender's Correspondent"
        /// </summary>
        public const string FIELD_53B = "53B";

        /// <summary>
        /// "Receiver's Correspondent"
        /// </summary>
        public const string FIELD_54A = "54A";

        /// <summary>
        /// "Intermediary"
        /// </summary>
        public const string FIELD_56A = "56A";

        /// <summary>
        /// "Account with Institution"
        /// </summary>
        public const string FIELD_56D = "56D";

        /// <summary>
        /// "Account with Institution"
        /// </summary>
        public const string FIELD_57A = "57A";

        /// <summary>
        /// "Account with Institution"
        /// </summary>
        public const string FIELD_57D = "57D";

        /// <summary>
        /// "Beneficiary Institution"
        /// </summary>
        public const string FIELD_58A = "58A";

        /// <summary>
        /// "Account with Institution"
        /// </summary>
        public const string FIELD_58D = "58D";


        /// <summary>
        /// "Beneficiary Customer"
        /// </summary>
        public const string FIELD_59 = "59";

        /// <summary>
        /// "Ultimate Benificiary Customer"
        /// </summary>
        public const string FIELD_59A = "59A";

        /// <summary>
        /// "Details of Charges"
        /// </summary>
        public const string FIELD_71A = "71A";

        /// <summary>
        /// "Sender to Receiver Information"
        /// </summary>
        public const string FIELD_72 = "72";

        /// <summary>
        /// Remittance Information
        /// </summary>
        public const string FIELD_70 = "70";

        /// <summary>
        /// Acknowledgement (ACK) or Negative Acknowledgement (NACK)
        /// ACK = 0
        /// NACK = 1
        /// </summary>
        public const string FIELD_451 = "451";

        /// <summary>
        /// Reason code when Negative Acknowledgement (NACK) is received
        /// </summary>
        public const string FIELD_405 = "405";

        /// <summary>
        /// Time Indicator
        /// </summary>
        public const string FIELD_13C = "13C";

        /// <summary>
        /// Function of the Message
        /// </summary>
        public const string FIELD_23G = "23G";

        /// <summary>
        /// End of Block
        /// </summary>
        public const string FIELD_16S = "16S";

        /// <summary>
        /// Quantity of Financial Instrument
        /// </summary>
        public const string FIELD_36B = "36B";

        /// <summary>
        /// Linkage Type Indicator
        /// </summary>
        public const string FIELD_22F = "22F";

        /// <summary>
        /// Corporate Action Option Code Indicator
        /// </summary>
        public const string FIELD_22H = "22H";

        /// <summary>
        /// Account
        /// </summary>
        public const string FIELD_97A = "97A";

        /// <summary>
        /// Qualifier and Date
        /// </summary>
        public const string FIELD_98A = "98A";

        public const string FIELD_95R = "95R";
        public const string FIELD_95P = "95P";

        /// <summary>
        /// Qualifier and Price
        /// </summary>
        public const string FIELD_90A = "90A";

        /// <summary>
        /// Identification of the Financial Instrument
        /// </summary>
        public const string FIELD_35B = "35B";
        public const string FIELD_25D = "25D";



        public const string FIELD_98C = "98C";


        public static readonly List<string> QualifiedBlock3FieldsList = new List<string>()
        {
            FIELD_103,FIELD_113,FIELD_108,FIELD_119,FIELD_423,FIELD_106,FIELD_424,FIELD_111,FIELD_121,FIELD_115,FIELD_165,FIELD_433,FIELD_434
        };


        /// <summary>
        /// Statement Line
        /// </summary>
        public static string FIELD_61 = "61";

        /// <summary>
        /// Date and Time of User Submission
        /// </summary>
        public static string FIELD_177 = "177";


        public static bool IsBlock3Field(string fieldName)
        {
            return QualifiedBlock3FieldsList.Contains(fieldName);
        }

        public static bool IsBlock3Field(Field field)
        {
            return QualifiedBlock3FieldsList.Contains(field.Name);
        }

        public static Dictionary<string, string> Labels = new Dictionary<string, string>()
        {
            {FIELD_11S,"MT and Date of the Original Message"},
            {FIELD_13C,"Time Indication"},
            {FIELD_16R,"Start of Block"},
            {FIELD_16S,"End of Block"},

            {FIELD_20, "Transaction Reference Number"},
            {FIELD_20C, "Reference"},
            {FIELD_21,"Related Reference"},
            {FIELD_22F,"Linkage Type Indicator"},
            {FIELD_23B,"Bank Operation Code"},
            {FIELD_23G,"Function of the Message"},
            {FIELD_25,"Ultimate Fund Account"},

            {FIELD_30,"Value Date"},
            {FIELD_32A ,"Value Date/Currency/Interbank Settled Amount"},
            {FIELD_32B ,"Currency Code, Amount"},
            {FIELD_35B ,"Identification of the Financial Instrument"},
            {FIELD_36B ,"Quantity of Financial Instrument"},

            {FIELD_50A ,"Ordering Customer"},
            {FIELD_50K ,"Ordering Customer"},
            {FIELD_52A ,"Ordering Institution"},
            {FIELD_52D ,"Ordering Institution"},
            {FIELD_53A ,"Sender's Correspondent"},
            {FIELD_53B ,"Sender's Correspondent"},
            {FIELD_54A ,"Receiver's Correspondent"},
            {FIELD_56A ,"Intermediary"},
            {FIELD_56D, "Intermediary Institution"},
            {FIELD_57A, "Account with Institution"},
            {FIELD_57D, "Account with Institution"},
            {FIELD_58A, "Beneficiary Institution"},
            {FIELD_58D, "Beneficiary Institution"},
            {FIELD_59, "Beneficiary Customer"},

            {FIELD_61,"Statement Line" },

            {FIELD_70, "Remittance Information"},
            {FIELD_71A, "Details of Charges"},
            {FIELD_72, "Sender to Receiver Information"},


            {FIELD_90A,"Qualifier and Price"},
            {FIELD_97A,"Qualifier and Account"},
            {FIELD_98A,"Qualifier and Date"},



            { FIELD_103,"Service Identifier"},
            {FIELD_106,"Message Input Reference MIR (for MIRS only)"},
            {FIELD_108,"Message User Reference (MUR)"},

            {FIELD_111,"Service type identifier"},
            {FIELD_113,"Banking Priority"},
            {FIELD_115,"Addressee Information (for FINCopy only)"},
            {FIELD_119,"Validation Flag"},

            {FIELD_121,"Unique end-to-end transaction reference"},

            {FIELD_165,"Payment release information receiver (For FINInform services only)"},

            {FIELD_177,"Submitted Date/Time"},

            {FIELD_405,"Reason Code for N-Ack"},

            {FIELD_423,"Balance checkpoint date and time (for MIRS only)"},
            {FIELD_424,"Related reference (for MIRS only)"},

            {FIELD_433,"Sanctions screening information for the receiver"},
            {FIELD_434,"Payment controls information for receiver"},

            {FIELD_451,"Accept / Reject Tag"},
        };


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HedgeMark.SwiftMessageHandler.Model;
using HedgeMark.SwiftMessageHandler.Model.Blocks;
using HedgeMark.SwiftMessageHandler.Model.Fields;

namespace HedgeMark.SwiftMessageHandler.Utils
{
    public class SwiftMessageParser
    {
        public static SwiftMessage Parse(string message)
        {
            //Get individual blocks
            var blockConstruct = GetBlocks(message);
            SwiftMessage underlyingSwiftMessage = null;
            string messageType;

            if (blockConstruct.IsServiceMessage)
            {
                underlyingSwiftMessage = Parse(blockConstruct.UnderlyingOriginalMessage);
                messageType = underlyingSwiftMessage.MessageType;
            }
            else
            {
                messageType = GetMessageType(blockConstruct.QualifiedBlocks);
            }

            var swiftMessage = new SwiftMessage(messageType);

            if (blockConstruct.IsServiceMessage)
            {
                swiftMessage.UnderlyingOriginalFINMessage = blockConstruct.UnderlyingOriginalMessage;
                if (underlyingSwiftMessage != null)
                    swiftMessage.UnderlyingOriginalSwiftMessage = underlyingSwiftMessage;
            }


            foreach (var qualifiedBlock in blockConstruct.QualifiedBlocks)
            {
                if (qualifiedBlock.Key == "1")
                {
                    var block1 = ConstructBlock1(qualifiedBlock.Value);
                    swiftMessage.AddBlock(block1);
                }
                else if (qualifiedBlock.Key == "2")
                {
                    var block2 = ConstructBlock2(qualifiedBlock.Value, messageType);
                    swiftMessage.AddBlock(block2);
                }
                else if (qualifiedBlock.Key == "3")
                {
                    var block3 = ConstructBlock3(qualifiedBlock.Value, messageType);
                    swiftMessage.AddBlock(block3);
                }
                else if (qualifiedBlock.Key == "4")
                {
                    var block4 = blockConstruct.IsServiceMessage ? ConstructBlock4OfServiceId21(qualifiedBlock.Value) : ConstructBlock4(qualifiedBlock.Value);
                    swiftMessage.AddBlock(block4);
                }
                else if (qualifiedBlock.Key == "5")
                {
                    var block5 = ConstructBlock5(qualifiedBlock.Value);
                    swiftMessage.AddBlock(block5);
                }
                else
                {
                    var blockUser = ConstructBlockUser(qualifiedBlock.Key, qualifiedBlock.Value);
                    swiftMessage.AddBlock(blockUser);
                }
            }


            return swiftMessage;
        }

        private static SwiftBlock1 ConstructBlock1(string block1)
        {
            var swiftblock1 = new SwiftBlock1
            {
                AppId = block1.Length >= 1 ? block1.Substring(0, 1) : string.Empty,
                ServiceId = block1.Length >= 3 ? block1.Substring(1, 2) : string.Empty,
                LTAddress = new LogicalTerminalAddress()
                {
                    BIC = block1.Length >= 11 ? block1.Substring(3, 8) : string.Empty,
                    TerminalCode = block1.Length >= 12 ? block1.Substring(11, 1) : string.Empty,
                    BICBranchCode = block1.Length >= 15 ? block1.Substring(12, 3) : string.Empty,
                },
            };

            if (block1.Length >= 19)
                swiftblock1.SessionNo = block1.Substring(15, 4);
            if (block1.Length >= 25)
                swiftblock1.SequenceNo = block1.Substring(19, 6);

            return swiftblock1;
        }

        private static SwiftBlock2 ConstructBlock2(string block2, string messageType)
        {
            //INPUT lenght of Block as per definition
            //InputId (1) + MsgType (3) + Recvr (12) + Priority (1)+ DelMoni(1) +ObsolescencePeriod (3) = 21

            //OUTPUT length of Blocl as per definition
            //InputId (1) + MsgType (3) + TIME (4)+ DATE(6) + Recvr (12)+ TIME (4)+ DATE(6)+Session(4)+Sequence(6)  + Priority (1)+ DelMoni(1) +ObsolescencePeriod (3) = 31

            var actualMessage = block2.Replace("{2:", string.Empty).Replace("}", string.Empty);

            var swiftBlock2 = new SwiftBlock2()
            {
                InputOrOutputId = actualMessage.Length >= 1 ? block2.Substring(0, 1) : string.Empty,
                MessageType = actualMessage.Length >= 4 ? block2.Substring(1, 3) : string.Empty,
            };

            if (swiftBlock2.InputOrOutputId == "I")
            {
                swiftBlock2.Receiver = actualMessage.Length >= 16 ? actualMessage.Substring(4, 12) : string.Empty;
                swiftBlock2.Priority = actualMessage.Length >= 17 ? actualMessage.Substring(16, 1) : string.Empty;
                swiftBlock2.DeliveryMonitoring = actualMessage.Length >= 18 ? actualMessage.Substring(17, 1) : string.Empty;
                swiftBlock2.ObsolescencePeriod = actualMessage.Length >= 21 ? actualMessage.Substring(18, 3) : string.Empty;

                return swiftBlock2;
            }

            swiftBlock2.SenderInputTime = actualMessage.Length >= 8 ? actualMessage.Substring(4, 4) : string.Empty;
            swiftBlock2.SenderInputDate = actualMessage.Length >= 14 ? actualMessage.Substring(8, 6) : string.Empty;
            swiftBlock2.Receiver = actualMessage.Length >= 26 ? actualMessage.Substring(14, 12) : string.Empty;

            swiftBlock2.SenderSessionNo = actualMessage.Length >= 30 ? actualMessage.Substring(26, 4) : string.Empty;
            swiftBlock2.SenderSequenceNo = actualMessage.Length >= 36 ? actualMessage.Substring(30, 6) : string.Empty;

            swiftBlock2.SenderOutputDate = actualMessage.Length >= 42 ? actualMessage.Substring(36, 6) : string.Empty;
            swiftBlock2.SenderOutputTime = actualMessage.Length >= 46 ? actualMessage.Substring(42, 4) : string.Empty;

            swiftBlock2.Priority = actualMessage.Length >= 47 ? actualMessage.Substring(46, 1) : string.Empty;
            swiftBlock2.DeliveryMonitoring = actualMessage.Length >= 48 ? actualMessage.Substring(47, 1) : string.Empty;
            swiftBlock2.ObsolescencePeriod = actualMessage.Length >= 50 ? actualMessage.Substring(48, 3) : string.Empty;

            return swiftBlock2;
        }

        private static List<Field> GetFieldsListWith2Levels(string block)
        {
            var pattern = FieldExtractorFromBlockWith2Levels.Replace("X:", string.Empty);
            var regex = new Regex(pattern, RegexOptions.Singleline);
            var matches = regex.Matches(block);

            var listOfFields = new List<KeyValuePair<string, string>>();
            foreach (Match match in matches)
            {
                var fieldBlock = match.Value.Split(':');
                listOfFields.Add(new KeyValuePair<string, string>(fieldBlock[0].Trim(), fieldBlock.Length > 1 ? fieldBlock[1].Trim() : string.Empty));
            }

            return FieldInstantiator.InstantiateAndGetFields(listOfFields);
        }


        private static SwiftBlock3 ConstructBlock3(string block3, string messageType)
        {
            var swiftblock3 = new SwiftBlock3(messageType);

            var fields = GetFieldsListWith2Levels(block3);

            foreach (var field in fields)
                swiftblock3.AddField(field);

            return swiftblock3;
        }

        private static SwiftBlock4 ConstructBlock4OfServiceId21(string block3)
        {
            var swiftblock4 = new SwiftBlock4();

            var fields = GetFieldsListWith2Levels(block3);

            foreach (var field in fields)
                swiftblock4.AddField(field);

            return swiftblock4;
        }

        private static SwiftBlock4 ConstructBlock4(string block4)
        {
            var swiftblock4 = new SwiftBlock4();

            var messageBlock = block4.Replace("{4:", string.Empty).Replace("-}", string.Empty);

            var startIndex = messageBlock.IndexOf(":", StringComparison.Ordinal) + 1;
            var messageToExtract = messageBlock.Substring(startIndex, messageBlock.Length - startIndex).Split(':');
            var listOfFields = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < messageToExtract.Length - 1; i += 2)
                listOfFields.Add(new KeyValuePair<string, string>(messageToExtract[i].Trim(), messageToExtract[i + 1].Trim()));

            var fields = FieldInstantiator.InstantiateAndGetFields(listOfFields);

            foreach (var field in fields)
                swiftblock4.AddField(field);

            return swiftblock4;
        }

        private static SwiftBlock5 ConstructBlock5(string block5)
        {
            var swiftblock5 = new SwiftBlock5();
            var fields = GetFieldsListWith2Levels(block5);

            foreach (var field in fields)
                swiftblock5.AddField(field);

            return swiftblock5;
        }

        private static SwiftBlockUser ConstructBlockUser(string blockName, string blockUser)
        {
            var swiftblockUser = new SwiftBlockUser(blockName);
            var fields = GetFieldsListWith2Levels(blockUser);

            foreach (var field in fields)
                swiftblockUser.AddField(field);
            return swiftblockUser;
        }

        private static string GetMessageType(Dictionary<string, string> blocks)
        {
            var regex = new Regex(MessageTypeExtractorFromBlock2, RegexOptions.Singleline);
            var messageType = regex.Match(blocks["2"]).Value;
            var validationType = string.Empty;
            if (blocks.ContainsKey("3"))
                validationType = GetFieldValueFromBlock3(blocks["3"], FieldDirectory.FIELD_119);

            return string.Format("{0}{1}", messageType, validationType);
        }

        private static string GetFieldValueFromBlock3(string message, string field)
        {
            var pattern = FieldExtractorFromBlockWith2Levels.Replace("X", field);
            var regex = new Regex(pattern, RegexOptions.Singleline);
            var match = regex.Match(message);
            return match.Value;
        }

        private static string GetFieldValueFromBlock4(string message, string field)
        {
            var pattern = FieldExtractorFromBlock4.Replace("X", field);
            var regex = new Regex(pattern, RegexOptions.Singleline);
            var match = regex.Match(message);
            return match.Value;
        }

        private const string ServiceIdExtractorFromBlock1 = @"(?<=^.{4}).{2}";
        private const string MessageTypeExtractorFromBlock2 = @"(?<=^.{1}).{3}";

        private const string FieldExtractorFromBlockWith2Levels = @"(?<=\{X:)(.*?)(?=\})";
        private const string FieldExtractorFromBlock4 = @"(?<=X:)(.*?)(?=:)";

        private const string FieldsExtractor = @"(?<=\{)(.*?)(?=\})";

        //private const string Blocks1Extractor = @"(?<=\{1:)(.*?)(?=\})";
        //private const string Blocks2Extractor = @"(?<=\{2:)(.*?)(?=\})";
        //private const string Blocks3Extractor = @"(?<=\{3:)(.*?)(\}\})";
        //private const string Blocks4Extractor = @"(?<=\{4:)(.*?)(?=-\})";
        //private const string Blocks5Extractor = @"(?<=\{5:)(.*?)(\}\})";

        //private static readonly Dictionary<int, string> BlockExtractorMap = new Dictionary<int, string>()
        //{
        //    { 1,Blocks1Extractor },
        //    { 2,Blocks2Extractor },
        //    { 3,Blocks3Extractor },
        //    { 4,Blocks4Extractor },
        //    { 5,Blocks5Extractor }
        //};


        private class BlockConstruct
        {
            public bool IsServiceMessage { get; set; }
            public List<string> AllParsedBlock { get; set; }
            public Dictionary<string, string> QualifiedBlocks { get; set; }
            public string FinMessage { get; set; }
            public string UnderlyingOriginalMessage { get; set; }
        }

        private static BlockConstruct GetBlocks(string message)
        {
            var blocksList = GetBlockList(message);

            var isServiceMessage21 = false;
            var shouldStartCapturingOriginalMessage = false;
            var underlyingOriginalMessage = new StringBuilder();
            var qualifiedBlocks = new Dictionary<string, string>();

            foreach (var block in blocksList)
            {
                if (isServiceMessage21 && block.StartsWith("{1:") && qualifiedBlocks.ContainsKey("1"))
                    shouldStartCapturingOriginalMessage = true;

                if (isServiceMessage21 && shouldStartCapturingOriginalMessage)
                {
                    underlyingOriginalMessage.Append(block);
                    continue;
                }

                //Is Block 1 and is a Service message
                if (block.StartsWith("{1:"))
                {
                    //Check if this is a service message 
                    var regex = new Regex(ServiceIdExtractorFromBlock1, RegexOptions.Singleline);
                    var serviceId = regex.Match(block).Value;

                    if (serviceId == "21")
                    {
                        isServiceMessage21 = true;

                        //If the Service Message is in the rear end - we need to properly set the Underlying original properly
                        qualifiedBlocks.ForEach(s => { underlyingOriginalMessage.AppendFormat("{{{0}:{1}", s.Key, s.Value); });
                        qualifiedBlocks = new Dictionary<string, string>();
                    }

                    qualifiedBlocks.Add("1", block.Replace("{1:", string.Empty));
                }

                else if (block.StartsWith("{2:"))
                {
                    qualifiedBlocks.Add("2", block.Replace("{2:", string.Empty));
                }
                else if (block.StartsWith("{3:"))
                {
                    qualifiedBlocks.Add("3", block.Replace("{3:", string.Empty));
                }
                else if (block.StartsWith("{4:"))
                {
                    qualifiedBlocks.Add("4", block.Replace("{4:", string.Empty));
                }
                else if (block.StartsWith("{5:"))
                {
                    qualifiedBlocks.Add("5", block.Replace("{5:", string.Empty));
                }
                else
                {
                    var blockName = block.Split(':')[0].Replace("{", string.Empty).Trim();
                    qualifiedBlocks.Add(blockName, block.Replace(string.Format("{{{0}:", blockName), string.Empty));
                }
            }

            //for (var i = 1; i <= 5; i++)
            //{
            //    var pattern = BlockExtractorMap[i];
            //    var regex = new Regex(pattern, RegexOptions.Singleline);
            //    var matches = regex.Matches(message);

            //    if (matches.Count == 1)
            //        blocks.Add(i, matches[0].Value);
            //}


            var construct = new BlockConstruct()
            {
                IsServiceMessage = isServiceMessage21,
                FinMessage = message,
                AllParsedBlock = blocksList,
                QualifiedBlocks = qualifiedBlocks,
                UnderlyingOriginalMessage = underlyingOriginalMessage.ToString()
            };

            return construct;
        }

        private static List<string> GetBlockList(string message)
        {
            var blocks = new List<string>();

            int level = 0;
            var builder = new StringBuilder();
            foreach (var cHar in message)
            {
                builder.Append(cHar);

                switch (cHar)
                {
                    case '{':
                        level++;
                        break;
                    case '}':
                        level--;

                        //End of Block
                        if (level != 0)
                            continue;

                        blocks.Add(builder.ToString());
                        builder = new StringBuilder();
                        break;
                }
            }

            return blocks;
        }
    }
}

using Com.HedgeMark.Commons;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware.Util;
using IBM.WMQ;
using log4net;
using System;
using System.Collections;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MQC = IBM.WMQ.MQC;

namespace HedgeMark.Operations.Secure.Middleware.Queues
{
    public class QueueSystemManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(QueueSystemManager));

        private static string QueueManagerName { get { return ConfigurationManagerWrapper.StringSetting("QueueManagerName", "LQAL"); } }
        private static string HostName { get { return ConfigurationManagerWrapper.StringSetting("ChannelConnectionName", "r26ln00.bnymellon.net"); } }
        private static int ChannelConnectionPort { get { return ConfigurationManagerWrapper.IntegerSetting("ChannelConnectionPort", 1462); } }
        private static string ClientChannelName { get { return ConfigurationManagerWrapper.StringSetting("ClientChannelName", QueueManagerName + ".DMO.CLIENT"); } }

        private static string QueueManagerCcIdProperty { get { return ConfigurationManagerWrapper.StringSetting("QueueManagerCCIdProperty", "1208"); } }

        /*Out-Bound Parameters*/
        private static string SenderQueueName { get { return ConfigurationManagerWrapper.StringSetting("SenderQueueName", "DMO.EMX.DMO2EMX.OUTBOUND.U1.F"); } }
        /*In-Bound Parameters*/
        private static string ReceiverQueueName { get { return ConfigurationManagerWrapper.StringSetting("ReceiverQueueName", "DMO.EMX.EMX2DMO.INBOUND.U1.F"); } }
        /*Ack In-Bound Parameters*/
        private static string ReceiverAckQueueName { get { return ConfigurationManagerWrapper.StringSetting("ReceiverAckQueueName", "DMO.EMX.EMX2DMO.ACK.U1.F"); } }

        //We might need to create Queue based on Environmental parametes

        private static MQQueueManager QueueManager;

        public static void EstablishConnection()
        {
            Environment.SetEnvironmentVariable("MQCCSID", QueueManagerCcIdProperty);
            ConnectMQ();
        }

        private static void ConnectMQ()
        {
            try
            {
                var connectionParams = new Hashtable
                {
                    {MQC.CHANNEL_PROPERTY, ClientChannelName},
                    {MQC.HOST_NAME_PROPERTY, HostName},
                    {MQC.PORT_PROPERTY, ChannelConnectionPort},
                    {MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_CLIENT}
                };

                QueueManager = new MQQueueManager(QueueManagerName, connectionParams);
                QueueManager.Disconnect();
                QueueManager.Connect(QueueManagerName);
                Logger.Debug("MQ Connected Successfully to Send Message");
            }
            catch (MQException mexc)
            {
                var message = string.Format("Unable to connect to Queue Manager: {0} ReasonCode: {1}{2}", mexc.Message, mexc.ReasonCode, mexc.StackTrace);
                if (DateTime.Today.DayOfWeek != DayOfWeek.Sunday)
                    Logger.Error(message, mexc);
                throw new Exception(message, mexc);
            }
        }

        public static void SendMessage(string swiftMessage)
        {
            if (Utility.IsLocal())
                return;

            //Send this Swift Message
            SendMessageInQueue(swiftMessage);

            new TaskFactory().StartNew(() =>
            {
                Task.Delay(1000 * 3);

                //Look out for FEACK or ACK
                GetAndProcessAcknowledgement();
            });
        }

        private static void SendMessageInQueue(string swiftMessage, bool isRetry = false)
        {
            try
            {
                if (QueueManager == null || !QueueManager.IsConnected)
                    ConnectMQ();

                var queue = QueueManager.AccessQueue(SenderQueueName, MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);

                var queueMessage = new MQMessage();
                queueMessage.WriteString(swiftMessage);
                queueMessage.Format = MQC.MQFMT_STRING;

                var queuePutMessageOptions = new MQPutMessageOptions();
                queue.Put(queueMessage, queuePutMessageOptions);
                queue.Close();

                //Log all OutGoing messages
                LogMQMessage(swiftMessage, SenderQueueName, true);
                Logger.Debug("MQ Message sent Successfully");
            }

            catch (MQException ex)
            {
                //Retry once to fix the Broken connection - will fail on the second retry
                if (!isRetry && ex.Message == "MQRC_CONNECTION_BROKEN")
                {
                    //sleep for 2 second
                    Thread.Sleep(1000 * 2);
                    SendMessageInQueue(swiftMessage, true);
                }
                else
                {
                    Logger.Error("Sending MQ Message Failed: " + ex.Message, ex);
                    throw;
                }
            }

            catch (Exception ex)
            {
                Logger.Error("Sending Message Failed: " + ex.Message, ex);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void GetAndProcessAcknowledgement(long wireId = -1)
        {
            if (!IsPendingResponseQueue())
                return;

            GetAndProcessQueueMessage(ReceiverAckQueueName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void GetAndProcessMessage()
        {
            if (!IsPendingResponseQueue())
                return;

            GetAndProcessQueueMessage(ReceiverQueueName);
        }

        private static void GetAndProcessQueueMessage(string queueName)
        {
            if (Utility.IsLocal())
                return;

            if (QueueManager == null || !QueueManager.IsConnected)
                ConnectMQ();

            // accessing queue
            var queue = QueueManager.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING);

            //var noOfMessages = queue.CurrentDepth;

            //if (noOfMessages > 0)
            //    Logger.Info(string.Format("Total messages available as of {0} is ={1}", DateTime.Now.ToLongTimeString(), noOfMessages));

            // creating a message options object
            var mqGetMsgOpts = new MQGetMessageOptions { Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_WAIT };

            while (true)
            {
                hmsMQLog mqLog = null;
                try
                {
                    Logger.Debug("Getting the Inbound message from Queue..");
                    var message = new MQMessage();

                    queue.Get(message, mqGetMsgOpts);
                    var messageAsText = message.ReadString(message.MessageLength);

                    //Log all Incoming messages
                    mqLog = LogMQMessage(messageAsText, queueName, false);

                    WireTransactionManager.ProcessInboundMessage(messageAsText);

                    UpdateMQMessageLog(mqLog, "MQ Message Processed Successfully");

                    message.ClearMessage();
                }
                catch (MQException mqe)
                {
                    //This means no messages available in Queue to Process
                    if (mqe.ReasonCode == 2033)
                        break;

                    var msg = string.Format("MQException caught: {0} {1}", mqe.ReasonCode, mqe.Message);
                    Logger.Error(msg, mqe);
                    if (mqLog != null)
                        UpdateMQMessageLog(mqLog, msg);
                }
                catch (UnhandledWireMessageException ex)
                {
                    if (mqLog != null)
                        UpdateMQMessageLog(mqLog, ex.Message);
                }
                catch (Exception ex)
                {
                    var msg = string.Format("Unhandled Exception when processing inbound : {0}", ex.Message);
                    Logger.Error(msg, ex);
                    if (mqLog != null)
                        UpdateMQMessageLog(mqLog, msg);
                }
            }

            queue.Close();
        }

        public static bool IsPendingResponseQueue()
        {
            //NotInitiated = 1,
            //Processing =2,
            //Acknowledged=3,
            //NegativeAcknowledged=4,
            //Completed=5,
            //Failed=6,

            using (var context = new OperationsSecureContext())
            {
                return context.hmsWires.Any(s => s.SwiftStatusId == 2 || s.SwiftStatusId == 3);
            }
        }

        public static hmsMQLog LogMQMessage(string message, string queueName, bool isOutBound)
        {
            using (var context = new OperationsSecureContext())
            {
                var messageMqLog = new hmsMQLog()
                {
                    QueueManager = QueueManagerName,
                    Message = message,
                    QueueName = queueName,
                    IsOutBound = isOutBound,
                    CreatedAt = DateTime.Now
                };

                if (isOutBound)
                    messageMqLog.OpsSecureHandlerMessage = "MQ Message sent Successfully";


                context.hmsMQLogs.Add(messageMqLog);
                context.SaveChanges();

                return messageMqLog;
            }
        }

        public static void UpdateMQMessageLog(hmsMQLog log, string hmOpsHandlerMessage)
        {
            using (var context = new OperationsSecureContext())
            {
                log.OpsSecureHandlerMessage = hmOpsHandlerMessage;
                context.hmsMQLogs.AddOrUpdate(log);
                context.SaveChanges();
            }
        }


        //private static void ProcessMessage(IMessage msg)
        //{
        //    var bytesMessage = (IBytesMessage)msg;

        //    var buffer = new byte[bytesMessage.BodyLength];
        //    bytesMessage.ReadBytes(buffer, (int)bytesMessage.BodyLength);
        //    var messageAsText = Encoding.Unicode.GetString(buffer);
        //    Logger.Info("Got a message: " + messageAsText);
        //    WireTransactionManager.ProcessInboundMessage(messageAsText);
        //    Logger.Info("Message Processing Complete");
        //}

        //private static void OnException(Exception ex)
        //{
        //    Logger.Error("MQ Inbound - got an Exception: " + ex.Message, ex);
        //}



        //public static void ConnectInBoundMQAndListen()
        //{
        //    XMSFactoryFactory xff = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
        //    IConnectionFactory cf = xff.CreateConnectionFactory();
        //    cf.SetStringProperty(XMSC.WMQ_HOST_NAME, HostName);//"10.87.188.156(7111)"
        //    cf.SetIntProperty(XMSC.WMQ_PORT, ChannelConnectionPort);
        //    cf.SetStringProperty(XMSC.WMQ_CHANNEL, ClientChannelName);
        //    cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
        //    cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, QueueManagerName);
        //    cf.SetIntProperty(XMSC.WMQ_BROKER_VERSION, XMSC.WMQ_BROKER_V1);

        //    IConnection conn = cf.CreateConnection();
        //    Console.WriteLine("connection created");
        //    ISession sess = conn.CreateSession(false, AcknowledgeMode.AutoAcknowledge);
        //    IDestination dest = sess.CreateQueue(ReceiverAckQueueName);
        //    IMessageConsumer consumer = sess.CreateConsumer(dest);
        //    MessageListener ml = new MessageListener(OnMessage);
        //    consumer.MessageListener = ml;
        //    conn.Start();
        //    Console.WriteLine("Consumer started");
        //}

        //private static void OnMessage(IMessage msg)
        //{
        //    //ITextMessage textMsg = (ITextMessage)msg;
        //    //Console.Write("Got a message: ");
        //    //Console.WriteLine(textMsg.Text);


        //    IBytesMessage bytesMessage = (IBytesMessage)msg;

        //    var buffer = new byte[bytesMessage.BodyLength];
        //    bytesMessage.ReadBytes(buffer, (int)bytesMessage.BodyLength);
        //    var messageAsText = Encoding.Unicode.GetString(buffer);

        //    Logger.Debug("Message Received from Listener:" + messageAsText);

        //    //var messageAsText = textMsg.Text.ReadString(message.MessageLength);

        //    //Log all Incoming messages
        //    LogMQMessage(messageAsText, ReceiverAckQueueName, false);

        //    WireTransactionManager.ProcessInboundMessage(messageAsText);
        //    Logger.Debug("Message Processing Complete");

        //}


        //private static void ConnectInBoundMQAndListen()
        //{
        //    var xfactoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
        //    var connectionFactory = xfactoryFactory.CreateConnectionFactory();
        //    connectionFactory.SetStringProperty(XMSC.WMQ_HOST_NAME, HostName);
        //    connectionFactory.SetIntProperty(XMSC.WMQ_PORT, ChannelConnectionPort);
        //    connectionFactory.SetStringProperty(XMSC.WMQ_CHANNEL, ClientChannelName);
        //    connectionFactory.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, QueueManagerName);
        //    connectionFactory.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
        //    connectionFactory.SetIntProperty(XMSC.WMQ_BROKER_VERSION, XMSC.WMQ_BROKER_V1);
        //    connectionFactory.SetStringProperty(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_CLIENT);

        //    var connection = connectionFactory.CreateConnection();
        //    Logger.Info("MQ connection created..");

        //    // Create the connection and register an exception listener
        //    connection.ExceptionListener = OnException;

        //    var sess = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge);
        //    var dest = sess.CreateQueue(ReceiverQueueName);

        //    // Create the consumer and register an async message listener
        //    var consumer = sess.CreateConsumer(dest);
        //    MessageListener ml = OnMessage;

        //    consumer.MessageListener = ml;
        //    connection.Start();
        //    Logger.Info("Consumer started..");

        //}

        //private static void ConnectInBoundMQAndListen()
        //{
        //    // Get an instance of factory.
        //    var xfactoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
        //    var connectionFactory = xfactoryFactory.CreateConnectionFactory();
        //    connectionFactory.SetStringProperty(XMSC.WMQ_HOST_NAME, HostName);
        //    connectionFactory.SetIntProperty(XMSC.WMQ_PORT, ChannelConnectionPort);
        //    connectionFactory.SetStringProperty(XMSC.WMQ_CHANNEL, ClientChannelName);
        //    connectionFactory.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, QueueManagerName);
        //    connectionFactory.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
        //    connectionFactory.SetIntProperty(XMSC.WMQ_BROKER_VERSION, XMSC.WMQ_BROKER_V1);
        //    connectionFactory.SetStringProperty(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_CLIENT);

        //    var connection = connectionFactory.CreateConnection();
        //    Logger.Info("MQ connection created..");

        //    // Create the connection and register an exception listener
        //    connection.ExceptionListener = OnException;

        //    var sess = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge);
        //    var dest = sess.CreateQueue(ReceiverQueueName);
        //    var consumer = sess.CreateConsumer(dest);
        //    MessageListener ml = new MessageListener(OnMessage);
        //    consumer.MessageListener = ml;

        //    connection.Start();
        //    Logger.Info("Consumer started..");

        //    Logger.Info("Wait 30 seconds for messages.");
        //    System.Threading.Thread.Sleep(1000 * 60);

        //    // Cleanup
        //    consumer.Close();
        //    dest.Dispose();
        //    sess.Dispose();
        //    connection.Close();
        //}

        //private static void ConnectInBoundMQAndListen()
        //{
        //    try
        //    {
        //        // mq properties
        //        var connectionParams = new Hashtable
        //        {
        //            {MQC.CHANNEL_PROPERTY, ClientChannelName},
        //            {MQC.HOST_NAME_PROPERTY, HostName},
        //            {MQC.PORT_PROPERTY, ChannelConnectionPort},
        //            {MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_CLIENT},

        //            //ConnectionProperties.Add(MQC.CCSID_PROPERTY, MQC.CODESET_UTF);
        //        //{MQC.CCSID_PROPERTY, QueueManagerCcIdProperty}
        //    };

        //        // create connection
        //        Logger.Info("Connecting to MQ.. ");
        //        MQQueueManager queueManager = new MQQueueManager(QueueManagerName, connectionParams);
        //        Logger.Info("MQ connection created..");


        //    }
        //    catch (MQException ex)
        //    {
        //        Logger.Error("MQ Inbound - got an Exception: " + ex.Message, ex);
        //    }
        //}

    }
}

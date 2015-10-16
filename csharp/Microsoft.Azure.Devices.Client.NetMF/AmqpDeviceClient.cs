// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client
{
    using Amqp;
    using Amqp.Framing;
    using System;
    using System.Text;

    sealed class AmqpDeviceClient : DeviceClientHelper
    {
        const uint DefaultPrefetchCount = 50;
        static readonly IotHubConnectionCache connectionCache = new IotHubConnectionCache(AccessRights.DeviceConnect);
        readonly string deviceId;
        SenderLink sendingLink;
        ReceiverLink receivingLink;
        readonly IotHubConnection IotHubConnection;
        readonly TimeSpan openTimeout;
        readonly TimeSpan operationTimeout;

        int eventsDeliveryTag;

        public AmqpDeviceClient(IotHubConnectionString connectionString)
        {
            this.IotHubConnection = connectionCache.GetConnection(connectionString);
            this.deviceId = connectionString.DeviceId;
            this.openTimeout = IotHubConnection.DefaultOpenTimeout;
            this.operationTimeout = IotHubConnection.DefaultOperationTimeout;
            this.DefaultReceiveTimeout = IotHubConnection.DefaultOperationTimeout;
        }

        /// <summary>
        /// Create a DeviceClient from individual parameters
        /// </summary>
        /// <param name="hostname">The fully-qualified DNS hostname of IoT Hub</param>
        /// <param name="authMethod">The authentication method that is used</param>
        /// <returns>DeviceClient</returns>
        public static AmqpDeviceClient Create(string hostname, IAuthenticationMethod authMethod)
        {
            if (hostname == null)
            {
                throw new ArgumentNullException("hostname");
            }

            if (authMethod == null)
            {
                throw new ArgumentNullException("authMethod");
            }

            var connectionStringBuilder = IotHubConnectionStringBuilder.Create(hostname, authMethod);
            return CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        /// <summary>
        /// Create DeviceClient from the specified connection string
        /// </summary>
        /// <param name="connectionString">Connection string for the IoT hub</param>
        /// <returns>DeviceClient</returns>
        public static AmqpDeviceClient CreateFromConnectionString(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }

            var iotHubConnectionString = IotHubConnectionString.Parse(connectionString);
            return new AmqpDeviceClient(iotHubConnectionString);
        }

        //// This Finalizer gets cancelled when/if the user calls CloseAsync.
        //~AmqpDeviceClient()
        //{
        //    // If the user failed to call CloseAsync make sure the connection's reference count gets updated.
        //    this.CloseAsync().Fork();
        //}

        public TimeSpan OpenTimeout
        {
            get
            {
                return this.openTimeout;
            }
        }

        public TimeSpan OperationTimeout
        {
            get
            {
                return this.operationTimeout;
            }
        }

        public IotHubConnection Connection
        {
            get
            {
                return this.IotHubConnection;
            }
        }

        public Amqp.SenderLink EventSendingLink
        {
            get
            {
                return this.sendingLink;
            }
        }

        //public AmqpLink DeviceBoundReceivingLink
        //{
        //    get
        //    {
        //        return this.faultTolerantDeviceBoundReceivingLink.Value;
        //    }
        //}

        protected override TimeSpan DefaultReceiveTimeout { get; set; }

        protected override void OnOpen(bool explicitOpen)
        {
            //if (!explicitOpen)
            //{
            //    return;
            //}

            //if(sendingLink == null)
            //{
            //    CreateEventSendingLink(this.OpenTimeout);
            //}

            if (receivingLink == null)
            {
                CreateDeviceBoundReceivingLink(this.OpenTimeout);
                //this.receivingLink.Start(1, null);
            }
        }

        //protected override async Task OnOpenAsync(bool explicitOpen)
        //{
        //    if (!explicitOpen)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        await Task.WhenAll(
        //            this.faultTolerantEventSendingLink.OpenAsync(this.OpenTimeout),
        //            this.faultTolerantDeviceBoundReceivingLink.OpenAsync(this.OpenTimeout));
        //    }
        //    catch (Exception exception)
        //    {
        //        if (exception.IsFatal())
        //        {
        //            throw;
        //        }

        //        throw AmqpClientHelper.ToIotHubClientContract(exception);
        //    }
        //}

       // protected override Task OnCloseAsync()
       // {
       //     GC.SuppressFinalize(this);
       //     return connectionCache.ReleaseConnectionAsync(this.Connection);
       // }

        protected override void OnSendEvent(Message message)
        {
            this.sendingLink.Send(message, MessageOutComeCallback, null);
        }

        private void MessageOutComeCallback(Amqp.Message message, Outcome outcome, object state)
        {
            if (outcome is Accepted)
            {
                //// if message has an async callback on accepted call it
                //if (((Message)message).AsyncCallbackOnAccepted != null)
                //{
                //    ((Message)message).AsyncCallbackOnAccepted(null);
                //}
            }
            else
            {
                // error sending message
            }

            //if (this.AMQPSendQueue.Count > 0)
            //{
            //    var nextMessage = (Message)this.AMQPSendQueue.Dequeue();
            //    //this._amqpSender.Send(nextMessage, MessageOutComeCallback, null);
            //}
            //else
            //{
            //    this.bPendingSend = false;

            //    // no more messages, raise queue empty event
            //    OnQueueEmpty();
            //}

        }

       // protected override async Task OnSendEventAsync(IEnumerable<Message> messages)
       // {
       //     // List to hold messages in Amqp friendly format
       //     var messageList = new List<Data>();

       //     foreach (var message in messages)
       //     {
       //         using (AmqpMessage amqpMessage = message.ToAmqpMessage())
       //         {
       //             var data = new Data() { Value = MessageConverter.ReadStream(amqpMessage.ToStream()) };
       //             messageList.Add(data);
       //         }
       //     }

       //     Outcome outcome;
       //     using (var amqpMessage = AmqpMessage.Create(messageList))
       //     {
       //         amqpMessage.MessageFormat = AmqpConstants.AmqpBatchedMessageFormat;
       //         outcome = await this.SendAmqpMessageAsync(amqpMessage);
       //     }

       //     if (outcome.DescriptorCode != Accepted.Code)
       //     {
       //         throw AmqpErrorMapper.GetExceptionFromOutcome(outcome);
       //     }
       // }

        protected override Message OnReceive(TimeSpan timeout)
        {
            //try
            //{
            //    ReceivingAmqpLink deviceBoundReceivingLink = await this.GetDeviceBoundReceivingLinkAsync();
            //    amqpMessage = await deviceBoundReceivingLink.ReceiveMessageAsync(timeout);
            //}
            //catch (Exception exception)
            //{
            //    if (exception.IsFatal())
            //    {
            //        throw;
            //    }

            //    throw AmqpClientHelper.ToIotHubClientContract(exception);
            //}

            Amqp.Message amqpMessage = this.receivingLink.Receive();

            Message message;
            if (amqpMessage != null)
            {
                message = new Message(amqpMessage)
                {
                    LockToken = new Guid(amqpMessage.DeliveryTag).ToString()
                };
            }
            else
            {
                message = null;
            }

            return message;
        }

        protected override void OnComplete(string lockToken)
        {
            //return this.DisposeMessageAsync(lockToken, AmqpConstants.AcceptedOutcome);
        }

        protected override void OnAbandon(string lockToken)
        {
            //return this.DisposeMessage(lockToken, AmqpConstants.ReleasedOutcome);
        }

        protected override void OnReject(string lockToken)
        {
            //return this.DisposeMessageAsync(lockToken, AmqpConstants.RejectedOutcome);
        }

       // protected override Task OnRejectAsync(Message message)
       // {
       //     if (message == null)
       //     {
       //         throw Fx.Exception.ArgumentNull("message");
       //     }

       //     return this.DisposeMessageAsync(message.LockToken, AmqpConstants.RejectedOutcome);
       // }

       //async Task<Outcome> SendAmqpMessageAsync(AmqpMessage amqpMessage)
       //{
       //     Outcome outcome;
       //     try
       //     {
       //         SendingAmqpLink eventSendingLink = await this.GetEventSendingLinkAsync();
       //         outcome = await eventSendingLink.SendMessageAsync(amqpMessage, IotHubConnection.GetNextDeliveryTag(ref this.eventsDeliveryTag), AmqpConstants.NullBinary, this.OperationTimeout);
       //     }
       //     catch (Exception exception)
       //     {
       //         if (exception.IsFatal())
       //         {
       //             throw;
       //         }

       //         throw AmqpClientHelper.ToIotHubClientContract(exception);
       //     }

       //     return outcome;
       //} 

        //void DisposeMessage(string lockToken, Outcome outcome)
        //{
        //    var deliveryTag = IotHubConnection.ConvertToDeliveryTag(lockToken);

        //    Outcome disposeOutcome;
        //    try
        //    {
        //        ReceivingAmqpLink deviceBoundReceivingLink = await this.GetDeviceBoundReceivingLinkAsync();
        //        disposeOutcome = await deviceBoundReceivingLink.DisposeMessageAsync(deliveryTag, outcome, batchable: true, timeout: this.OperationTimeout);
        //    }
        //    catch (Exception exception)
        //    {
        //        if (exception.IsFatal())
        //        {
        //            throw;
        //        }

        //        throw AmqpClientHelper.ToIotHubClientContract(exception);
        //    }

        //    if (disposeOutcome.DescriptorCode != Accepted.Code)
        //    {
        //        if (disposeOutcome.DescriptorCode == Rejected.Code)
        //        {
        //            var rejected = (Rejected)disposeOutcome;

        //            // Special treatment for NotFound amqp rejected error code in case of DisposeMessage 
        //            if (rejected.Error != null && rejected.Error.Condition.Equals(AmqpErrorCode.NotFound))
        //            {
        //                throw new DeviceMessageLockLostException(rejected.Error.Description);
        //            }
        //        }

        //        throw AmqpErrorMapper.GetExceptionFromOutcome(disposeOutcome);
        //    }
        //}

       // async Task<SendingAmqpLink> GetEventSendingLinkAsync()
       // {
       //     SendingAmqpLink eventSendingLink;
       //     if (!this.faultTolerantEventSendingLink.TryGetOpenedObject(out eventSendingLink))
       //     {
       //         eventSendingLink = await this.faultTolerantEventSendingLink.GetOrCreateAsync(this.OpenTimeout);
       //     }
       //     return eventSendingLink;
       // }

        void CreateEventSendingLink(TimeSpan timeout)
        {
            string path = "/devices/" + HttpUtility.UrlEncode(this.deviceId) + "/messages/events";

            this.sendingLink = this.IotHubConnection.CreateSendingLink(path, timeout);
        }

       // async Task<ReceivingAmqpLink> GetDeviceBoundReceivingLinkAsync()
       // {
       //     ReceivingAmqpLink deviceBoundReceivingLink;
       //     if (!this.faultTolerantDeviceBoundReceivingLink.TryGetOpenedObject(out deviceBoundReceivingLink))
       //     {
       //         deviceBoundReceivingLink = await this.faultTolerantDeviceBoundReceivingLink.GetOrCreateAsync(this.OpenTimeout);
       //     }

       //     return deviceBoundReceivingLink;
       // }

        void CreateDeviceBoundReceivingLink(TimeSpan timeout)
        {
            string path = "/devices/" + HttpUtility.UrlEncode(this.deviceId) + "/messages/deviceBound";

            this.receivingLink = this.IotHubConnection.CreateReceivingLink(path, timeout, DefaultPrefetchCount);
        }
    }
}

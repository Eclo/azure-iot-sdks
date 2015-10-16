// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client
{
    using Amqp;
using System;

    sealed class IotHubConnection
    {
        internal static readonly TimeSpan DefaultOperationTimeout = new TimeSpan(0, 1, 0);
        internal static readonly TimeSpan DefaultOpenTimeout = new TimeSpan(0, 1, 0);
        static readonly TimeSpan RefreshTokenBuffer = new TimeSpan(0, 2, 0);
        static readonly TimeSpan RefreshTokenRetryInterval = new TimeSpan(0, 0, 30);

        //static readonly AmqpVersion AmqpVersion_1_0_0 = new AmqpVersion(1, 0, 0);
        const string DisableServerCertificateValidationKeyName = "Microsoft.Azure.Devices.DisableServerCertificateValidation";
        //readonly static Lazy<bool> DisableServerCertificateValidation = new Lazy<bool>(InitializeDisableServerCertificateValidation);
        readonly IotHubConnectionString connectionString;
        readonly AccessRights accessRights;
        Amqp.Session session;
        Amqp.Connection connection;

        //readonly IOThreadTimer refreshTokenTimer;

        public IotHubConnection(IotHubConnectionString connectionString, AccessRights accessRights)
        {
            this.connectionString = connectionString;
            this.accessRights = accessRights;
            this.session = this.CreateSession();
            //this.refreshTokenTimer = new IOThreadTimer(s => ((IotHubConnection)s).OnRefreshToken(), this, false);
        }

        public IotHubConnectionString ConnectionString
        {
            get
            {
                return this.connectionString;
            }
        }

        //public Task OpenAsync(TimeSpan timeout)
        //{
        //    return this.faultTolerantSession.GetOrCreateAsync(timeout);
        //}

        //public Task CloseAsync()
        //{
        //    return this.faultTolerantSession.CloseAsync();
        //}

        //public void SafeClose(Exception exception)
        //{
        //    this.faultTolerantSession.Close();
        //}

        public SenderLink CreateSendingLink(string path, TimeSpan timeout)
        {
            return new SenderLink(this.session, Guid.NewGuid().ToString(), path);
        }

        public ReceiverLink CreateReceivingLink(string path, TimeSpan timeout, uint prefetchCount)
        {
            var linkAddress = this.connectionString.BuildLinkAddress(path);

            return new ReceiverLink(this.session, Guid.NewGuid().ToString(), new Amqp.Framing.Source() { Address = linkAddress.AbsoluteUri }, null);
        }

        //public async Task<RequestResponseAmqpLink> CreateRequestResponseLink(string path, TimeSpan timeout)
        //{
        //    var timeoutHelper = new TimeoutHelper(timeout);

        //    AmqpSession session;
        //    if (!this.faultTolerantSession.TryGetOpenedObject(out session))
        //    {
        //        session = await this.faultTolerantSession.GetOrCreateAsync(timeoutHelper.RemainingTime());
        //    }

        //    var linkAddress = this.connectionString.BuildLinkAddress(path);

        //    var linkSettings = new AmqpLinkSettings()
        //    {
        //        TotalLinkCredit = 0,
        //        AutoSendFlow = false,
        //        Source = new Source() { Address = linkAddress.AbsoluteUri },
        //        SettleType = SettleMode.SettleOnDispose,
        //        LinkName = Guid.NewGuid().ToString("N") // Use a human readable link name to help with debuggin
        //    };

        //    linkSettings.AddProperty(IotHubAmqpProperty.TimeoutName, timeoutHelper.RemainingTime().TotalMilliseconds);

        //    var link = new RequestResponseAmqpLink(session, linkSettings);

        //    await OpenLinkAsync(link, timeoutHelper.RemainingTime());

        //    return link;
        //}

        //public void CloseLink(AmqpLink link)
        //{
        //    link.SafeClose();
        //}

        //static bool InitializeDisableServerCertificateValidation()
        //{
        //    string value = ConfigurationManager.AppSettings[DisableServerCertificateValidationKeyName];
        //    if (!string.IsNullOrEmpty(value))
        //    {
        //        return bool.Parse(value);
        //    }

        //    return false;
        //}

        Amqp.Session CreateSession()
        {
            this.connection = new Connection(new Address(this.connectionString.HostName, 5671, this.connectionString.GetUser(), this.connectionString.GetPassword()));
            this.session = new Session(this.connection);
            return this.session;

            //// This adds itself to amqpConnection.Extensions
            //var cbsLink = new AmqpCbsLink(amqpConnection);
            //await this.SendCbsTokenAsync(cbsLink, timeoutHelper.RemainingTime());
            //return amqpSession;
        }

        //static async Task OpenLinkAsync(AmqpObject link, TimeSpan timeout)
        //{
        //    var timeoutHelper = new TimeoutHelper(timeout);
        //    try
        //    {
        //        await link.OpenAsync(timeoutHelper.RemainingTime());
        //    }
        //    catch (Exception exception)
        //    {
        //        if (exception.IsFatal())
        //        {
        //            throw;
        //        }

        //        link.SafeClose(exception);

        //        throw;
        //    }
        //}

        void CloseConnection()
        {
            // Closing the connection also closes any sessions.
            this.session.Connection.Close();
        }

//        AmqpSettings CreateAmqpSettings()
//        {
//            var amqpSettings = new AmqpSettings();          

//            var amqpTransportProvider = new AmqpTransportProvider();
//            amqpTransportProvider.Versions.Add(AmqpVersion_1_0_0);
//            amqpSettings.TransportProviders.Add(amqpTransportProvider);

//            return amqpSettings;
//        }

//        TlsTransportSettings CreateTlsTransportSettings()
//        {
//            var tcpTransportSettings = new TcpTransportSettings()
//            {
//                Host = this.connectionString.HostName,
//                Port = this.connectionString.AmqpEndpoint.Port
//            };

//            var tlsTransportSettings = new TlsTransportSettings(tcpTransportSettings)
//            {
//                TargetHost = this.connectionString.HostName,
//                Certificate = null, // TODO: add client cert support
//                CertificateValidationCallback = this.OnRemoteCertificateValidation
//            };

//            return tlsTransportSettings;
//        }

//        async Task SendCbsTokenAsync(AmqpCbsLink cbsLink, TimeSpan timeout)
//        {
//            string audience = this.ConnectionString.AmqpEndpoint.AbsoluteUri;
//            string resource = this.ConnectionString.AmqpEndpoint.AbsoluteUri;
//            var expiresAtUtc = await cbsLink.SendTokenAsync(
//                this.ConnectionString,
//                this.ConnectionString.AmqpEndpoint,
//                audience,
//                resource,
//                AccessRightsHelper.AccessRightsToStringArray(this.accessRights),
//                timeout);
//            this.ScheduleTokenRefresh(expiresAtUtc);
//        }

//        async void OnRefreshToken()
//        {
//            AmqpSession amqpSession = this.faultTolerantSession.Value;
//            if (amqpSession != null && !amqpSession.IsClosing())
//            {
//                var cbsLink = amqpSession.Connection.Extensions.Find<AmqpCbsLink>();
//                if (cbsLink != null)
//                {
//                    try
//                    {
//                        await this.SendCbsTokenAsync(cbsLink, DefaultOperationTimeout);
//                    }
//                    catch (Exception exception)
//                    {
//                        if (Fx.IsFatal(exception))
//                        {
//                            throw;
//                        }

//                        this.refreshTokenTimer.Set(RefreshTokenRetryInterval);
//                    }
//                }
//            }
//        }

//        bool OnRemoteCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
//        {
//            if (sslPolicyErrors == SslPolicyErrors.None)
//            {
//                return true;
//            }

//            if (DisableServerCertificateValidation.Value && sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
//            {
//                return true;
//            }

//            return false;

//        }

//        public static ArraySegment<byte> GetNextDeliveryTag(ref int deliveryTag)
//        {
//            int nextDeliveryTag = Interlocked.Increment(ref deliveryTag);
//            return new ArraySegment<byte>(BitConverter.GetBytes(nextDeliveryTag));
//        }

//        public static ArraySegment<byte> ConvertToDeliveryTag(string lockToken)
//        {
//            if (lockToken == null)
//            {
//                throw new ArgumentNullException("lockToken");
//            }

//            Guid lockTokenGuid;
//            if (!Guid.TryParse(lockToken, out lockTokenGuid))
//            {
//                throw new ArgumentException("Should be a valid Guid", "lockToken");
//            }

//            var deliveryTag = new ArraySegment<byte>(lockTokenGuid.ToByteArray());
//            return deliveryTag;
//        }

//        void ScheduleTokenRefresh(DateTime expiresAtUtc)
//        {
//            if (expiresAtUtc == DateTime.MaxValue)
//            {
//                return;
//            }

//            TimeSpan timeFromNow = expiresAtUtc.Subtract(RefreshTokenBuffer).Subtract(DateTime.UtcNow);
//            if (timeFromNow > TimeSpan.Zero)
//            {
//                this.refreshTokenTimer.Set(timeFromNow);
//            }
//        }
    }
}

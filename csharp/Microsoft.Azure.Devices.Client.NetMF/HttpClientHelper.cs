﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client
{
    using Eclo.NetMF.SIM800H.Http;
    using Microsoft.Azure.Devices.Client.Extensions;
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Text;

    sealed class HttpClientHelper : IDisposable
    {
        readonly Uri baseAddress;
        readonly IAuthorizationHeaderProvider authenticationHeaderProvider;
        bool isDisposed;

        public HttpClientHelper(
            Uri baseAddress,
            IAuthorizationHeaderProvider authenticationHeaderProvider,
            TimeSpan timeout)
        {
            this.baseAddress = baseAddress;
            this.authenticationHeaderProvider = authenticationHeaderProvider;
        }

        public HttpWebResponse Get(
            string requestUri,
            Hashtable customHeaders)
        {
            return this.Get(requestUri, customHeaders, true);
        }

        public HttpWebResponse Get(
            string requestUri,
            Hashtable customHeaders,
            bool throwIfNotFound)
        {
            if (throwIfNotFound)
            {
                var webRequest = HttpWebRequest.Create(new Uri(this.baseAddress.OriginalString + requestUri));
                webRequest.Method = "GET";

                {
                    // add authorization header
                    webRequest.Headers.Add("Authorization", this.authenticationHeaderProvider.GetAuthorizationHeader());

                    // add custom headers
                    AddCustomHeaders(webRequest, customHeaders);

                    // perform request and get response
                    var webResponse = webRequest.GetResponse();

                    // message received
                    return ReadResponseMessageAsync(webResponse);
                }
            }
            else
            {
                //this.ExecuteAsync(
                //   HttpMethod.Get,
                //   new Uri(this.baseAddress, requestUri),
                //   (requestMsg, token) => AddCustomHeaders(requestMsg, customHeaders),
                //   message => message.IsSuccessStatusCode || message.StatusCode == HttpStatusCode.NotFound,
                //   async (message, token) => result = message.StatusCode == HttpStatusCode.NotFound ? (default(T)) : await ReadResponseMessageAsync<T>(message, token),
                //   errorMappingOverrides,
                //   cancellationToken);
            }

            return null;
        }

        static HttpWebResponse ReadResponseMessageAsync(HttpWebResponse message)
        {
            // TODO
            // unclear what is the purpose of this method and how to implement it
            return message;
           //// T entity = message.Content.ReadAsAsync<T>(token);

           // // Etag in the header is considered authoritative
           // var eTagHolder = entity as IETagHolder;
           // if (eTagHolder != null)
           // {
           //     if (message.Headers.GetValues("ETag") != null)
           //     {
           //         var etagValue = message.Headers.GetValues("ETag");

           //         if (etagValue.Length == 1 && !etagValue[0].IsNullOrWhiteSpace())
           //         {
           //             // RDBug 3429280:Make the version field of Device object internal
           //             eTagHolder.ETag = etagValue[0];
           //         }
           //     }
           // }

           // return entity;
        }

        static void AddCustomHeaders(HttpWebRequest requestMessage, Hashtable customHeaders)
        {
            foreach (var header in customHeaders.Keys)
            {
                requestMessage.Headers.Add(header as string, customHeaders[header] as string);
            }
        }

        static void InsertEtag(HttpWebRequest requestMessage, IETagHolder entity, PutOperationType operationType)
        {
            if (operationType == PutOperationType.CreateEntity)
            {
                return;
            }

            if (operationType == PutOperationType.ForceUpdateEntity)
            {
                const string etag = "\"*\"";
                requestMessage.Headers.Add("IfMatch", etag);
            }
            else
            {
                InsertEtag(requestMessage, entity);
            }
        }

        static void InsertEtag(HttpWebRequest requestMessage, IETagHolder entity)
        {
            if (entity.ETag.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("The entity does not have its ETag set.");
            }

            string etag = entity.ETag;

            if (etag.IndexOf("\"") != 0)
            {
                etag = "\"" + etag;
            }

            if (etag.LastIndexOf("\"") != etag.Length)
            {
                etag = etag + "\"";
            }

            requestMessage.Headers.Add("IfMatch", etag);
        }

        public void Post(
            string requestUri, 
            object entity, 
            Hashtable customHeaders)
        {
            var webRequest = HttpWebRequest.Create(new Uri(this.baseAddress.OriginalString + requestUri));
            {
                //webRequest.ProtocolVersion = HttpVersion.Version11;
                //webRequest.KeepAlive = true;
                webRequest.Method = "POST";

                // add authorization header
                webRequest.Headers.Add(HttpKnownHeaderNames.Authorization, this.authenticationHeaderProvider.GetAuthorizationHeader());

                // add custom headers
                AddCustomHeaders(webRequest, customHeaders);

                if (entity != null)
                {
                    if (entity.GetType().Equals(typeof(MemoryStream)))
                    {
                        using (StreamReader reader = new StreamReader((MemoryStream)entity))
                        {
                            webRequest.Data = reader.ReadToEnd();
                        }
                    }
                    else if (entity.GetType().Equals(typeof(string)))
                    {
                        webRequest.Data = entity as string;
                        webRequest.ContentType = CommonConstants.BatchedMessageContentType;
                    }
                    else
                    {
                        // TODO
                        // requestMsg.Content = new ObjectContent<T>(entity, new JsonMediaTypeFormatter());
                    }
                }
                else
                {
                }

                // perform request and get response
                using (var webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (webResponse.StatusCode == HttpStatusCode.NoContent)
                    {
                        // success!
                        return;
                    }
                    else
                    {
                        throw new Exception("Post failed");
                    }
                }
            }
        }

        public void Delete(
            string requestUri,
            IETagHolder etag,
            Hashtable customHeaders)
        {
            using (var webRequest = HttpWebRequest.Create(new Uri(this.baseAddress.OriginalString + requestUri)))
            {

                webRequest.Method = "DELETE";

                // add authorization header
                webRequest.Headers.Add("Authorization", this.authenticationHeaderProvider.GetAuthorizationHeader());

                // add custom headers
                AddCustomHeaders(webRequest, customHeaders);

                // add ETag header
                InsertEtag(webRequest, etag);

                // perform request and get response
                using (var webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (webResponse.StatusCode == HttpStatusCode.NoContent)
                    {
                        // success!
                        return;
                    }
                    else
                    {
                        throw new Exception("Delete failed");
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
            }
        }
    }
}

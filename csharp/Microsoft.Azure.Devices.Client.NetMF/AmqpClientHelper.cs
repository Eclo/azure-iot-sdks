// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client
{
    using System;
    using Microsoft.Azure.Devices.Client.Exceptions;

    class AmqpClientHelper
    {
        public static Exception ToIotHubClientContract(Exception exception)
        {
            if (exception is Amqp.TimeoutException)
            {
                return new IotHubCommunicationException(exception.Message);
            }
            // FIX ME
            //else if (exception is UnauthorizedAccessException)
            //{
            //    return new UnauthorizedException(exception.Message);
            //}
            else
            {
                var amqpException = exception as Amqp.AmqpException;
                if (amqpException != null)
                {
                    //return AmqpErrorMapper.ToIotHubClientContract(amqpException.Error);
                    return new Exception("FIXME");
                }

                return exception;
            }
        }
    }
}

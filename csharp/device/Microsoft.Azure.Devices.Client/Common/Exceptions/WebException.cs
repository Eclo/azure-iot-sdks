using System;
using Microsoft.SPOT;
using Eclo.NETMF.SIM800H;

namespace Microsoft.Azure.Devices.Client.Exceptions
{
    /// <devdoc>
    ///    <para>
    ///       Specifies the status of a network request.
    ///    </para>
    /// </devdoc>
    public enum WebExceptionStatus
    {
        /// <devdoc>
        ///    <para>
        ///       A complete response was not received from the remote server.
        ///    </para>
        /// </devdoc>
        ReceiveFailure = 3
    }

    public class WebException : Exception
    {
        private Exception innerException1;
        private HttpWebResponse response;
        private WebExceptionStatus status;

        public WebException()
        {

        }

        public WebException(string message) : this(message, null)
        {
        }

        public WebException(string message, Exception innerException) :
                base(message, innerException)
        {
        }

        public WebException(string message, WebExceptionStatus status) :
                this(message, null, status, null)
        {
        }

        public WebException(string message,
                            Exception innerException,
                            WebExceptionStatus status,
                            HttpWebResponse response) :
            this(message, null, innerException, status, response)
        { }

        public WebException(string message, Exception innerException, Exception innerException1, WebExceptionStatus status, HttpWebResponse response) : this(message, innerException)
        {
            this.innerException1 = innerException1;
            this.status = status;
            this.response = response;
        }

    }
}

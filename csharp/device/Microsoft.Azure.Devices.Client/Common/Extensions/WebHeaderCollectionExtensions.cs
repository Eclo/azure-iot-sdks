using Eclo.NetMF.SIM800H.Http;

namespace Microsoft.Azure.Devices.Client.Extensions
{
    static class WebHeaderCollectionExtensions
    {
        public static void TryGetValues(this WebHeaderCollection s, string name, out string[] values)
        {
            values = s.GetValues(name);
        }
    }
}

using Eclo.NetMF.SIM800H;

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

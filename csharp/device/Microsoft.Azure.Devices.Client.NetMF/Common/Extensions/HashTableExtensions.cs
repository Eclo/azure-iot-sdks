using System.Collections;
namespace Microsoft.Azure.Devices.Client.Extensions
{
    static class HashtableExtensions
    {
        public static void TryGetValues(this Hashtable s, string name, out string[] values)
        {
            if (s[name] != null)
            {
                if (s[name].GetType().Equals(typeof(string)))
                {
                    values = new string[] { s[name] as string };
                    return;
                }
                else if (s[name].GetType().Equals(typeof(string[])))
                {
                    values = s[name] as string[];
                    return;
                }
            }

            values = null;
        }
    }
}

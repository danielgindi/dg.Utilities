using System;
using System.Text;

namespace dg.Utilities
{
    public static class Base64Helper
    {
        public static string Encode(string data, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data), options);
        }

        public static string Decode(string base64, bool ignoreErrors = false)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            }
            catch
            {
                if (ignoreErrors)
                {
                    return "";
                }

                throw;
            }
        }
    }
}

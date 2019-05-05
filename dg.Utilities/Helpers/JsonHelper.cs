using System.Collections;
using System.Globalization;
using System.Text;

namespace dg.Utilities
{
    public static class JsonHelper
    {
        public static void WriteValue(object value, StringBuilder sb)
        {
            if (value == null)
            {
                sb.Append(@"null");
            }
            else if (value is string)
            {
                sb.Append(((string)value).ToJavaScript('"', true));
            }
            else if (value is bool)
            {
                sb.Append((bool)value == true ? @"true" : @"false");
            }
            else if (value is IDictionary)
            { // Must come before IEnumerable
                var dic = (IDictionary)value;
                bool first = true;
                sb.Append('{');

                foreach (object key in dic.Keys)
                {
                    if (!first) sb.Append(',');
                    else first = false;

                    sb.Append(StringHelper.EncodeJavaScript(key.ToString(), '"', true));
                    sb.Append(':');
                    WriteValue(dic[key], sb);
                }
                sb.Append('}');
            }
            else if (value is IEnumerable)
            {
                bool first = true;
                sb.Append('[');

                foreach (object arg in (IEnumerable)value)
                {
                    if (!first) sb.Append(',');
                    else first = false;

                    WriteValue(arg, sb);
                }
                sb.Append(']');
            }
            else
            {
                // Invariant culture, for printing numbers with "." as decimals
                sb.AppendFormat(CultureInfo.InvariantCulture, @"{0}", value);
            }
        }

        public static string Encode(object value)
        {
            StringBuilder sb = new StringBuilder();
            WriteValue(value, sb);
            return sb.ToString();
        }
    }
}

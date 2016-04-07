using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace dg.Utilities
{
    public static class StringHelper
    {
        #region Split

        /// <summary>
        /// Split a string, and give also an array of the whitespace that was stripped
        /// </summary>
        /// <returns></returns>
        public static void Split(
            string source, char[] separator,
            out string[] arrStrings, out string[] arrWhitespace)
        {
            if ((source.Length == 0))
            {
                arrStrings = new string[0];
                arrWhitespace = new string[0];
            }
            int[] sepList = new int[source.Length];
            int numReplaces = MakeSeparatorList(source, separator, ref sepList);
            if ((numReplaces == 0))
            {
                arrStrings = new string[] { source };
                arrWhitespace = new string[] { string.Empty, string.Empty };
            }
            InternalSplitOmitEmptyEntries(source, sepList, null, numReplaces,
                out arrStrings, out arrWhitespace);
        }

        private static int MakeSeparatorList(string source, char[] separator, ref int[] sepList)
        {
            int num = 0;
            int sourceLength = source.Length;
            int length = sepList.Length;
            int separatorCount = separator.Length;
            if ((separator == null) || (separatorCount == 0))
            {
                for (int i = 0; (i < sourceLength) && (num < length); i++)
                {
                    if (char.IsWhiteSpace(source[i]))
                    {
                        sepList[num++] = i;
                    }
                }
                return num;
            }

            for (int j = 0; (j < sourceLength) && (num < length); j++)
            {
                for (int x = 0; x < separatorCount; x++)
                {
                    if (source[j] == separator[x])
                    {
                        sepList[num++] = j;
                        break;
                    }
                }
            }

            return num;
        }

        private static void InternalSplitOmitEmptyEntries(
            string source, int[] sepList, int[] lengthList, int numReplaces,
            out string[] arrStrings, out string[] arrWhitespace)
        {
            int maxStrings = (numReplaces < 0x7fffffff) ? (numReplaces + 1) : 0x7fffffff;
            int maxWhitespace = (maxStrings < 0x7fffffff) ? (maxStrings + 1) : 0x7fffffff;
            string[] arrStrings1 = new string[maxStrings];
            string[] arrWhitespace1 = new string[maxWhitespace];
            int startIndex = 0;
            int lastIndex = -1;
            int actualStringCount = 0;
            for (int i = 0; (i < numReplaces) && (startIndex < source.Length); i++)
            {
                if ((sepList[i] - startIndex) > 0)
                {
                    if (startIndex > 0)
                    {
                        if (lastIndex > -1)
                            arrWhitespace1[actualStringCount] = source.Substring(lastIndex, startIndex - lastIndex);
                        else arrWhitespace1[actualStringCount] = source.Substring(0, startIndex);
                    }
                    else arrWhitespace1[actualStringCount] = string.Empty;
                    arrStrings1[actualStringCount++] = source.Substring(startIndex, sepList[i] - startIndex);
                    lastIndex = sepList[i];
                }
                startIndex = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
                if (actualStringCount == (0x7fffffff - 1))
                {
                    while ((i < (numReplaces - 1)) && (startIndex == sepList[++i]))
                    {
                        startIndex += (lengthList == null) ? 1 : lengthList[i];
                    }
                    break;
                }
            }

            if (startIndex < source.Length)
            {
                if (startIndex > 0)
                {
                    if (lastIndex > -1)
                        arrWhitespace1[actualStringCount] = source.Substring(lastIndex, startIndex - lastIndex);
                    else arrWhitespace1[actualStringCount] = source.Substring(0, startIndex);
                }
                else arrWhitespace1[actualStringCount] = string.Empty;
                arrStrings1[actualStringCount++] = source.Substring(startIndex);
                lastIndex = source.Length;
            }

            if (lastIndex < source.Length)
            {
                arrWhitespace1[actualStringCount] = source.Substring(lastIndex);
            }
            else
            {
                arrWhitespace1[actualStringCount] = string.Empty;
            }

            arrStrings = arrStrings1;
            arrWhitespace = arrWhitespace1;

            if (actualStringCount != maxStrings)
            {
                arrStrings = new string[actualStringCount];
                arrWhitespace = new string[actualStringCount + 1];
                for (int j = 0; j < actualStringCount; j++)
                {
                    arrStrings[j] = arrStrings1[j];
                    arrWhitespace[j] = arrWhitespace1[j];
                }
                arrWhitespace[actualStringCount] = arrWhitespace1[actualStringCount];
            }
        }

        public static string[] SplitWithEscape(string source, char separator, char escape)
        {
            if (source.Length == 0) return new string[0];
            List<string> lstString = new List<string>();
            int len = source.Length;
            bool inEscape = false;
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < len; j++)
            {
                if (source[j] == escape && !inEscape)
                {
                    inEscape = true;
                    continue;
                }
                if (inEscape)
                {
                    sb.Append(source[j]);
                    inEscape = false;
                }
                else
                {
                    if (source[j] == separator)
                    {
                        if (sb.Length > 0)
                        {
                            lstString.Add(sb.ToString());
                            sb = new StringBuilder();
                        }
                    }
                    else
                    {
                        sb.Append(source[j]);
                    }
                }
            }
            if (sb.Length > 0) lstString.Add(sb.ToString());
            return lstString.ToArray();
        }

        #endregion

        #region Split to numbers

        public static int[] SplitToIntegers(string source, params char[] delimiters)
        {
            string[] strs = source.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            int i;
            List<int> lst = new List<int>();
            foreach (string str in strs)
            {
                if (int.TryParse(str, out i)) lst.Add(i);
            }
            return lst.ToArray();
        }

        public static Int64[] SplitToInt64(string source, params char[] delimiters)
        {
            string[] strs = source.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            Int64 i;
            List<Int64> lst = new List<Int64>();
            foreach (string str in strs)
            {
                if (Int64.TryParse(str, out i)) lst.Add(i);
            }
            return lst.ToArray();
        }

        #endregion

        #region Strip Html

        public static string StripHtml(string content, int maxChars, out bool wasCut)
        {
            string stripped = StripHtml(content);
            string cut = CutByWords(stripped, maxChars);
            if (cut.Length < stripped.Length) wasCut = true;
            else wasCut = false;
            return cut;
        }

        public static string StripHtml(string content, int maxChars)
        {
            return CutByWords(StripHtml(content), maxChars);
        }

        public static string StripHtml(string content)
        {
            //Strips the <script> tags from the Html
            content = Regex.Replace(content,
                @"<script[^>.]*>[\s\S]*?</script>", " ",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

            //Strips the <style> tags from the Html
            content = Regex.Replace(content,
                @"<style[^>.]*>[\s\S]*?</style>", " ",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

            //Strips the <!--comment--> tags from the Html	
            content = Regex.Replace(content,
                @"<!(?:--[\s\S]*?--\s*)?>", " ",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

            //Strips the HTML tags from the Html
            content = Regex.Replace(content, @"<(.|\n)+?>", " ", RegexOptions.IgnoreCase);

            //Decode all html entities
            content = HttpUtility.HtmlDecode(content);

            // remove all double spacing
            content = Regex.Replace(content, @"[ \t\r\n\u00a0]+", @" ");

            return content;
        }

        #endregion

        #region Cut by words

        public static string CutByWords(string text, int maxChars)
        {
            if (maxChars > 0 && text.Length > maxChars)
            {
                for (int i = maxChars; i >= 0; i--)
                {
                    if (text[i] == ' ')
                    {
                        text = text.Remove(i);
                        break;
                    }
                }
            }

            return text;
        }

        public static string CutByWords(string text, int maxChars, string suffixWhenCut)
        {
            string cut = CutByWords(text, maxChars);
            if (text.Length != cut.Length) cut += suffixWhenCut;
            return cut;
        }

        #endregion

        #region Random

        /// <summary>
        /// Generates a random string.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="alphaNumericOnly">Should use only alphanumeric characters.</param>
        /// <returns></returns>
        public static string GenerateRandomString(int length, bool alphaNumericOnly)
        {
            string chars;
            if (alphaNumericOnly) chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            else chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!#%&()@${[]}";
            string randomString = "";
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            while (length-- > 0)
            {
                randomString += chars[rnd.Next(chars.Length)];
            }
            return randomString;
        }

        /// <summary>
        /// Generates a random string out of the supplied character string.
        /// </summary>
        /// <param name="possibleCharacters">Possible characters.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GenerateRandomString(string possibleCharacters, int length)
        {
            string randomString = "";
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            while (length-- > 0)
            {
                randomString += possibleCharacters[rnd.Next(possibleCharacters.Length)];
            }
            return randomString;
        }

        /// <summary>
        /// Generates a random string out of the supplied character string.
        /// </summary>
        /// <param name="possibleCharacters">Possible characters.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GenerateRandomString(char[] possibleCharacters, int length)
        {
            string randomString = "";
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            while (length-- > 0)
            {
                randomString += possibleCharacters[rnd.Next(possibleCharacters.Length)];
            }
            return randomString;
        }

        #endregion

        #region Html

        public static string EncodeHtml(string plain, bool newlines = false)
        {
            // The StringBuilder method saves a lot of resources (both memory and processing),
            //   in comparison to the Replace method.
            //   unless the string has absolutely no work to do... 
            //   But we use this when we do expect work to be done.
            if (plain == null) return string.Empty;
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (char c in plain)
                {
                    if (c == '&') sb.Append(@"&amp;");
                    else if (c == '<') sb.Append(@"&lt;");
                    else if (c == '>') sb.Append(@"&gt;");
                    else if (c == '\'') sb.Append(@"&#39;");
                    else if (c == '"') sb.Append(@"&quot;");
                    else sb.Append(c);
                }

                var html = sb.ToString();

                if (newlines)
                {
                    html = html.Replace("\n", @"<br />");
                }

                return html;
            }
        }

        #endregion

        #region Camel case

        public static string UnCamelCase(string camelCase)
        {
            char[] letters = camelCase.ToCharArray();
            string sOut = "";
            foreach (char c in letters)
            {
                if (c.ToString() != c.ToString().ToLower())
                {
                    //it's uppercase, add a space
                    sOut += " " + c.ToString();
                }
                else
                {
                    sOut += c.ToString();
                }
            }
            return sOut;
        }

        #endregion

        #region JavaScript

        private static string ToCharAsUnicode(char c)
        {
            char h1 = IntToHex((c >> 12) & '\x000f');
            char h2 = IntToHex((c >> 8) & '\x000f');
            char h3 = IntToHex((c >> 4) & '\x000f');
            char h4 = IntToHex(c & '\x000f');
            return new string(new[] { '\\', 'u', h1, h2, h3, h4 });
        }

        private static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 48);
            }
            return (char)((n - 10) + 97);
        }

        public static string EncodeJavaScript(string plain, char delimiter, bool appendDelimiters)
        {
            StringBuilder sb = new StringBuilder();

            // leading delimiter
            if (appendDelimiters) sb.Append(delimiter);

            if (plain != null)
            {
                int lastWritePosition = 0;
                int skipped = 0;
                char[] chars = null;

                for (int i = 0; i < plain.Length; i++)
                {
                    char c = plain[i];
                    string escapedValue;

                    switch (c)
                    {
                        case '\t':
                            escapedValue = @"\t";
                            break;
                        case '\n':
                            escapedValue = @"\n";
                            break;
                        case '\r':
                            escapedValue = @"\r";
                            break;
                        case '\f':
                            escapedValue = @"\f";
                            break;
                        case '\b':
                            escapedValue = @"\b";
                            break;
                        case '\\':
                            escapedValue = @"\\";
                            break;
                        case '\u0085': // Next Line
                            escapedValue = @"\u0085";
                            break;
                        case '\u2028': // Line Separator
                            escapedValue = @"\u2028";
                            break;
                        case '\u2029': // Paragraph Separator
                            escapedValue = @"\u2029";
                            break;
                        case '\'':
                            // only escape if this charater is being used as the delimiter
                            escapedValue = (delimiter == '\'') ? @"\'" : null;
                            break;
                        case '"':
                            // only escape if this charater is being used as the delimiter
                            escapedValue = (delimiter == '"') ? "\\\"" : null;
                            break;
                        default:
                            escapedValue = (c <= '\u001f') ? ToCharAsUnicode(c) : null;
                            break;
                    }

                    if (escapedValue != null)
                    {
                        if (chars == null)
                            chars = plain.ToCharArray();

                        // write skipped text
                        if (skipped > 0)
                        {
                            sb.Append(chars, lastWritePosition, skipped);
                            skipped = 0;
                        }

                        // write escaped value and note position
                        sb.Append(escapedValue);
                        lastWritePosition = i + 1;
                    }
                    else
                    {
                        skipped++;
                    }
                }

                // write any remaining skipped text
                if (skipped > 0)
                {
                    if (lastWritePosition == 0)
                        sb.Append(plain);
                    else
                        sb.Append(chars, lastWritePosition, skipped);
                }
            }

            // trailing delimiter
            if (appendDelimiters) sb.Append(delimiter);

            return sb.ToString();
        }

        #endregion

        #region Email

        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[A-Z0-9\._%+\-]+@[A-Z0-9\-]+(\.[A-Z0-9\-]+)*$", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
        }

        /// <summary>
        /// Normalizes an email
        /// Lowers the case
        /// Considers gmail "dot" and "plus" tricks
        /// </summary>
        /// <param name="email">input email address</param>
        /// <returns>normalized email address, or null if invalid</returns>
        public static string NormalizeEmail(string email)
        {
            email = email.Trim().ToLowerInvariant();
            if (!IsValidEmail(email)) return null;
            int idx = email.IndexOf(@"@");
            string un = email.Substring(0, idx);
            string domain = email.Substring(idx);
            if (domain == @"@gmail.com")
            {
                un = un.Replace(@".", "");
                idx = un.IndexOf(@"+");
                if (idx >= 0) un = un.Substring(0, idx);
            }
            return un + domain;
        }

        #endregion

        #region Levenshtein Distance

        /// <summary>
        /// Computes a distance between two strings (count of single actions taken to get from string A to string B)
        /// </summary>
        /// <param name="fromString">from string</param>
        /// <param name="toString">to string</param>
        /// <returns>distance</returns>
        public static int LevenshteinDistance(string fromString, string toString)
        {
            int n = fromString.Length;
            int m = toString.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (toString[j - 1] == fromString[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = System.Math.Min(
                        System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        #endregion

        #region Enums

        public static T ParseAsEnum<T>(string value)
        {
            if (value == null || value.Length == 0)
            {
                throw new ArgumentNullException
                    ("Can't parse an empty string");
            }

            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException("The type is not an Enum");
            }

            // warning, can throw
            return (T)Enum.Parse(enumType, value);
        }

        #endregion

        #region Sharp replacer

        public delegate string SharpCodeValueDelegate(string code);

        /// <summary>
        /// Replaces codes in the strings with the values from the dictionary.
        /// Code may appear in the #CODE# form.
        /// Use double sharp (##) to escape where a literal is required.
        /// Keys that do not appear in the dictionary - will not be replaced
        /// </summary>
        /// <param name="text">The input text</param>
        /// <param name="keyValueMap">Dictionary of the code->value mappings</param>
        /// <param name="preserveNotFoundValues">Dictionary of the code->value mappings</param>
        /// <returns>Processed data after replacing the sharps with the corresponding values</returns>
        public static string ReplaceSharps(string input, Dictionary<string, string> keyValueMap, bool preserveNotFoundValues = true)
        {
            if (input == null || input.Length == 0) return input;

            return input.ReplaceSharps(key => { string value = null; keyValueMap.TryGetValue(key, out value); return value; }, preserveNotFoundValues);
        }

        /// <summary>
        /// Replaces codes in the strings with the values from the dictionary.
        /// Code may appear in the #CODE# form.
        /// Use double sharp (##) to escape where a literal is required.
        /// Keys that do not appear in the dictionary - will not be replaced
        /// </summary>
        /// <param name="text">The input text</param>
        /// <param name="supplier">Supplier of values for specific codes</param>
        /// <param name="preserveNotFoundValues">Dictionary of the code->value mappings</param>
        /// <returns>Processed data after replacing the sharps with the corresponding values</returns>
        public static string ReplaceSharps(string input, SharpCodeValueDelegate supplier, bool preserveNotFoundValues = true)
        {
            if (input == null || input.Length == 0) return input;

            StringBuilder sb = new StringBuilder();
            int firstSharp = -1;

            char c;
            for (int j = 0; j < input.Length; j++)
            {
                c = input[j];
                if (c == '#')
                {
                    if (firstSharp == -1)
                    {
                        firstSharp = j;
                    }
                    else if (firstSharp == j - 1)
                    { // Convert ## to #, to allow escaping those #
                        firstSharp = -1;
                        sb.Append(c);
                    }
                    else
                    {
                        string value = null;
                        if (j - firstSharp > 1)
                        {
                            if (supplier != null)
                            {
                                value = supplier(input.Substring(firstSharp + 1, j - firstSharp - 1));
                            }
                            if (value != null)
                            {
                                sb.Append(value);
                            }
                        }
                        if (value != null || !preserveNotFoundValues)
                        {
                            firstSharp = -1;
                        }
                        else
                        {
                            sb.Append(input.Substring(firstSharp, j - firstSharp));
                            firstSharp = j;
                        }
                    }
                }
                else if (firstSharp == -1)
                {
                    sb.Append(c);
                }
            }
            if (firstSharp > -1)
            {
                sb.Append(input.Substring(firstSharp));
            }

            return sb.ToString();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities
{
    public static class StringExtensions
    {
        public static void Split(this string input, char[] separator,
            out string[] arrStrings, out string[] arrWhitespace)
        {
            Strings.Split(input, separator, out arrStrings, out arrWhitespace);
        }
        public static string[] SplitWithEscape(this string input, char separator, char escape)
        {
            return Strings.SplitWithEscape(input, separator, escape);
        }

        public static int[] SplitToIntegers(this string source, params char[] delimiters)
        {
            return Strings.SplitToIntegers(source, delimiters);
        }
        public static Int64[] SplitToInt64(this string source, params char[] delimiters)
        {
            return Strings.SplitToInt64(source, delimiters);
        }

        public static string StripHtml(this string input, int maxChars, out bool wasCut)
        {
            return Strings.StripHtml(input, maxChars, out wasCut);
        }
        public static string StripHtml(this string input, int maxChars)
        {
            return Strings.StripHtml(input, maxChars);
        }
        public static string StripHtml(this string input)
        {
            return Strings.StripHtml(input);
        }
        public static string CutByWords(this string input, int maxChars)
        {
            return Strings.CutByWords(input, maxChars);
        }
        public static string CutByWords(this string input, int maxChars, string suffixWhenCut)
        {
            string cut = Strings.CutByWords(input, maxChars);
            if (input.Length != cut.Length) cut += suffixWhenCut;
            return cut;
        }
        public static string RandomString(this string input, int length)
        {
            return Strings.GenerateRandomString(input, length);
        }
        public static string ToHtml(this string input)
        {
            return Strings.HtmlEncode(input);
        }

        public static string UnCamelCase(this string input)
        {
            return Strings.UnCamelCase(input);
        }

        private static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 48);
            }
            return (char)((n - 10) + 97);
        }
        private static string ToCharAsUnicode(char c)
        {
            char h1 = IntToHex((c >> 12) & '\x000f');
            char h2 = IntToHex((c >> 8) & '\x000f');
            char h3 = IntToHex((c >> 4) & '\x000f');
            char h4 = IntToHex(c & '\x000f');
            return new string(new[] { '\\', 'u', h1, h2, h3, h4 });
        }
        public static string ToJavaScript(this string input, char delimiter, bool appendDelimiters)
        {
            StringBuilder sb = new StringBuilder();

            // leading delimiter
            if (appendDelimiters) sb.Append(delimiter);

            if (input != null)
            {
                int lastWritePosition = 0;
                int skipped = 0;
                char[] chars = null;

                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
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
                            chars = input.ToCharArray();

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
                        sb.Append(input);
                    else
                        sb.Append(chars, lastWritePosition, skipped);
                }
            }

            // trailing delimiter
            if (appendDelimiters) sb.Append(delimiter);

            return sb.ToString();
        }
        public static bool IsValidEmail(this string input)
        {
            return Strings.IsValidEmail(input);
        }
        public static bool IsValidURL(this string input)
        {
            return Strings.IsValidURL(input);
        }
        public static bool IsValidURL(this string input, string ext, StringComparison comparisonType)
        {
            return Strings.IsValidURL(input, ext, comparisonType);
        }
        public static bool IsValidURL(this string input, string ext)
        {
            return Strings.IsValidURL(input, ext);
        }
        public static T ParseAsEnum<T>(this string value)
        {
            if (value==null ||value.Length ==0)
            {
                throw new ArgumentNullException
                    ("Can't parse an empty string");
            }

            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException ("The type is not an Enum");
            }

            // warning, can throw
            return (T)Enum.Parse(enumType, value);
        }

        /// <summary>
        /// Normalizes an email
        /// Lowers the case
        /// Considers gmail "dot" and "plus" tricks
        /// </summary>
        /// <param name="input">input email address</param>
        /// <returns>normalized email address, or null if invalid</returns>
        public static string NormalizeEmail(this string input)
        {
            return Strings.NormalizeEmail(input);
        }

        /// <summary>
        /// Computes a distance between two strings (count of single actions taken to get from string A to string B)
        /// </summary>
        /// <param name="destString">to string</param>
        /// <returns>distance</returns>
        public static int DistanceTo(this string input, string destString)
        {
            return Strings.LevenshteinDistance(input, destString);
        }

        /// <summary>
        /// Computes a distance between two strings (count of single actions taken to get from string A to string B)
        /// </summary>
        /// <param name="fromString">from string</param>
        /// <returns>distance</returns>
        public static int DistanceFrom(this string input, string fromString)
        {
            return Strings.LevenshteinDistance(fromString, input);
        }
    }
}

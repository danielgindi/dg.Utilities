using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace dg.Utilities
{
    public static class Strings
    {
        [Obsolete]
        public static void Split(string source, char[] separator,
            out string[] arrStrings, out string[] arrWhitespace)
        {
            StringHelper.Split(source, separator, out arrStrings, out arrWhitespace);
        }

        [Obsolete]
        public static string[] SplitWithEscape(string source, char separator, char escape)
        {
            return StringHelper.SplitWithEscape(source, separator, escape);
        }

        [Obsolete]
        public static int[] SplitToIntegers(string source, params char[] delimiters)
        {
            return StringHelper.SplitToIntegers(source, delimiters);
        }

        [Obsolete]
        public static Int64[] SplitToInt64(string source, params char[] delimiters)
        {
            return StringHelper.SplitToInt64(source, delimiters);
        }

        [Obsolete]
        public static string StripHtml(string content, int maxChars, out bool wasCut)
        {
            return StringHelper.StripHtml(content, maxChars, out wasCut);
        }

        [Obsolete]
        public static string StripHtml(string content, int maxChars)
        {
            return StringHelper.StripHtml(content, maxChars);
        }

        [Obsolete]
        public static string StripHtml(string content)
        {
            return StringHelper.StripHtml(content);
        }

        [Obsolete]
        public static string CutByWords(string text, int maxChars)
        {
            return StringHelper.CutByWords(text, maxChars);
        }

        [Obsolete]
        public static string GenerateRandomString(int length, bool alphaNumericOnly)
        {
            return StringHelper.GenerateRandomString(length, alphaNumericOnly);
        }

        [Obsolete]
        public static string GenerateRandomString(string possibleCharacters, int length)
        {
            return StringHelper.GenerateRandomString(possibleCharacters, length);
        }

        [Obsolete]
        public static string GenerateRandomString(char[] possibleCharacters, int length)
        {
            return StringHelper.GenerateRandomString(possibleCharacters, length);
        }

        [Obsolete]
        public static string HtmlEncode(string plain)
        {
            return StringHelper.EncodeHtml(plain);
        }

        [Obsolete]
        public static bool IsValidEmail(string email)
        {
            return StringHelper.IsValidEmail(email);
        }

        [Obsolete]
        public static string NormalizeEmail(string email)
        {
            return StringHelper.NormalizeEmail(email);
        }

        [Obsolete]
        public static bool IsValidURL(string url)
        {
            return UriHelper.IsValidURL(url);
        }

        [Obsolete]
        public static bool IsValidURL(string url, string ext)
        {
            return UriHelper.IsValidURL(url, ext);
        }

        [Obsolete]
        public static bool IsValidURL(string url, string ext, StringComparison comparisonType)
        {
            return UriHelper.IsValidURL(url, ext, comparisonType);
        }

        [Obsolete]
        public static string UnCamelCase(string camelCase)
        {
            return StringHelper.UnCamelCase(camelCase);
        }

        [Obsolete]
        public static int LevenshteinDistance(string fromString, string toString)
        {
            return StringHelper.LevenshteinDistance(fromString, toString);
        }
    }
}

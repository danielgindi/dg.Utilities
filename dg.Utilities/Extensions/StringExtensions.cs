using System;
using System.Collections.Generic;

namespace dg.Utilities
{
    public static class StringExtensions
    {
        #region Split

        public static void Split(this string input, char[] separator,
            out string[] arrStrings, out string[] arrWhitespace)
        {
            StringHelper.Split(input, separator, out arrStrings, out arrWhitespace);
        }

        public static string[] SplitWithEscape(this string input, char separator, char escape)
        {
            return StringHelper.SplitWithEscape(input, separator, escape);
        }

        public static int[] SplitToIntegers(this string source, params char[] delimiters)
        {
            return StringHelper.SplitToIntegers(source, delimiters);
        }

        public static Int64[] SplitToInt64(this string source, params char[] delimiters)
        {
            return StringHelper.SplitToInt64(source, delimiters);
        }

        #endregion

        #region Strip Html

        public static string StripHtml(this string input, int maxChars, out bool wasCut)
        {
            return StringHelper.StripHtml(input, maxChars, out wasCut);
        }

        public static string StripHtml(this string input, int maxChars)
        {
            return StringHelper.StripHtml(input, maxChars);
        }

        public static string StripHtml(this string input)
        {
            return StringHelper.StripHtml(input);
        }

        #endregion

        #region Cut by words

        public static string CutByWords(this string input, int maxChars)
        {
            return StringHelper.CutByWords(input, maxChars);
        }

        public static string CutByWords(this string input, int maxChars, string suffixWhenCut)
        {
            return StringHelper.CutByWords(input, maxChars, suffixWhenCut);
        }

        #endregion

        #region Random

        public static string RandomString(this string input, int length)
        {
            return StringHelper.GenerateRandomString(input, length);
        }

        #endregion

        #region Html

        public static string ToHtml(this string input)
        {
            return StringHelper.EncodeHtml(input);
        }

        #endregion

        #region Camel case

        public static string UnCamelCase(this string input)
        {
            return StringHelper.UnCamelCase(input);
        }

        #endregion

        #region JavaScript

        public static string ToJavaScript(this string input, char delimiter, bool appendDelimiters)
        {
            return StringHelper.EncodeJavaScript(input, delimiter, appendDelimiters);
        }

        #endregion

        #region Email

        public static bool IsValidEmail(this string input)
        {
            return StringHelper.IsValidEmail(input);
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
            return StringHelper.NormalizeEmail(input);
        }

        #endregion

        #region Urls

        public static bool IsValidURL(this string input)
        {
            return UriHelper.IsValidURL(input);
        }

        public static bool IsValidURL(this string input, string ext)
        {
            return UriHelper.IsValidURL(input, ext);
        }

        public static bool IsValidURL(this string input, string ext, StringComparison comparisonType)
        {
            return UriHelper.IsValidURL(input, ext, comparisonType);
        }

        #endregion

        #region Levenshtein Distance

        /// <summary>
        /// Computes a distance between two strings (count of single actions taken to get from string A to string B)
        /// </summary>
        /// <param name="destString">to string</param>
        /// <returns>distance</returns>
        public static int DistanceTo(this string input, string destString)
        {
            return StringHelper.LevenshteinDistance(input, destString);
        }

        /// <summary>
        /// Computes a distance between two strings (count of single actions taken to get from string A to string B)
        /// </summary>
        /// <param name="fromString">from string</param>
        /// <returns>distance</returns>
        public static int DistanceFrom(this string input, string fromString)
        {
            return StringHelper.LevenshteinDistance(fromString, input);
        }

        #endregion

        #region Sharp replacer

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
        public static string ReplaceSharps(this string input, Dictionary<string, string> keyValueMap, bool preserveNotFoundValues = true)
        {
            return StringHelper.ReplaceSharps(input, keyValueMap, preserveNotFoundValues);
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
        public static string ReplaceSharps(this string input, StringHelper.SharpCodeValueDelegate supplier, bool preserveNotFoundValues = true)
        {
            return StringHelper.ReplaceSharps(input, supplier, preserveNotFoundValues);
        }

        #endregion
    }
}

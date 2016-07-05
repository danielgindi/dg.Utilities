using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace dg.Utilities
{
    public static class UriHelper
    {
        /// <summary>
        /// Escapes a uri safely.
        /// If the uri is null - a null will be returned.
        /// If the uri is invalid - it will be returned as it is.
        /// If the uri is valid - it will be correctly escaped using <code>Uri.EscapeUriString(...)</code>
        /// 
        /// In any case - no exceptions will be thrown
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string EscapeUri(string uri)
        {
            if (uri == null) return null;
            if (uri.Length == 0) return "";

            try
            {
                return Uri.EscapeUriString(uri);
            }
            catch
            {
                return uri;
            }
        }

        public static bool IsValidURL(string url)
        {
            return Regex.IsMatch(url, @"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&%\$#\=~])*[^\.\,\)\(\s]$");
        }

        public static bool IsValidURL(string url, string ext)
        {
            return IsValidURL(url, ext, StringComparison.Ordinal);
        }

        public static bool IsValidURL(string url, string ext, StringComparison comparisonType)
        {
            ext = ext.Trim();
            if (ext.StartsWith(@"."))
            {
                return url.EndsWith(ext, comparisonType) && IsValidURL(url);
            }
            else
            {
                return url.EndsWith(@"." + ext, comparisonType) && IsValidURL(url);
            }
        }

        /// <summary>
        /// Resolves a relative URL with ~ character or simple relative url
        /// Same as Control.ResolveUrl
        /// </summary>
        public static string ResolveUrl(string relativeUrl)
        {
            if (relativeUrl == null)
                throw new ArgumentNullException("relativeUrl");

            if (relativeUrl.Length == 0 || relativeUrl[0] == '/' || relativeUrl[0] == '\\')
                return relativeUrl;

            int idxOfScheme = relativeUrl.IndexOf(@"://", StringComparison.Ordinal);
            if (idxOfScheme != -1)
            {
                int idxOfQM = relativeUrl.IndexOf('?');
                if (idxOfQM == -1 || idxOfQM > idxOfScheme) return relativeUrl;
            }

            StringBuilder sbUrl = new StringBuilder();
            sbUrl.Append(HttpRuntime.AppDomainAppVirtualPath);
            if (sbUrl.Length == 0 || sbUrl[sbUrl.Length - 1] != '/')
            {
                sbUrl.Append('/');
            }

            bool foundQM = false; // found question mark already? query string, do not touch!
            bool foundSlash; // the latest char was a slash?

            if (relativeUrl.Length > 1
                && relativeUrl[0] == '~'
                && (relativeUrl[1] == '/' || relativeUrl[1] == '\\'))
            {
                relativeUrl = relativeUrl.Substring(2);
                foundSlash = true;
            }
            else
            {
                foundSlash = false;
            }

            foreach (char c in relativeUrl)
            {
                if (!foundQM)
                {
                    if (c == '?')
                    {
                        // Stop "processing", and pass the query string as-is.
                        foundQM = true;
                    }
                    else
                    {
                        // Strip extra slashes
                        if (c == '/' || c == '\\')
                        {
                            if (foundSlash) continue;
                            else
                            {
                                sbUrl.Append('/');
                                foundSlash = true;
                                continue;
                            }
                        }
                        else if (foundSlash)
                        {
                            foundSlash = false;
                        }
                    }
                }
                sbUrl.Append(c);
            }

            return sbUrl.ToString();
        }

        /// <summary>
        /// Will check if the given url is "local", which is relative to the domain of current request url, or same domain.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            Uri absoluteUri;
            if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri))
            {
                return String.Equals(HttpContext.Current.Request.Url.Host, absoluteUri.Host, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return !url.Contains(@"://");
            }
        }

        public static class Encoding
        {
            public static bool IsNonAsciiByte(byte b)
            {
                if (b < 0x7f)
                {
                    return (b < 0x20);
                }
                return true;
            }

            /// <summary>
            /// Url-Encode non-ascii characters only
            /// </summary>
            public static string UrlEncodeNonAscii(string str, System.Text.Encoding e)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return str;
                }
                if (e == null)
                {
                    e = System.Text.Encoding.UTF8;
                }
                byte[] bytes = e.GetBytes(str);
                bytes = UrlEncodeBytesToBytesInternalNonAscii(bytes, 0, bytes.Length, false);
                return System.Text.Encoding.ASCII.GetString(bytes);
            }

            internal static byte[] UrlEncodeBytesToBytesInternalNonAscii(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
            {
                int num = 0;
                for (int i = 0; i < count; i++)
                {
                    if (IsNonAsciiByte(bytes[offset + i]))
                    {
                        num++;
                    }
                }
                if (!alwaysCreateReturnValue && (num == 0))
                {
                    return bytes;
                }
                byte[] buffer = new byte[count + (num * 2)];
                int num3 = 0;
                byte bhex;
                for (int j = 0; j < count; j++)
                {
                    byte b = bytes[offset + j];
                    if (IsNonAsciiByte(b))
                    {
                        buffer[num3++] = 0x25;
                        bhex = (byte)((b >> 4) & 15);
                        buffer[num3++] = (bhex <= 9) ? (byte)(bhex + 0x30) : (byte)((bhex - 10) + 0x61);
                        bhex = (byte)(b & 15);
                        buffer[num3++] = (bhex <= 9) ? (byte)(bhex + 0x30) : (byte)((bhex - 10) + 0x61);
                    }
                    else
                    {
                        buffer[num3++] = b;
                    }
                }
                return buffer;
            }
        }
    }
}

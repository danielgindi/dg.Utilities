﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace dg.Utilities
{
    public static class Http
    {
        /// <summary>
        /// Ends response with 404 status code
        /// <param name="endResponse">Will end the response</param>
        /// </summary>
        public static void Respond404(bool endResponse)
        {
            HttpContext.Current.Response.StatusCode = 404;
            if (endResponse)
            {
                System.Configuration.Configuration webConfig = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
                CustomErrorsSection customErrors3 = (CustomErrorsSection)webConfig.GetSection("system.web/customErrors");
                CustomErrorsSection customErrors4 = (CustomErrorsSection)webConfig.GetSection("system.webServer/httpErrors");
                if (customErrors3 != null || customErrors4 != null)
                {
                    CustomError customError = customErrors4 != null ? customErrors4.Errors.Get(@"404") : null;
                    if (customError == null) customError = customErrors3 != null ? customErrors3.Errors.Get(@"404") : null;
                    if (customError != null)
                    {
                        RedirectPermanent(ResolveUrl(customError.Redirect), true);
                    }
                    else
                    {
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.ClearHeaders();
                        HttpContext.Current.Response.End();
                    }
                }
            }
        }

        /// <summary>
        /// Ends response with 404 status code
        /// </summary>
        public static void Respond404()
        {
            Respond404(true);
        }

        /// <summary>
        /// Redirects with permanent 301 status code
        /// </summary>
        public static void RedirectPermanent(string url)
        {
            RedirectPermanent(url, true);
        }

        /// <summary>
        /// Url-Encode spaces only
        /// </summary>
        public static string UrlEncodeSpaces(string str)
        {
            if ((str != null) && (str.IndexOf(' ') >= 0))
            {
                str = str.Replace(" ", "%20");
            }
            return str;
        }

        /// <summary>
        /// Redirects with permanent 301 status code
        /// </summary>
        public static void RedirectPermanent(string url, bool endResponse)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            if (url.IndexOf('\n') >= 0)
            {
                throw new ArgumentException("Redirect string can't contain newline characters");
            }

            HttpResponse Response = HttpContext.Current.Response;
            HttpRequest Request = HttpContext.Current.Request;

            Response.ClearHeaders();

            url = Response.ApplyAppPathModifier(url);
            if (Request.Browser["requiresFullyQualifiedRedirectUrl"] == "true")
            {
                url = new Uri(Request.Url, url).AbsoluteUri;
            }

            int index = url.IndexOf('?');
            if (index >= 0)
            {
                System.Text.Encoding e = (Request != null) ? Request.ContentEncoding : Response.ContentEncoding;
                url = UrlEncodeSpaces(Encoding.UrlEncodeNonAscii(url.Substring(0, index), System.Text.Encoding.UTF8)) + Encoding.UrlEncodeNonAscii(url.Substring(index), e);
            }
            else
            {
                url = UrlEncodeSpaces(Encoding.UrlEncodeNonAscii(url, System.Text.Encoding.UTF8));
            }

            Response.Clear();
            Response.Status = "301 Moved Permanently";
            HttpContext.Current.Response.AddHeader("Location", url);

            // html output
            if ((url.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)) || ((url.StartsWith("ftp:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("file:", StringComparison.OrdinalIgnoreCase)) || url.StartsWith("news:", StringComparison.OrdinalIgnoreCase)))
            {
                url = HttpUtility.HtmlAttributeEncode(url);
            }
            else
            {
                url = HttpUtility.HtmlAttributeEncode(HttpUtility.UrlEncode(url));
            }
            Response.Write("<html><head><title>Object moved</title></head><body>\r\n");
            Response.Write("<h2>Object moved to <a href=\"" + url + "\">here</a>.</h2>\r\n");
            Response.Write("</body></html>\r\n");

            // end response?
            if (endResponse) HttpContext.Current.Response.End();
        }


        /// <summary>
        /// Resolves a relative URL with ~ character or simple relative url
        /// Same as Control.ResolveUrl
        /// </summary>
        public static string ResolveUrl(string relativeUrl)
        {
            if (relativeUrl == null) throw new ArgumentNullException("relativeUrl");

            if (relativeUrl.Length == 0 || relativeUrl[0] == '/' || relativeUrl[0] == '\\') return relativeUrl;

            int idxOfScheme = relativeUrl.IndexOf(@"://", StringComparison.Ordinal);
            if (idxOfScheme != -1)
            {
                int idxOfQM = relativeUrl.IndexOf('?');
                if (idxOfQM == -1 || idxOfQM > idxOfScheme) return relativeUrl;
            }

            StringBuilder sbUrl = new StringBuilder();
            sbUrl.Append(HttpRuntime.AppDomainAppVirtualPath);
            if (sbUrl.Length == 0 || sbUrl[sbUrl.Length - 1] != '/') sbUrl.Append('/');

            bool foundQM = false; // found question mark already? query string, do not touch!
            bool foundSlash; // the latest char was a slash?
            if (relativeUrl.Length > 1
                && relativeUrl[0] == '~'
                && (relativeUrl[1] == '/' || relativeUrl[1] == '\\'))
            {
                relativeUrl = relativeUrl.Substring(2);
                foundSlash = true;
            }
            else foundSlash = false;
            foreach (char c in relativeUrl)
            {
                if (!foundQM)
                {
                    if (c == '?') foundQM = true;
                    else
                    {
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
                        else if (foundSlash) foundSlash = false;
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

        static public class Encoding
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

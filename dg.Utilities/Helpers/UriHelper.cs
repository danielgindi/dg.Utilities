using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO.Compression;

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
    }
}

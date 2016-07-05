using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Text.RegularExpressions;

namespace dg.Utilities
{
    public static class Http
    {
        [Obsolete]
        public static void Respond404(bool endResponse)
        {
            HttpResponseHelper.Respond404(endResponse);
        }

        [Obsolete]
        public static void Respond404()
        {
            HttpResponseHelper.Respond404();
        }

        [Obsolete]
        public static void RedirectPermanent(string url)
        {
            HttpResponseHelper.RedirectPermanent(url);
        }
        
        [Obsolete]
        public static void RedirectPermanent(string url, bool endResponse)
        {
            HttpResponseHelper.RedirectPermanent(url, endResponse);
        }

        [Obsolete]
        public static string ResolveUrl(string relativeUrl)
        {
            return UriHelper.ResolveUrl(relativeUrl);
        }

        [Obsolete]
        public static bool IsLocalUrl(string url)
        {
            return UriHelper.IsLocalUrl(url);
        }

        [Obsolete]
        public static bool IsCurrentRequestFromMobileBrowser()
        {
            return HttpRequestHelper.IsCurrentRequestFromMobileBrowser();
        }

        [Obsolete]
        public static bool IsNonCrawlerBrowser(string userAgent)
        {
            return HttpRequestHelper.IsNonCrawlerBrowser(userAgent);
        }

        [Obsolete]
        public static bool IsCrawlerBrowser(string userAgent)
        {
            return HttpRequestHelper.IsCrawlerBrowser(userAgent);
        }
    }
}

using System;
using System.Web;
using System.IO.Compression;
using System.Web.Configuration;

namespace dg.Utilities
{
    public static class HttpResponseHelper
    {
        public static void EnableCompressionInResponseToRequest(HttpResponse response, HttpRequest request)
        {
            if ((request.Headers["Accept-Encoding"] ?? "").ToLower().Contains("gzip"))
            {
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                response.AppendHeader("Content-Encoding", "gzip");
            }
        }

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
                        RedirectPermanent(UriHelper.ResolveUrl(customError.Redirect), true);
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
                url = UriHelper.EscapeUri(UriHelper.Encoding.UrlEncodeNonAscii(url.Substring(0, index), System.Text.Encoding.UTF8)) + UriHelper.Encoding.UrlEncodeNonAscii(url.Substring(index), e);
            }
            else
            {
                url = UriHelper.EscapeUri(UriHelper.Encoding.UrlEncodeNonAscii(url, System.Text.Encoding.UTF8));
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
    }
}

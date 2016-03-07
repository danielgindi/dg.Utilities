using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO.Compression;

namespace dg.Utilities
{
    public static class HttpResponseHelpers
    {
        public static void EnableCompressionInResponseToRequest(HttpResponse response, HttpRequest request)
        {
            if ((request.Headers["Accept-Encoding"] ?? "").ToLower().Contains("gzip"))
            {
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                response.AppendHeader("Content-Encoding", "gzip");
            }
        }
    }
}

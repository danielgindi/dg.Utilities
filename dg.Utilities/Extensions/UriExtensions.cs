using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace dg.Utilities
{
    public static class UriExtensions
    {
        public static string GetSchemeHostPort(this Uri input)
        {
            return input.Scheme + "://" + input.Host + ":" + input.Port + "/";
        }
    }
}

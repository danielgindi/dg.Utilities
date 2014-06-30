using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace dg.Utilities.WebApiServices
{
    public struct RestHandlerRoute
    {
        public RestHandlerRoute(string route, IRestHandlerTarget target)
        {
            this.Pattern = RouteToRegex(route);
            this.Target = target;
        }

        public RestHandlerRoute(Regex pattern, IRestHandlerTarget target)
        {
            this.Pattern = pattern;
            this.Target = target;
        }

        public static RestHandlerRoute FromOldRouteFormat(string route, IRestHandlerTarget target)
        {
            if (route.StartsWith(@"/")) route = route.Remove(0, 1);

            route = Regex.Replace(route, @"#match:([^/]*)", m => "(" + m.Groups[1].Captures[0].Value.Trim('^', '$') + ")"); // match:...
            route = route.Replace(@"#*", @"([^/]+)"); // /#*
            route = Regex.Replace(route, @"#([^/]+)", m => "(" + Regex.Escape(m.Groups[1].Captures[0].Value) + ")"); // pass through path parts
            route += @"/?$";

            return new RestHandlerRoute(new Regex('^' + route, RegexOptions.ECMAScript | RegexOptions.Compiled), target);
        }

        public Regex Pattern;
        public IRestHandlerTarget Target;

        /// <summary>
        ///  Backbone style routes, most credit to Backbone authors.
        /// </summary>
        static Regex optionalParam = new Regex(@"\((.*?)\)", RegexOptions.ECMAScript | RegexOptions.Compiled);
        static Regex namedParam = new Regex(@"(\(\?)?:\w+", RegexOptions.ECMAScript | RegexOptions.Compiled);
        static Regex namedNumericParam = new Regex(@"(\(\?)?#\w+", RegexOptions.ECMAScript | RegexOptions.Compiled);
        static Regex splatParam = new Regex(@"\*\w+", RegexOptions.ECMAScript | RegexOptions.Compiled);
        static Regex escapeRegExp = new Regex(@"[\-{}\[\]+?.,\\\^$|\s]", RegexOptions.ECMAScript | RegexOptions.Compiled);

        internal static Regex RouteToRegex(string route)
        {
            if (route.StartsWith(@"/")) route = route.Remove(0, 1);

            route = splatParam.Replace(
                namedParam.Replace(
                namedNumericParam.Replace(
                optionalParam.Replace(
                escapeRegExp.Replace(route, @"\$&"), // Escape regex while leaving our special characters intact
                @"(?:$1)?"), // optionalParam
                m => m.Groups[1].Captures.Count == 1 ? m.Value : @"([0-9]+)"), // namedNumericParam
                m => m.Groups[1].Captures.Count == 1 ? m.Value : @"([^/]+)"), // namedParam
                @"([^?]*?)"); // splatParam
            return new Regex('^' + route + @"/?$", RegexOptions.ECMAScript | RegexOptions.Compiled);
        }
    }
}

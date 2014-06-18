using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net;

namespace dg.Utilities.WebApiServices
{
    /// <summary>
    /// Supports Backbone-style with some additional sugar.
    /// 
    /// :name_param
    /// :named_digits_param
    /// *wildcard_including_backslashes
    /// (optional_param)
    /// 
    /// Examples:
    /// 
    /// help => help
    /// search/:query => search/kiwis
    /// search/:query/p:page => search/kiwis/p7 and search/kiwis/plast
    /// search/:query/p#page => search/kiwis/p7
    /// file/*path => file/nested/folder/file.txt
    /// docs/:section(/:subsection) => docs/faq and docs/faq/installing
    /// </summary>
    public abstract class RestHandler : IHttpHandler
    {
        public RestHandler(Dictionary<string, List<RestHandlerRoute>> routes)
        {
            this.Routes = routes;
        }
        public RestHandler()
        {
        }

        public bool IsReusable { get { return true; } }

        public virtual void ProcessRequest(System.Web.HttpContext context)
        {
            HttpRequest Request = context.Request;
            HttpResponse Response = context.Response;

            Response.ContentEncoding = Encoding.UTF8;

            string httpMethod = Request.HttpMethod;

            IEnumerable<RestHandlerRoute> Routes;
            if (this._Routes.ContainsKey(httpMethod))
            {
                Routes = this._Routes[httpMethod];
            }
            else
            {
                Response.StatusCode = (int)_DefaultStatusCode;
                Response.End();
                return;
            }

            string path = Request.Url.AbsolutePath;
            if (_PathPrefix != null && path.StartsWith(_PathPrefix))
            {
                path = path.Remove(0, _PathPrefix.Length);
            }

            // Strip query string
            int qIndex = path.IndexOf('?');
            if (qIndex > -1)
            {
                path = path.Remove(qIndex);
            }

            Match match;
            ArrayList pathParams;
            int i, len;

            foreach (RestHandlerRoute Route in Routes)
            {
                match = Route.Pattern.Match(path);
                if (!match.Success) continue;

                pathParams = new ArrayList();
                for (i = 1, len = match.Groups.Count; i<len; i++)
                {
                    if (match.Groups[i].Captures.Count == 0) continue;
                    pathParams.Add(HttpUtility.UrlDecode(match.Groups[i].Captures[0].Value));
                }

                if (httpMethod == @"GET")
                {
                    Route.Target.Get(Request, Response, (string[])pathParams.ToArray(typeof(string)));
                    Response.End();
                    break; // FINISHED
                }
                else if (httpMethod == @"POST")
                {
                    Route.Target.Post(Request, Response, (string[])pathParams.ToArray(typeof(string)));
                    Response.End();
                    break; // FINISHED
                }
                else if (httpMethod == @"PUT")
                {
                    Route.Target.Put(Request, Response, (string[])pathParams.ToArray(typeof(string)));
                    Response.End();
                    break; // FINISHED
                }
                else if (httpMethod == @"DELETE")
                {
                    Route.Target.Delete(Request, Response, (string[])pathParams.ToArray(typeof(string)));
                    Response.End();
                    break; // FINISHED
                }
                else if (httpMethod == @"HEAD")
                {
                    Route.Target.Head(Request, Response, (string[])pathParams.ToArray(typeof(string)));
                    Response.End();
                    break; // FINISHED
                }
                else if (httpMethod == @"OPTIONS")
                {
                    Route.Target.Options(Request, Response, (string[])pathParams.ToArray(typeof(string)));
                    Response.End();
                    break; // FINISHED
                }
                else if (httpMethod == @"PATCH")
                {
                    Route.Target.Patch(Request, Response, (string[])pathParams.ToArray(typeof(string)));
                    Response.End();
                    break; // FINISHED
                }
            }

            Response.StatusCode = (int)_DefaultStatusCode;
            Response.End();
        }

        #region Variables
        private Dictionary<string, List<RestHandlerRoute>> _Routes = new Dictionary<string, List<RestHandlerRoute>>();
        private HttpStatusCode _DefaultStatusCode = HttpStatusCode.NotImplemented;
        private string _PathPrefix = @"/";
        #endregion

        #region Properties
        public Dictionary<string, List<RestHandlerRoute>> Routes
        {
            get { return _Routes; }
            set { _Routes = value; }
        }
        public HttpStatusCode DefaultStatusCode
        {
            get { return _DefaultStatusCode; }
            set { _DefaultStatusCode = value; }
        }
        public string PathPrefix
        {
            get { return _PathPrefix; }
            set
            {
                if (value == null || value.Length == 0)
                {
                    value = @"/";
                }
                _PathPrefix = value;
            }
        }

        #endregion

        #region AddRoute

        public void AddRoute(string HttpMethod, RestHandlerRoute Route)
        {
            List<RestHandlerRoute> routes;
            if (_Routes.ContainsKey(HttpMethod.ToUpperInvariant()))
            {
                routes = _Routes[HttpMethod.ToUpperInvariant()];
            }
            else
            {
                routes = new List<RestHandlerRoute>();
                _Routes[HttpMethod.ToUpperInvariant()] = routes;
            }

            routes.Add(Route);
        }

        public void Get(string route, IRestHandlerTarget target)
        {
            AddRoute(@"GET", new RestHandlerRoute(route, target));
        }

        public void Get(Regex routePattern, IRestHandlerTarget target)
        {
            AddRoute(@"GET", new RestHandlerRoute(routePattern, target));
        }

        public void Post(string route, IRestHandlerTarget target)
        {
            AddRoute(@"POST", new RestHandlerRoute(route, target));
        }

        public void Post(Regex routePattern, IRestHandlerTarget target)
        {
            AddRoute(@"POST", new RestHandlerRoute(routePattern, target));
        }

        public void Put(string route, IRestHandlerTarget target)
        {
            AddRoute(@"PUT", new RestHandlerRoute(route, target));
        }

        public void Put(Regex routePattern, IRestHandlerTarget target)
        {
            AddRoute(@"PUT", new RestHandlerRoute(routePattern, target));
        }

        public void Delete(string route, IRestHandlerTarget target)
        {
            AddRoute(@"DELETE", new RestHandlerRoute(route, target));
        }

        public void Delete(Regex routePattern, IRestHandlerTarget target)
        {
            AddRoute(@"DELETE", new RestHandlerRoute(routePattern, target));
        }

        public void Head(string route, IRestHandlerTarget target)
        {
            AddRoute(@"HEAD", new RestHandlerRoute(route, target));
        }

        public void Head(Regex routePattern, IRestHandlerTarget target)
        {
            AddRoute(@"HEAD", new RestHandlerRoute(routePattern, target));
        }

        public void Options(string route, IRestHandlerTarget target)
        {
            AddRoute(@"OPTIONS", new RestHandlerRoute(route, target));
        }

        public void Options(Regex routePattern, IRestHandlerTarget target)
        {
            AddRoute(@"OPTIONS", new RestHandlerRoute(routePattern, target));
        }

        public void Patch(string route, IRestHandlerTarget target)
        {
            AddRoute(@"PATCH", new RestHandlerRoute(route, target));
        }

        public void Patch(Regex routePattern, IRestHandlerTarget target)
        {
            AddRoute(@"PATCH", new RestHandlerRoute(routePattern, target));
        }

        /// <summary>
        /// Deprecated. here for backwards compatibility
        /// </summary>
        public void AddRoute(string route, IRestHandlerTarget target, bool handleGet, bool handlePost, bool handlePut, bool handleDelete, bool handleHead)
        {
            if (handleGet)
            {
                AddRoute(@"GET", RestHandlerRoute.FromOldRouteFormat(route, target));
            }
            if (handlePost)
            {
                AddRoute(@"POST", RestHandlerRoute.FromOldRouteFormat(route, target));
            }
            if (handlePut)
            {
                AddRoute(@"PUT", RestHandlerRoute.FromOldRouteFormat(route, target));
            }
            if (handleDelete)
            {
                AddRoute(@"DELETE", RestHandlerRoute.FromOldRouteFormat(route, target));
            }
            if (handleHead)
            {
                AddRoute(@"HEAD", RestHandlerRoute.FromOldRouteFormat(route, target));
            }
        }

        #endregion
    }
}

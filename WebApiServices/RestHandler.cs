using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net;

namespace dg.Utilities.WebApiServices
{
    public abstract class RestHandler : IHttpHandler
    {
        public RestHandler(RestHandlerRoute[] Routes)
        {
            this.Routes = Routes;
        }
        public RestHandler()
        {
        }

        public bool IsReusable { get { return true; } }

        private static char[] PATH_SEPARATOR = new char[] { '/' };
        private static string[] EMPTY_STRING_ARRAY = new string[] { };

        public void ProcessRequest(System.Web.HttpContext context)
        {
            HttpRequest Request = context.Request;
            HttpResponse Response = context.Response;

            Response.ContentEncoding = Encoding.UTF8;

            string HttpMethod = Request.HttpMethod;

            string path = Request.Url.AbsolutePath;
            if (_PathPrefix != null && path.StartsWith(_PathPrefix))
            {
                path = path.Remove(0, _PathPrefix.Length);
            }
            string[] pathParts = path.Split(PATH_SEPARATOR, StringSplitOptions.None);

            RestHandlerRoute[] Routes = this._Routes;
            int idx, count, partLen;
            ArrayList pathParams;
            string part;
            bool[] Wildcards;
            bool[] PushParams;
            Regex[] RouteRegexes;
            bool SlashEnding = path.Length > 0 && path[path.Length - 1] == '/';

            foreach (RestHandlerRoute Route in Routes)
            {
                count = Route.Route.Length;
                if ((count != pathParts.Length && (!_AutomaticallyHandleEndingSlash || SlashEnding == Route.SlashEnding)) ||
                    (_AutomaticallyHandleEndingSlash && SlashEnding != Route.SlashEnding &&
                     ((SlashEnding && count != pathParts.Length - 1) || (!SlashEnding && count != pathParts.Length + 1))
                    )) continue;
                pathParams = null;
                Wildcards = Route.Wildcards;
                PushParams = Route.PushParams;
                RouteRegexes = Route.RouteRegexes;
                for (idx = 0; idx < count; idx++)
                {
                    part = Route.Route[idx];
                    partLen = part.Length;
                    if (_AutomaticallyHandleEndingSlash && idx == count - 1 && !SlashEnding && Route.SlashEnding)
                    {
                        continue; // OK
                    }
                    if (Wildcards[idx])
                    {
                        if (PushParams[idx])
                        {
                            if (pathParams == null) pathParams = new ArrayList();
                            pathParams.Add(pathParts[idx]);
                        }
                        continue; // OK
                    }
                    else if (RouteRegexes[idx] != null)
                    {
                        if (RouteRegexes[idx].IsMatch(pathParts[idx]))
                        {
                            if (PushParams[idx])
                            {
                                if (pathParams == null) pathParams = new ArrayList();
                                pathParams.Add(pathParts[idx]);
                            }
                            continue; // OK
                        }
                    }
                    else if (part == pathParts[idx])
                    {
                        if (PushParams[idx])
                        {
                            if (pathParams == null) pathParams = new ArrayList();
                            pathParams.Add(pathParts[idx]);
                        }
                        continue; // OK
                    }
                    break; // FAIL
                }
                if (idx == count)
                {
                    if (HttpMethod == @"GET")
                    {
                        if (Route.HandleGet)
                        {
                            Route.Target.Get(Request, Response, pathParams == null ? EMPTY_STRING_ARRAY : (string[])pathParams.ToArray(typeof(string)));
                            Response.End();
                            break; // FINISHED
                        }
                        else
                        {
                            continue; // Next handler
                        }
                    }
                    else if (HttpMethod == @"POST")
                    {
                        if (Route.HandlePost)
                        {
                            Route.Target.Post(Request, Response, pathParams == null ? EMPTY_STRING_ARRAY : (string[])pathParams.ToArray(typeof(string)));
                            Response.End();
                            break; // FINISHED
                        }
                        else
                        {
                            continue; // Next handler
                        }
                    }
                    else if (HttpMethod == @"PUT")
                    {
                        if (Route.HandlePut)
                        {
                            Route.Target.Put(Request, Response, pathParams == null ? EMPTY_STRING_ARRAY : (string[])pathParams.ToArray(typeof(string)));
                            Response.End();
                            break; // FINISHED
                        }
                        else
                        {
                            continue; // Next handler
                        }
                    }
                    else if (HttpMethod == @"DELETE")
                    {
                        if (Route.HandleDelete)
                        {
                            Route.Target.Delete(Request, Response, pathParams == null ? EMPTY_STRING_ARRAY : (string[])pathParams.ToArray(typeof(string)));
                            Response.End();
                            break; // FINISHED
                        }
                        else
                        {
                            continue; // Next handler
                        }
                    }
                    else if (HttpMethod == @"HEAD")
                    {
                        if (Route.HandleHead)
                        {
                            Route.Target.Head(Request, Response, pathParams == null ? EMPTY_STRING_ARRAY : (string[])pathParams.ToArray(typeof(string)));
                            Response.End();
                            break; // FINISHED
                        }
                        else
                        {
                            continue; // Next handler
                        }
                    }
                    continue; // Next handler
                }
            }

            Response.StatusCode = (int)_DefaultStatusCode;
            Response.End();
        }

        #region Variables
        private RestHandlerRoute[] _Routes = new RestHandlerRoute[] { };
        private HttpStatusCode _DefaultStatusCode = HttpStatusCode.NotImplemented;
        private string _PathPrefix = @"/";
        private bool _AutomaticallyHandleEndingSlash = false;
        #endregion

        #region Properties
        public RestHandlerRoute[] Routes
        {
            get
            {
                return _Routes;
            }
            set
            {
                _Routes = value ?? new RestHandlerRoute[] { };
            }
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
        public bool AutomaticallyHandleEndingSlash
        {
            get { return _AutomaticallyHandleEndingSlash; }
            set { _AutomaticallyHandleEndingSlash = value; }
        }
        #endregion

        #region AddRoute

        public void AddRoute(RestHandlerRoute Route)
        {
            List<RestHandlerRoute> routes = new List<RestHandlerRoute>(_Routes);
            routes.Add(Route);
            _Routes = routes.ToArray();
        }
        public void AddRoute(string[] Route, IRestHandlerTarget Target, bool HandleGet, bool HandlePost, bool HandlePut, bool HandleDelete, bool HandleHead)
        {
            AddRoute(new RestHandlerRoute(Route, Target, HandleGet, HandlePost, HandlePut, HandleDelete, HandleHead));
        }
        public void AddRoute(string Route, IRestHandlerTarget Target, bool HandleGet, bool HandlePost, bool HandlePut, bool HandleDelete, bool HandleHead)
        {
            AddRoute(new RestHandlerRoute(Route, Target, HandleGet, HandlePost, HandlePut, HandleDelete, HandleHead));
        }

        #endregion
    }
}

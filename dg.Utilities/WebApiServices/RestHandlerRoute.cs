using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace dg.Utilities.WebApiServices
{
    public struct RestHandlerRoute
    {
        public RestHandlerRoute(string[] Route, IRestHandlerTarget Target, bool HandleGet, bool HandlePost, bool HandlePut, bool HandleDelete, bool HandleHead)
        {
            this.Route = Route;
            this.Target = Target;
            this.HandleGet = HandleGet;
            this.HandlePost = HandlePost;
            this.HandlePut = HandlePut;
            this.HandleDelete = HandleDelete;
            this.HandleHead = HandleHead;
            this.Wildcards = null;
            this.RouteRegexes = null;
            this.PushParams = null;
            this.SlashEnding = false;
            CompileRoute();
        }
        public RestHandlerRoute(string Route, IRestHandlerTarget Target, bool HandleGet, bool HandlePost, bool HandlePut, bool HandleDelete, bool HandleHead)
        {
            this.Route = Route.Split(new char[] { '/' }, StringSplitOptions.None);
            this.Target = Target;
            this.HandleGet = HandleGet;
            this.HandlePost = HandlePost;
            this.HandlePut = HandlePut;
            this.HandleDelete = HandleDelete;
            this.HandleHead = HandleHead;
            this.Wildcards = null;
            this.RouteRegexes = null;
            this.PushParams = null;
            this.SlashEnding = false;
            CompileRoute();
        }

        public readonly string[] Route;
        public IRestHandlerTarget Target;
        public bool HandleGet;
        public bool HandlePost;
        public bool HandlePut;
        public bool HandleDelete;
        public bool HandleHead;

        internal bool[] Wildcards;
        internal Regex[] RouteRegexes;
        internal bool SlashEnding;
        internal bool[] PushParams;

        internal void CompileRoute()
        {
            ArrayList wildcards = new ArrayList();
            ArrayList regexes = new ArrayList();
            ArrayList pushParams = new ArrayList();

            string part;
            for (int j = 0; j < Route.Length; j++)
            {
                part = Route[j];
                if (part.StartsWith(@"#"))
                {
                    pushParams.Add(true);
                    part = part.Length > 1 ? part.Substring(1) : @"";
                    Route[j] = part;
                }
                else
                {
                    pushParams.Add(false);
                }

                if (part.StartsWith(@"match:"))
                {
                    wildcards.Add(false);
                    regexes.Add(new Regex(part.Substring(6), RegexOptions.Compiled | RegexOptions.ECMAScript));
                }
                else
                {
                    regexes.Add(null);
                    
                    if (part == @"*")
                    {
                        wildcards.Add(true);
                    }
                    else
                    {
                        wildcards.Add(false);
                        for (int s = part.IndexOf('\\'); s > -1; s = part.IndexOf('\\', s + 1))
                        {
                            part = part.Remove(s, 1);
                        }
                    }
                }
            }
            this.Wildcards = (bool[])wildcards.ToArray(typeof(bool));
            this.RouteRegexes = (Regex[])regexes.ToArray(typeof(Regex));
            this.PushParams = (bool[])pushParams.ToArray(typeof(bool));
            SlashEnding = Route.Length > 0 && Route[Route.Length - 1] == @"/";
        }
    }
}

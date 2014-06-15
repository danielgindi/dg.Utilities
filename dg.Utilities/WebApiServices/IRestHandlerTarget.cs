using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace dg.Utilities.WebApiServices
{
    public interface IRestHandlerTarget
    {
        void Get(HttpRequest Request, HttpResponse Response, params string[] PathParams);
        void Post(HttpRequest Request, HttpResponse Response, params string[] PathParams);
        void Put(HttpRequest Request, HttpResponse Response, params string[] PathParams);
        void Delete(HttpRequest Request, HttpResponse Response, params string[] PathParams);
        void Head(HttpRequest Request, HttpResponse Response, params string[] PathParams);
        void Options(HttpRequest Request, HttpResponse Response, params string[] PathParams);
        void Patch(HttpRequest Request, HttpResponse Response, params string[] PathParams);
    }
}

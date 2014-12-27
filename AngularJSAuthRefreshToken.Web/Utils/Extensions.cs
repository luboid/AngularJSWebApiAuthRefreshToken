using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Web;
using AngularJSAuthRefreshToken.Web.Models;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Net;
using System.Net.Http;

namespace AngularJSAuthRefreshToken.Web
{
    public static class Utils
    {
        //public static IHttpActionResult Conflict(this ApiController controller, object content)
        //{
        //    return new ResponseMessageResult(
        //        controller.ActionContext.Request.CreateResponse(HttpStatusCode.Conflict, content));
        //}

        public static IHttpActionResult Conflict<T>(this ApiController controller, T content)
        {
            return new ResponseMessageResult(
                controller.ActionContext.Request.CreateResponse<T>(HttpStatusCode.Conflict, content));
        }

        public static Action<IAppBuilder, string, string> Callback(this Dictionary<string, Action<IAppBuilder, string, string>> dic, string provider)
        {
            Action<IAppBuilder, string, string> callback = null;
            dic.TryGetValue(provider, out callback);
            return callback;
        }

        public static long GetUserId(this IPrincipal principal)
        {
            return Convert.ToInt64(principal.Identity.GetUserId());
        }

        public static string Base64ToUrlToken(string value)
        {
            var array = Convert.FromBase64String(value);
            return HttpServerUtility.UrlTokenEncode(array);
        }

        public static string UrlTokenToBase64(string value)
        {
            var array = HttpServerUtility.UrlTokenDecode(value);
            return Convert.ToBase64String(array);
        }
    }
}
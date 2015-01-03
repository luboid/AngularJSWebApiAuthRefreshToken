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
using AngularJSAuthRefreshToken.AspNetIdentity;

namespace AngularJSAuthRefreshToken.Web
{
    public static class Utils
    {
        public static void ThrowConflict(this ApiController controller, string message)
        {
            var content = new HttpError(message);
            var response = controller.ActionContext.Request
                .CreateResponse<HttpError>(HttpStatusCode.Conflict, content);

            throw new HttpResponseException(response);
        }

        public static IHttpActionResult NotFound(this ApiController controller, string message)
        {
            var content = new HttpError(message);
            return new ResponseMessageResult(
                controller.ActionContext.Request.CreateResponse<HttpError>(HttpStatusCode.NotFound, content));
        }

        public static IHttpActionResult Conflict(this ApiController controller, string message)
        {
            var content = new HttpError(message);
            return new ResponseMessageResult(
                controller.ActionContext.Request.CreateResponse<HttpError>(HttpStatusCode.Conflict, content));
        }

        public static IHttpActionResult Conflict<T>(this ApiController controller, T content)
        {
            return new ResponseMessageResult(
                controller.ActionContext.Request.CreateResponse<T>(HttpStatusCode.Conflict, content));
        }

        public static IHttpActionResult Forbidden<T>(this ApiController controller, T content)
        {
            return new ResponseMessageResult(
                controller.ActionContext.Request.CreateResponse<T>(HttpStatusCode.Forbidden, content));
        }

        public static IHttpActionResult Forbidden(this ApiController controller, string message)
        {
            var content = new HttpError(message);
            return new ResponseMessageResult(
                controller.ActionContext.Request.CreateResponse<HttpError>(HttpStatusCode.Forbidden, content));
        }

        public static void ThrowForbidden(this ApiController controller, string message)
        {
            var content = new HttpError(message);
            var response = controller.ActionContext.Request
                .CreateResponse<HttpError>(HttpStatusCode.Forbidden, content);

            throw new HttpResponseException(response);
        }

        public static void ThrowForbiddenIfMe(this ApiController controller, IPrincipal principal, string userId, Func<string> message)
        {
            if (principal.IsMe(userId))
            {
                controller.ThrowForbidden(message());
            }
        }

        public static Action<IAppBuilder, string, string> Callback(this Dictionary<string, Action<IAppBuilder, string, string>> dic, string provider)
        {
            Action<IAppBuilder, string, string> callback = null;
            dic.TryGetValue(provider, out callback);
            return callback;
        }

        public static string GetUserId(this IPrincipal principal)
        {
            return principal.Identity.GetUserId();
        }

        public static bool IsMe(this IPrincipal principal, string id)
        {
            return !string.IsNullOrWhiteSpace(id) && 
                string.Compare(principal.GetUserId(), id, StringComparison.InvariantCultureIgnoreCase) == 0;
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

        public static IdentityUser Sanitize(this IdentityUser user)
        {
            user.PasswordHash = null;
            user.SecurityStamp = null;
            user.PushRegistrationId = null;
            return user;
        }
    }
}
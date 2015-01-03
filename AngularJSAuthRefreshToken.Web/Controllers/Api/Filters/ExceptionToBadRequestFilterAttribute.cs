using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

using LL.MSSQL.Extensions;

namespace AngularJSAuthRefreshToken.Web.Controllers.Api
{
    public class ExceptionToBadRequestFilterAttribute : ExceptionFilterAttribute
    {
        static Dictionary<Type, Func<HttpActionExecutedContext, HttpResponseMessage>> handlers =
            new Dictionary<Type, Func<HttpActionExecutedContext, HttpResponseMessage>>() { 
            {typeof(BrowserContextException), (context)=>{
                return context.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, context.Exception.Message);
            }}
        };

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            Func<HttpActionExecutedContext, HttpResponseMessage> handler;
            if (handlers.TryGetValue(actionExecutedContext.Exception.GetType(), out handler))
            {
                actionExecutedContext.Response = handler(actionExecutedContext);
            }
        }
    }
}
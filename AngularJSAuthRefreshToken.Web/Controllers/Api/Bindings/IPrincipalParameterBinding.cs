using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace AngularJSAuthRefreshToken.Web.Controllers.Api
{
    public class IPrincipalParameterBinding : HttpParameterBinding
    {
        public IPrincipalParameterBinding(HttpParameterDescriptor parameter)
            : base(parameter)
        { }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider,
            HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            SetValue(actionContext, actionContext.RequestContext.Principal);

            return Task.FromResult<object>(null);
        }
    }

    public class IPrincipalAttribute : ParameterBindingAttribute
    {
        public IPrincipalAttribute()
        { }

        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            if (parameter.ParameterType == typeof(IPrincipal))
            {
                return new IPrincipalParameterBinding(parameter);
            }
            return parameter.BindAsError("Wrong parameter type");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using AngularJSAuthRefreshToken.Web.Controllers.Api;

namespace AngularJSAuthRefreshToken.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Services.Add(typeof(System.Web.Http.Validation.ModelValidatorProvider),
                new FluentValidation.WebApi.FluentValidationModelValidatorProvider(
                        NinjectWebCommon.ValidatorFactory));

            FluentValidation.ValidatorOptions.ResourceProviderType = typeof(Resources.FluentValidation); // if you have any related resource file (resx)
            FluentValidation.ValidatorOptions.CascadeMode = FluentValidation.CascadeMode.StopOnFirstFailure; //if you need!
            //FluentValidation.ValidatorOptions.PropertyNameResolver += (type, memberInfo, lambda) => {
            //    return memberInfo.Name;
            //};

            //DataAnnotationsModelValidatorProvider
            //  .AddImplicitRequiredAttributeForValueTypes = false;

            var formatters = config.Formatters;
            if (null != formatters.XmlFormatter)
            {
                formatters.Remove(formatters.XmlFormatter);
            }

            if (null != formatters.JsonFormatter)
            {
                formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            config.Filters.Add(new ValidationFilterAttribute());
            config.Filters.Add(new ExceptionToBadRequestFilterAttribute());
            

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}

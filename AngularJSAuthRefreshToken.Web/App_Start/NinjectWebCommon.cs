[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(AngularJSAuthRefreshToken.Web.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(AngularJSAuthRefreshToken.Web.NinjectWebCommon), "Stop")]

namespace AngularJSAuthRefreshToken.Web
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Modules;
    using FluentValidation;
    using System.Reflection;
    using LL.Repository;
    using AngularJSAuthRefreshToken.Web.AspNetIdentity;
    using AngularJSAuthRefreshToken.Web.Models;
    using Microsoft.AspNet.Identity;
    using AngularJSAuthRefreshToken.Repository;
    using AngularJSAuthRefreshToken.Data.Interfaces;
    using Microsoft.Owin.Security;

    public static class NinjectWebCommon 
    {
        #region NinjectValidatorFactory
        public class NinjectValidatorFactory : IValidatorFactory
        {
            static Type genericValidator = typeof(IValidator<>);

            IKernel Kernel
            {
                get;
                set;
            }

            Type CreateType(params Type[] parameters)
            {
                return genericValidator.MakeGenericType(parameters);
            }

            public NinjectValidatorFactory(IKernel kernel)
            {
                this.Kernel = kernel;
            }

            public IValidator<T> GetValidator<T>()
            {
                return (IValidator<T>)this.GetValidator(typeof(T));
            }

            public IValidator GetValidator(Type type)
            {
                var genericType = CreateType(type);
                return this.Kernel.TryGet(genericType) as IValidator;
            }
        }
        #endregion

        #region IDependencyResolver
        class DependencyResolver : AngularJSAuthRefreshToken.Web.IDependencyResolver
        {
            IKernel kernel;

            public DependencyResolver(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public T GetService<T>(bool throwException = true)
            {
                var instance = kernel.Get<T>();
                if (throwException && null == instance)
                {
                    throw new NotSupportedException(typeof(T).FullName);
                }
                return instance;
            }

            public System.Collections.Generic.IEnumerable<T> GetServices<T>()
            {
                return kernel.GetAll<T>();
            }
        }
        #endregion

        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        public static NinjectValidatorFactory ValidatorFactory
        {
            get
            {
                return new NinjectValidatorFactory(bootstrapper.Kernel);
            }
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel(
                new ValidatorsModule(), 
                new CommonModule());

            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<AngularJSAuthRefreshToken.Web.IDependencyResolver>()
                .ToConstant(new DependencyResolver(kernel)).InSingletonScope();
        }      
    }

    public class ValidatorsModule : NinjectModule
    {
        public override void Load()
        {
            var validators = AssemblyScanner.FindValidatorsInAssembly(Assembly.GetExecutingAssembly());
            validators.ForEach((match) =>
            {
                Bind(match.InterfaceType).To(match.ValidatorType).InRequestScope();
            });
        }
    }

    public class CommonModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDbContext>().To<DbContext>().InRequestScope();
            Bind<IExternalLoginProviders>().To<ExternalLoginProviders>().InTransientScope();

            Bind<ISecureDataFormat<AuthenticationTicket>>().ToMethod((context) => {
                return Startup.OAuthOptions.AccessTokenFormat;
            }).InSingletonScope();


            #region User Authorizations services
            Bind<IUserStore<IdentityUser>>().To<UserStore>().InRequestScope();
            Bind<IRoleStore<IdentityRole, string>>().To<RoleStore>().InRequestScope();
            Bind<IRefreshTokenStore>().To<RefreshTokenStore>().InRequestScope();

            Bind<ApplicationUserManager>().ToSelf().InRequestScope();
            Bind<ApplicationRoleManager>().ToSelf().InRequestScope();
            Bind<ApplicationRefreshTokenManager>().ToSelf().InRequestScope();


            Bind<IUserManagerService>().To<UserManagerService>().InSingletonScope();
            Bind<IRoleManagerService>().To<RoleManagerService>().InSingletonScope();

            Bind<ISignInManagerService>().To<SignInManagerService>().InSingletonScope();
            Bind<IAuthenticationManagerService>().To<AuthenticationManagerService>().InSingletonScope();
            Bind<IRefreshTokenManagerService>().To<RefreshTokenManagerService>().InSingletonScope();
            #endregion User Authorizations services

            //MVC
            //this.BindFilter<BookKeeper.Web.Controllers.Filters.AuthorizeFirmFilter>(FilterScope.Controller, 0)
            //    .WhenControllerHas<BookKeeper.Web.Controllers.Filters.AuthorizeFirmAttribute>();

            //WebApi
            //this.BindHttpFilter<BookKeeper.Web.ApiControlers.Filters.AuthorizeFirmFilter>(System.Web.Http.Filters.FilterScope.Controller)
            //    .WhenControllerHas<BookKeeper.Web.ApiControlers.Filters.AuthorizeFirmAttribute>();

            /*
             https://github.com/ninject/ninject.web.mvc/wiki/MVC3
             https://github.com/ninject/ninject.web.mvc/wiki/Filter-configurations
this.BindFilter<LogFilter>(FilterScope.Controller, 0)
     .WhenControllerHas<LogAttribute>()
     .WithConstructorArgumentFromControllerAttribute<LogAttribute>(
          "logLevel",
          attribute => attribute.LogLevel);
// For property injection WithPropertyValueFromControllerAttribute instead
 
this.BindFilter<LogFilter>(FilterScope.Action, 0)
     .WhenActionMethodHas<LogAttribute>()
     .WithConstructorArgumentFromActionAttribute<LogAttribute>(
          "logLevel",
          attribute => attribute.LogLevel);
// For property injection WithPropertyValueFromActionAttribute instead
 
this.BindFilter<LogFilter>(FilterScope.Action, 0)
     .WhenActionHas<LogAttribute>()
     .WithConstructorArgument((
          "logLevel",
          (context, controllerContext, actionDescriptor) =>
               actionDescriptor.ActionName == "Index" ? Level.Info : Level.Warn);             
             */
        }
    }
}

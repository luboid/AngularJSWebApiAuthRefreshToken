using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using AngularJSAuthRefreshToken.AspNetIdentity;
using System.Web.Mvc;
using System.Net.Configuration;
using System.Net.Mail;
using System.Diagnostics;
using System.Web.Configuration;
using System.Web.Security;
using System.Text;

using ClaimsIdentityFactory = AngularJSAuthRefreshToken.Web.AspNetIdentity.ClaimsIdentityFactory<AngularJSAuthRefreshToken.AspNetIdentity.IdentityUser,string>;

namespace AngularJSAuthRefreshToken.Web.Models
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public interface IAuthenticationManagerService
    {
        IAuthenticationManager Instance { get; }
    }

    public class AuthenticationManagerService : IAuthenticationManagerService
    {
        public IAuthenticationManager Instance
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }
    }

    public interface IUserManagerService
    {
        ApplicationUserManager Instance { get; }
    }

    public class UserManagerService : IUserManagerService
    {
        public ApplicationUserManager Instance
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }
    }

    public interface IPublicUserStore<TUser,TKey>
        where TUser : class, Microsoft.AspNet.Identity.IUser<TKey>
    {
        IUserStore<TUser, TKey> PublicStore { get; }
    }

    public class ApplicationUserManager : UserManager<IdentityUser>, IPublicUserStore<IdentityUser, string>
    {
        public ApplicationUserManager(IUserStore<IdentityUser> store)
            : base(store)
        {
        }

        public IUserStore<IdentityUser, string> PublicStore
        {
            get
            {
                return base.Store;
            }
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context)
        {
            var manager = DependencyResolver.Current.GetService<ApplicationUserManager>();

            // Configure validation logic for usernames
            manager.UserValidator = new AngularJSAuthRefreshToken.Web.AspNetIdentity.UserValidator<IdentityUser,string>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new AngularJSAuthRefreshToken.Web.AspNetIdentity.PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            manager.ClaimsIdentityFactory = new ClaimsIdentityFactory();

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug in here.
            //manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<IdentityUser>
            //{
            //    MessageFormat = "Your security code is: {0}"
            //});
            //manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<IdentityUser>
            //{
            //    Subject = "SecurityCode",
            //    BodyFormat = "Your security code is {0}"
            //});
            
            manager.EmailService = new EmailService();
            //manager.SmsService = new SmsService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<IdentityUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public interface IRoleManagerService
    {
        ApplicationRoleManager Instance { get; }
    }

    public class RoleManagerService : IRoleManagerService
    {
        public ApplicationRoleManager Instance
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            }
        }
    }

    // Configure the RoleManager used in the application. RoleManager is defined in the ASP.NET Identity core assembly
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole,string> roleStore)
            : base(roleStore)
        {
            RoleValidator = new AngularJSAuthRefreshToken.Web.AspNetIdentity.RoleValidator<IdentityRole, string>(this);
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return DependencyResolver.Current.GetService<ApplicationRoleManager>();
        }
    }

    public interface ISignInManagerService
    {
        ApplicationSignInManager Instance { get; }
    }

    public class SignInManagerService : ISignInManagerService
    {
        public ApplicationSignInManager Instance
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationSignInManager>();
            }
        }
    }

    // Configure the application sign-in manager which is used in this application.  
    public class ApplicationSignInManager : SignInManager<IdentityUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager) { }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(IdentityUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    public interface IRefreshTokenManagerService
    {
        ApplicationRefreshTokenManager Instance { get; }
    }

    public class RefreshTokenManagerService : IRefreshTokenManagerService
    {
        public ApplicationRefreshTokenManager Instance
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationRefreshTokenManager>();
            }
        }
    }

    public class ApplicationRefreshTokenManager : IRefreshTokenStore
    {
        static readonly string Purpose = "6C2A33BA-98B4-41E8-9BDF-0362F935E3AC";

        IRefreshTokenStore store;

        public ApplicationRefreshTokenManager(IRefreshTokenStore store)
        {
            this.store = store;
        }

        public Task<RefreshTokenClientApp> FindClientAppByIdAsync(string id)
        {
            return this.store.FindClientAppByIdAsync(id);
        }

        public Task<bool> ContainsClientAppByIdAsync(string id)
        {
            return this.store.ContainsClientAppByIdAsync(id);
        }

        public Task CreateTokenAsync(RefreshToken token)
        {
            return this.store.CreateTokenAsync(token);
        }

        public Task DeleteTokenAsync(string id)
        {
            return this.store.DeleteTokenAsync(id);
        }

        public Task<RefreshToken> FindTokenByIdAsync(string id)
        {
            return this.store.FindTokenByIdAsync(id);
        }

        public Task<bool> IsValidSecret(RefreshTokenClientApp client, string client_secret)
        {
            bool result = false;
            if (client.ApplicationType == RefreshTokenClientAppType.NativeConfidential)
            {
                if ((string.IsNullOrWhiteSpace(client_secret) && !string.IsNullOrWhiteSpace(client.Secret)) ||
                    (!string.IsNullOrWhiteSpace(client_secret) && string.IsNullOrWhiteSpace(client.Secret)))
                {
                    result = false;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(client_secret) && string.IsNullOrWhiteSpace(client.Secret))
                    {
                        result = true;
                    }
                    else
                    {
                        //client.Secret == Helper.GetHash(clientSecret)
                        result = string.Compare(client_secret, client.Secret, StringComparison.InvariantCulture) == 0;
                    }
                }
            }
            else
            {
                result = string.IsNullOrWhiteSpace(client_secret);
            }
            return Task.FromResult(result);
        }

        public string Protect(string refreshToken)
        {
            return HttpServerUtility.UrlTokenEncode(
                MachineKey.Protect(
                Encoding.ASCII.GetBytes(refreshToken), Purpose));
        }

        public string Unprotect(string refreshToken)
        {
            try
            {
                return Encoding.ASCII.GetString(
                    MachineKey.Unprotect(
                    HttpServerUtility.UrlTokenDecode(refreshToken), Purpose));
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (null != store)
            {
                (store as IDisposable).Dispose();
            }
            GC.SuppressFinalize(this);
        }

        public static ApplicationRefreshTokenManager Create(IdentityFactoryOptions<ApplicationRefreshTokenManager> options,
            IOwinContext context)
        {
            return DependencyResolver.Current.GetService<ApplicationRefreshTokenManager>();
        }
    }
}
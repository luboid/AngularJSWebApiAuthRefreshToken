using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using AngularJSAuthRefreshToken.Web.Models;
using Owin;
using System;
using System.Linq;
using AngularJSAuthRefreshToken.AspNetIdentity;
using Microsoft.Owin.Security.OAuth;
using AngularJSAuthRefreshToken.Web.Providers;
using System.Web.Mvc;
using AngularJSAuthRefreshToken.Data.Interfaces;
using System.Collections.Generic;

namespace AngularJSAuthRefreshToken.Web
{
    public partial class Startup
    {
        public static Dictionary<string, Action<IAppBuilder, string, string>> providersRegistrationCallback =
            new Dictionary<string, Action<IAppBuilder, string, string>>(StringComparer.InvariantCultureIgnoreCase) 
                { 
                    {"Google", (appBuilder, clientId, clientSecret) => {
                        appBuilder.UseGoogleAuthentication(clientId, clientSecret);
                    }},
                    {"Facebook", (appBuilder, appId, appSecret) => {
                        appBuilder.UseFacebookAuthentication(appId, appSecret);
                    }},
                    {"MicrosoftAccount", (appBuilder, clientId, clientSecret) => {
                        appBuilder.UseMicrosoftAccountAuthentication(clientId, clientSecret);
                    }},
                    {"Twitter", (appBuilder, consumerKey, consumerSecret) => {
                        appBuilder.UseTwitterAuthentication(consumerKey, consumerSecret);
                    }}
                };

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and role manager to use a single instance per request
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.CreatePerOwinContext<ApplicationRefreshTokenManager>(ApplicationRefreshTokenManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, IdentityUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Configure the application for OAuth based flow
            //PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/token"),
                Provider = new ApplicationOAuthProvider(),
                AuthorizeEndpointPath = new PathString("/api/account/externallogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                RefreshTokenProvider = new ApplicationRefreshTokenProvider()
#if DEBUG
                , AllowInsecureHttp = true
#endif
            };

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            //Register externla public logon providers 
            using (var providers = DependencyResolver.Current.GetService<IExternalLoginProviderRepository>())
            {
                foreach (var provider in providers.All()
                    .Select(p => new { Callback = providersRegistrationCallback.Callback(p.Id), p.ClientId, p.ClientSecret })
                    .Where(p => p.Callback != null))
                {
                    provider.Callback(app, provider.ClientId, provider.ClientSecret);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using AngularJSAuthRefreshToken.Web.Models;
using AngularJSAuthRefreshToken.AspNetIdentity;
using System.Web;
using Microsoft.Owin.Security.Infrastructure;

namespace AngularJSAuthRefreshToken.Web.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        public static readonly string LocalLoginProvider = "Local";

        public ApplicationOAuthProvider()
        { }

        async Task GenerateUserIdentityAsync(BaseValidatingTicketContext<OAuthAuthorizationServerOptions> context, ApplicationUserManager userManager, IdentityUser user)
        {
            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
               OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                CookieAuthenticationDefaults.AuthenticationType);

            var properties = new AuthenticationProperties();
            var refreshTokenClient = context.OwinContext.Get<RefreshTokenClientApp>();

            properties.Dictionary["client_id"] = refreshTokenClient == null ? null : refreshTokenClient.Id;

            var ticket = new AuthenticationTicket(oAuthIdentity, properties);

            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            var user = await userManager.FindByEmailAsync(context.UserName).WithCurrentCulture();
            if (user != null)
            {
                if (!user.EmailConfirmed)
                {
                    context.SetError("invalid_grant", Resources.Common.InvalidGrant);
                    return;
                }
                else
                {
                    if (!(await userManager.CheckPasswordAsync(user, context.Password).WithCurrentCulture()))
                    {
                        user = null;
                    }
                }
            }

            if (user == null)
            {
                context.SetError("invalid_grant", Resources.Common.InvalidGrant);
                return;
            }

            await GenerateUserIdentityAsync(context, userManager, user);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            AdditionalResponseParameters(context.AdditionalResponseParameters, context.Identity, context.Properties);

            return Task.FromResult<object>(null);
        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string client_id = string.Empty;
            string client_secret = string.Empty;


            if (!context.TryGetBasicCredentials(out client_id, out client_secret))
            {
                context.TryGetFormCredentials(out client_id, out client_secret);
            }

            if (context.ClientId == null)
            {
                //Remove the comments from the below line context.SetError, and invalidate context 
                //if you want to force sending clientId/secrects once obtain access tokens. 
                context.Validated();
                //context.SetError("invalid_clientId", "ClientId should be sent.");
                return;
            }

            var refreshTokenManager = context.OwinContext.Get<ApplicationRefreshTokenManager>();

            var client = await refreshTokenManager.FindClientAppByIdAsync(client_id);

            if (client == null)
            {
                context.SetError("invalid_clientId", string.Format("Client {0} is not registered in the system.", client_id));
                return;
            }


            if (!client.Active)
            {
                context.SetError("invalid_clientId", "Client is inactive.");
                return;
            }

            if (!await refreshTokenManager.IsValidSecret(client, client_secret))
            {
                context.SetError("invalid_clientId", "Client secret should be sent.");
                return;
            }


            context.OwinContext.Set(client);

            // GrantResourceOwnerCredentials
            // var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            //if (allowedOrigin == null) allowedOrigin = "*";

            context.OwinContext.Response.Headers.Set("Access-Control-Allow-Origin", client.AllowedOrigin ?? "*");

            context.Validated();
        }

        public override async Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            var userId = context.Ticket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await userManager.FindByIdAsync(userId).WithCurrentCulture();

            await GenerateUserIdentityAsync(context, userManager, user);
        }

        public override async Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            Uri expectedRootUri;

            var refreshTokenManager = context.OwinContext.Get<ApplicationRefreshTokenManager>();
            var refreshTokenClient = await refreshTokenManager.FindClientAppByIdAsync(context.ClientId);
            if (refreshTokenClient != null)
            {
                context.OwinContext.Set(refreshTokenClient);
                if (string.IsNullOrWhiteSpace(refreshTokenClient.AllowedOrigin))
                {
                    expectedRootUri = new Uri(context.Request.Uri, "/");
                }
                else
                {
                    expectedRootUri = new Uri(refreshTokenClient.AllowedOrigin);
                }
                // if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                // http://stackoverflow.com/questions/20693082/setting-the-redirect-uri-in-asp-net-identity
                if (context.RedirectUri.StartsWith(expectedRootUri.AbsoluteUri)) //on the same domain
                {
                    context.Validated();
                }
            }
        }

        public override async Task AuthorizationEndpointResponse(OAuthAuthorizationEndpointResponseContext context)
        {
            var provider = GetTokenProvider(context.Identity);
            if (provider == ApplicationOAuthProvider.LocalLoginProvider)
            {
                var tiket = context.Options.AccessTokenFormat.Unprotect(context.AccessToken);
                var refreshTokenContext = new AuthenticationTokenCreateContext(
                    context.OwinContext,
                    context.Options.RefreshTokenFormat, tiket);

                await context.Options.RefreshTokenProvider.CreateAsync(refreshTokenContext);

                context.AdditionalResponseParameters.Add("refresh_token", refreshTokenContext.Token);
            }

            AdditionalResponseParameters(context.AdditionalResponseParameters, context.Identity, context.Properties);
        }

        public static string GetTokenProvider(ClaimsIdentity identity)
        {
            var providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            return providerKeyClaim.Issuer == "LOCAL AUTHORITY" ? ApplicationOAuthProvider.LocalLoginProvider : providerKeyClaim.Issuer;
        }

        public static void AdditionalResponseParameters(IDictionary<string, object> additionalResponseParameters, ClaimsIdentity identity, AuthenticationProperties properties) 
        {
            foreach (KeyValuePair<string, string> property in properties.Dictionary)
            {
                additionalResponseParameters.Add(property.Key, property.Value ?? string.Empty);
            }

            additionalResponseParameters.Add("provider", GetTokenProvider(identity) ?? string.Empty);
            additionalResponseParameters.Add("userName", identity.FindFirstValue(ClaimTypes.Name) ?? string.Empty);
            additionalResponseParameters.Add("email", identity.FindFirstValue(ClaimTypes.Email) ?? string.Empty);
            
            var roles = string.Join(",", identity.FindAll(ClaimTypes.Role).Select(r => r.Value).OrderBy(r => r).ToArray());
            additionalResponseParameters.Add("roles", roles);
        }
    }
}
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AngularJSAuthRefreshToken.AspNetIdentity;
using AngularJSAuthRefreshToken.Web.Models;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.Identity;
using System.Text;
using System.Web.Security;

namespace AngularJSAuthRefreshToken.Web.Providers
{
    public class ApplicationRefreshTokenProvider : IAuthenticationTokenProvider
    {
        AuthenticationTicket CreateTicket(RefreshToken refreshToken)
        {
            var claimsIdentity = new ClaimsIdentity(
                OAuthDefaults.AuthenticationType,
                ClaimTypes.Name,
                ClaimTypes.Role);

            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, refreshToken.UserId));//UserId

            var props = new AuthenticationProperties()
            {
                IssuedUtc = refreshToken.IssuedUtc,
                ExpiresUtc = refreshToken.ExpiresUtc
            };

            return new AuthenticationTicket(claimsIdentity, props);
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var client = context.OwinContext.Get<RefreshTokenClientApp>();
            if (null != client)
            {
                var refreshTokenManager = context.OwinContext.Get<ApplicationRefreshTokenManager>();

                var nowUtc = Startup.OAuthOptions.SystemClock.UtcNow;
                var userId = context.Ticket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;

                var token = new RefreshToken()
                {
                    ClientId = client.Id,
                    UserId = userId,
                    IssuedUtc = nowUtc,
                    ExpiresUtc = nowUtc.AddMinutes(client.TokenLifeTime)
                };

                await refreshTokenManager.CreateTokenAsync(token);

                var protectedToken = refreshTokenManager.Protect(token.Id);

                context.SetToken(protectedToken);
            }
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var refreshTokenManager = context.OwinContext.Get<ApplicationRefreshTokenManager>();
            var client = context.OwinContext.Get<RefreshTokenClientApp>();
            var unprotectedToken = refreshTokenManager.Unprotect(context.Token);
            if (null != client && unprotectedToken != null)
            {
                var refreshToken = await refreshTokenManager.FindTokenByIdAsync(unprotectedToken);
                if (null != refreshToken && string.Compare(refreshToken.ClientId, client.Id) == 0)
                {
                    var ticket = CreateTicket(refreshToken);
                    context.SetTicket(ticket);

                    await refreshTokenManager.DeleteTokenAsync(unprotectedToken);
                }
            }
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}
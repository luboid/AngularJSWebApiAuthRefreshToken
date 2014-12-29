using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using AngularJSAuthRefreshToken.Web.Models;
using AngularJSAuthRefreshToken.Web.Models.Api;
using AngularJSAuthRefreshToken.Web.AspNetIdentity;
using AngularJSAuthRefreshToken.Web.Providers;
using System.Threading;
using System.Net;
using System.Security.Principal;



namespace AngularJSAuthRefreshToken.Web.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        public AccountController(
            IUserManagerService userManager, 
            IAuthenticationManagerService authenticationManager, 
            IRefreshTokenManagerService refreshTokenManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager.Instance;
            Authentication = authenticationManager.Instance;
            RefreshTokenManager = refreshTokenManager.Instance;
            AccessTokenFormat = accessTokenFormat;
        }

        ApplicationRefreshTokenManager RefreshTokenManager
        {
            get;
            set;
        }

        ApplicationUserManager UserManager
        {
            get;
            set;
        }

        IAuthenticationManager Authentication
        {
            get;
            set;
        }

        ISecureDataFormat<AuthenticationTicket> AccessTokenFormat
        {
            get;
            set;
        }


        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordBindingModel model)
        {

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return Ok(false);
            }

            await SendPasswordEmail(user, model);

            // If we got this far, something failed, redisplay form
            return Ok(true);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordBindingModel model)
        {
            model.UserId = ProtectData.UnprotectFromUrlToken(model.UserId);
            model.Code = Utils.UrlTokenToBase64(model.Code);

            var result = await UserManager.ResetPasswordAsync(model.UserId, model.Code, model.Password);
            if (!result.Succeeded) 
            {
                return ErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/Logout
        [AllowAnonymous]// това трябва да е така защото token може да е с изтекъл срок но ние искаме да унищожим и  refresh token-а
        [Route("Logout")]
        public IHttpActionResult Logout(LogOutBindingModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.RefreshToken))
            {
                model.RefreshToken = RefreshTokenManager.Unprotect(model.RefreshToken);
                if (!string.IsNullOrWhiteSpace(model.RefreshToken))
                {
                    RefreshTokenManager.DeleteTokenAsync(model.RefreshToken);
                }
            }

            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);

            return Ok();
        }

        // GET api/Account/Profile
        [Route("Profile")]//ManageInfo
        public async Task<ProfileViewModel> GetProfile([IPrincipal] IPrincipal principal)
        {
            IdentityUser user = await UserManager.FindByIdAsync(principal.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginProvidersViewModel> logins = (await UserManager.GetLoginsAsync(user.Id))
                .Select(l => new UserLoginProvidersViewModel
                {
                    Provider = l.LoginProvider,
                    Key = l.ProviderKey
                }).OrderBy(l=> l.Provider).ToList();

            if (user.PasswordHash != null)
            {
                logins.Insert(0, new UserLoginProvidersViewModel
                {
                    Provider = ApplicationOAuthProvider.LocalLoginProvider,
                    Key = user.Email,
                });
            }

            return new ProfileViewModel { Logins = logins };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword([IPrincipal] IPrincipal principal, ChangePasswordBindingModel model)
        {
            IdentityResult result = await UserManager.ChangePasswordAsync(
                principal.GetUserId(), 
                model.OldPassword,
                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return ErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword([IPrincipal] IPrincipal principal, SetPasswordBindingModel model)
        {
            var result = await UserManager.AddPasswordAsync(principal.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return ErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin([IPrincipal] IPrincipal principal, AddExternalLoginBindingModel model)
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(principal.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return ErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin([IPrincipal] IPrincipal principal, RemoveLoginBindingModel model)
        {
            IdentityResult result;

            if (model.Provider == ApplicationOAuthProvider.LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(principal.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(principal.GetUserId(),
                    new UserLoginInfo(model.Provider, model.Key));
            }

            if (!result.Succeeded)
            {
                return ErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null, string redirect_uri = null)
        {
            if (error != null)
            {
                return Redirect(redirect_uri + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            IdentityUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;
            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                if (user.EmailConfirmed)
                {
                    ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                       OAuthDefaults.AuthenticationType);
                    ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                        CookieAuthenticationDefaults.AuthenticationType);

                    Authentication.SignIn(new AuthenticationProperties(), oAuthIdentity, cookieIdentity); 
                }
                else
                {
                    return Redirect(redirect_uri + "#error=" + Uri.EscapeDataString(Resources.Common.InvalidGrant));
                }
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public async Task<IEnumerable<ExternalLoginViewModel>> GetExternalLogins(string returnUrl = null, bool generateState = false, string client_id = null)
        {
            const int strengthInBits = 256;
            string state;
            if (!await RefreshTokenManager.ContainsClientByIdAsync(client_id))
            {
                return Enumerable.Empty<ExternalLoginViewModel>();
            }

            var descriptions = Authentication.GetExternalAuthenticationTypes();


            if (generateState)
            {
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            return descriptions.Select(description => new ExternalLoginViewModel
                {
                    Caption = description.Caption,
                    Provider = description.AuthenticationType,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = client_id ?? "self",
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                });
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            var user = new IdentityUser() { UserName = model.Email, Email = model.Email };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return ErrorResult(result);
            }

            if (!user.EmailConfirmed)
            {
                await SendConfirmationEmail(user, model);
            }
            return Ok(!user.EmailConfirmed);
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null || string.Compare(model.LoginProvider, info.Login.LoginProvider) != 0)
            {
                return InternalServerError();
            }

            var emailConfirmation = string.Compare(info.Email, model.Email, StringComparison.InvariantCultureIgnoreCase) != 0;

            var user = new IdentityUser() { UserName = model.Email, Email = model.Email, EmailConfirmed = !emailConfirmation };

            var result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return ErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return ErrorResult(result); 
            }

            if (emailConfirmation)
            {
                await SendConfirmationEmail(user, model);
            }

            return Ok(emailConfirmation);
        }

        // POST api/Account/ConfirmEmail
        [AllowAnonymous]
        [Route("ConfirmEmail")]
        public async Task<IHttpActionResult> ConfirmEmail(ConfirmEmailBindingModel model)
        {
            model.UserId = ProtectData.UnprotectFromUrlToken(model.UserId);
            model.Code = Utils.UrlTokenToBase64(model.Code);

            var result = await UserManager.ConfirmEmailAsync(model.UserId, model.Code);
            if (!result.Succeeded)
            {
                return ErrorResult(result);
            }

            return Ok(true);
        }

        #region Helpers
        private IHttpActionResult ErrorResult(string errorMessage)
        {
            ModelState.AddModelError("", errorMessage);
            return BadRequest(ModelState);
        }

        private IHttpActionResult ErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                if (Email != null)
                {
                    claims.Add(new Claim(ClaimTypes.Email, Email, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name),
                    Email = identity.FindFirstValue(ClaimTypes.Email)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        private class ChallengeResult : IHttpActionResult
        {
            public ChallengeResult(string loginProvider, ApiController controller)
            {
                LoginProvider = loginProvider;
                Request = controller.Request;
            }

            public string LoginProvider { get; set; }
            public HttpRequestMessage Request { get; set; }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                Request.GetOwinContext().Authentication.Challenge(LoginProvider);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                response.RequestMessage = Request;
                return Task.FromResult(response);
            }
        }

        async Task SendConfirmationEmail(IdentityUser user, RegisterBaseBindingModel model)
        {
            var userId = ProtectData.ProtectStringToUrlToken(user.Id);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

            var href = string.Format(model.ApplicationLocation, userId,
                Utils.Base64ToUrlToken(code));

            await UserManager.SendEmailAsync(user.Id,
                string.Format(Resources.Common.EmailConfirmSubject, model.ApplicationName),
                string.Format(Resources.Common.EmailConfirmBody, model.ApplicationName, href));
        }

        async Task SendPasswordEmail(IdentityUser user, ForgotPasswordBindingModel model)
        {
            var userId = ProtectData.ProtectStringToUrlToken(user.Id);
            var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            var href = string.Format(model.ApplicationLocation, userId,
                Utils.Base64ToUrlToken(code));

            await UserManager.SendEmailAsync(user.Id,
                string.Format(Resources.Common.EmailPasswordSubject, model.ApplicationName),
                string.Format(Resources.Common.EmailPasswordBody, model.ApplicationName, href));
        }

        #endregion
    }
}

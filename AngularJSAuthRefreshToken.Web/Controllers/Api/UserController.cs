using AngularJSAuthRefreshToken.AspNetIdentity;
using AngularJSAuthRefreshToken.Data.Browser;
using AngularJSAuthRefreshToken.Web.Models;
using AngularJSAuthRefreshToken.Web.Models.Api;
using AngularJSAuthRefreshToken.Web.Providers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthRefreshToken.Web.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        ApplicationUserManager userManager;
        IUserStore<IdentityUser, string> userStore;

        public UserController(IUserManagerService userManager, IRoleManagerService roleManager)
        {
            this.userManager = userManager.Instance;
            this.userStore = this.userManager.PublicStore;
        }

        [Route]
        public async Task<IHttpActionResult> GetBrowser([FromUri] BrowserContext browserContext)
        {
            var browser = userStore as IUserBrowser;
            return Ok(await browser.Browser(browserContext));
        }

        [Route("{id:guid}")]
        public async Task<IHttpActionResult> Get(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (null == user)
            {
                return NotFound();
            }

            var roles = await userManager.GetRolesAsync(user.Id);
            var claims = await userManager.GetClaimsAsync(user.Id);
            var logins = (await userManager.GetLoginsAsync(user.Id))
                .Select(l => l.LoginProvider)
                .ToList();

            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                logins.Insert(0, ApplicationOAuthProvider.LocalLoginProvider);
            }

            return Ok(new
            {
                User = user.Sanitize(),
                Roles = roles,
                Claims = claims,
                Logins = logins
            });
        }

        [Route("{id:guid}/role")]
        public async Task<IHttpActionResult> GetRole([IPrincipal] IPrincipal principal, string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (null == user)
            {
                return NotFound();
            }

            return Ok(await userManager.GetRolesAsync(user.Id));
        }

        [Route("{id:guid}/role")]
        public async Task<IHttpActionResult> PostRole([IPrincipal] IPrincipal principal, string id, RoleBindingModel model)
        {
            this.ThrowForbiddenIfMe(principal, id, () => Resources.Common.CantEditYouSelf);

            var user = await userManager.FindByIdAsync(id);
            if (null == user)
            {
                return this.NotFound(Resources.Common.UnknownUser);
            }

            await userManager.AddToRoleAsync(id, model.Name);

            return Ok();
        }

        [Route("{id:guid}/role/{name}")]
        public async Task<IHttpActionResult> DeleteRole([IPrincipal] IPrincipal principal, string id, string name)
        {
            this.ThrowForbiddenIfMe(principal, id, () => Resources.Common.CantEditYouSelf);

            var user = await userManager.FindByIdAsync(id);
            if (null == user)
            {
                return this.NotFound(Resources.Common.UnknownUser);
            }

            await userManager.RemoveFromRoleAsync(id, name);

            return Ok();
        }

    }
}

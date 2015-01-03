using AngularJSAuthRefreshToken.AspNetIdentity;
using AngularJSAuthRefreshToken.Web.Models;
using AngularJSAuthRefreshToken.Web.Models.Api;
using AngularJSAuthRefreshToken.Web.Providers;
using FluentValidation;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using AngularJSAuthRefreshToken.Web.Validators;

namespace AngularJSAuthRefreshToken.Web.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("api/role")]
    public class RoleController : ApiController
    {
        ApplicationRoleManager roleManager;

        public RoleController(IRoleManagerService roleManager)
        {
            this.roleManager = roleManager.Instance;
        }

        [Route]
        public IHttpActionResult Get()
        {
            return Ok(this.roleManager.Roles.ToList());
        }

        [Route("{id:guid}", Name="GetRole")]
        public async Task<IHttpActionResult> Get(string id)
        {
            var role = await this.roleManager.FindByIdAsync(id);
            if (null == role)
            {
                return NotFound();
            }

            return Ok(role);
        }

        [Route]
        public async Task<IHttpActionResult> Post(IdentityRole model)
        {
            var role = await this.roleManager.FindByNameAsync(model.Name);
            if (null != role)
            {
                return this.Conflict(Resources.Role.AlreadyExists);
            }

            await this.roleManager.CreateAsync(model);

            return Created(Url.Link("GetRole", new { model.Id }), model);
        }

        [Route("{id:guid}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            var role = await this.roleManager.FindByIdAsync(id);
            if (null == role)
            {
                return NotFound();
            }

            await this.roleManager.DeleteAsync(role);

            return Ok();
        }

        #region Validators
        public class IdentityRoleValidator : AbstractValidator<IdentityRole>
        {
            public IdentityRoleValidator()
            {
                RuleFor(m => m.Name).NotEmpty();
                RuleFor(m => m.Id).Guid();
            }
        }
        #endregion
    }
}

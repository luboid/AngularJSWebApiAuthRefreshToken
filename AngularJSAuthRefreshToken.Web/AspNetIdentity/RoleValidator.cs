using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthRefreshToken.Web.AspNetIdentity
{
    /// <summary>
    ///     Validates roles before they are saved
    /// </summary>
    /// <typeparam name="TRole"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class RoleValidator<TRole, TKey> : IIdentityValidator<TRole>
        where TRole : class, IRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private RoleManager<TRole, TKey> Manager
        {
            get;
            set;
        }
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="manager"></param>
        public RoleValidator(RoleManager<TRole, TKey> manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            this.Manager = manager;
        }
        /// <summary>
        ///     Validates a role before saving
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> ValidateAsync(TRole item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            List<string> list = new List<string>();
            await this.ValidateRoleName(item, list);
            IdentityResult result;
            if (list.Count > 0)
            {
                result = IdentityResult.Failed(list.ToArray());
            }
            else
            {
                result = IdentityResult.Success;
            }
            return result;
        }
        private async Task ValidateRoleName(TRole role, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.AspNetIdentity.PropertyTooShort, new object[]
				{
					"Name"
				}));
            }
            else
            {
                TRole tRole = await this.Manager.FindByNameAsync(role.Name);
                if (tRole != null && !EqualityComparer<TKey>.Default.Equals(tRole.Id, role.Id))
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.AspNetIdentity.DuplicateName, new object[]
					{
						role.Name
					}));
                }
            }
        }
    }
}
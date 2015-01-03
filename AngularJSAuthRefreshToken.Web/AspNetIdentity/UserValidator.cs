using AngularJSAuthRefreshToken.Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthRefreshToken.Web.AspNetIdentity
{
    /// <summary>
    ///     Validates users before they are saved
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class UserValidator<TUser, TKey> : IIdentityValidator<TUser>
        where TUser : class, IUser<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     Only allow [A-Za-z0-9@_] in UserNames
        /// </summary>
        public bool AllowOnlyAlphanumericUserNames
        {
            get;
            set;
        }
        /// <summary>
        ///     If set, enforces that emails are non empty, valid, and unique
        /// </summary>
        public bool RequireUniqueEmail
        {
            get;
            set;
        }
        private UserManager<TUser, TKey> Manager
        {
            get;
            set;
        }

        private IUserEmailStore<TUser, TKey> Store
        {
            get;
            set;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="manager"></param>
        public UserValidator(UserManager<TUser, TKey> manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            this.AllowOnlyAlphanumericUserNames = true;
            this.Manager = manager;
            this.Store = (manager as IPublicUserStore<TUser, TKey>).PublicStore as IUserEmailStore<TUser, TKey>;
        }
        /// <summary>
        ///     Validates a user before saving
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> ValidateAsync(TUser item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            List<string> list = new List<string>();
            await this.ValidateUserName(item, list);
            if (this.RequireUniqueEmail)
            {
                await this.ValidateEmail(item, list);
            }
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
        private async Task ValidateUserName(TUser user, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.AspNetIdentity.PropertyTooShort, new object[]
				{
					"Name"
				}));
            }
            else
            {
                if (this.AllowOnlyAlphanumericUserNames && !Regex.IsMatch(user.UserName, "^[A-Za-z0-9@_\\.]+$"))
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.AspNetIdentity.InvalidUserName, new object[]
					{
						user.UserName
					}));
                }
                else
                {
                    TUser tUser = await this.Manager.FindByNameAsync(user.UserName);
                    if (tUser != null && !EqualityComparer<TKey>.Default.Equals(tUser.Id, user.Id))
                    {
                        errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.AspNetIdentity.DuplicateName, new object[]
						{
							user.UserName
						}));
                    }
                }
            }
        }
        private async Task ValidateEmail(TUser user, List<string> errors)
        {
            string text = await this.Store.GetEmailAsync(user).WithCurrentCulture<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.AspNetIdentity.PropertyTooShort, new object[]
				{
					"Email"
				}));
            }
            else
            {
                try
                {
                    new MailAddress(text);
                }
                catch (FormatException)
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.AspNetIdentity.InvalidEmail, new object[]
					{
						text
					}));
                    return;
                }
                TUser tUser = await this.Manager.FindByEmailAsync(text);
                if (tUser != null && !EqualityComparer<TKey>.Default.Equals(tUser.Id, user.Id))
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.AspNetIdentity.DuplicateEmail, new object[]
					{
						text
					}));
                }
            }
        }
    }
}
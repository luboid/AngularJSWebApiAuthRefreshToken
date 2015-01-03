using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Web.AspNetIdentity
{
    /// <summary>
    ///     Used to validate some basic password policy like length and number of non alphanumerics
    /// </summary>
    public class PasswordValidator : IIdentityValidator<string>
    {
        /// <summary>
        ///     Minimum required length
        /// </summary>
        public int RequiredLength
        {
            get;
            set;
        }
        /// <summary>
        ///     Require a non letter or digit character
        /// </summary>
        public bool RequireNonLetterOrDigit
        {
            get;
            set;
        }
        /// <summary>
        ///     Require a lower case letter ('a' - 'z')
        /// </summary>
        public bool RequireLowercase
        {
            get;
            set;
        }
        /// <summary>
        ///     Require an upper case letter ('A' - 'Z')
        /// </summary>
        public bool RequireUppercase
        {
            get;
            set;
        }
        /// <summary>
        ///     Require a digit ('0' - '9')
        /// </summary>
        public bool RequireDigit
        {
            get;
            set;
        }
        /// <summary>
        ///     Ensures that the string is of the required length and meets the configured requirements
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual Task<IdentityResult> ValidateAsync(string item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            List<string> list = new List<string>();
            if (string.IsNullOrWhiteSpace(item) || item.Length < this.RequiredLength)
            {
                list.Add(string.Format(CultureInfo.CurrentCulture, Resources.AspNetIdentity.PasswordTooShort, new object[]
				{
					this.RequiredLength
				}));
            }
            if (this.RequireNonLetterOrDigit && item.All(new Func<char, bool>(this.IsLetterOrDigit)))
            {
                list.Add(Resources.AspNetIdentity.PasswordRequireNonLetterOrDigit);
            }
            if (this.RequireDigit && item.All((char c) => !this.IsDigit(c)))
            {
                list.Add(Resources.AspNetIdentity.PasswordRequireDigit);
            }
            if (this.RequireLowercase && item.All((char c) => !this.IsLower(c)))
            {
                list.Add(Resources.AspNetIdentity.PasswordRequireLower);
            }
            if (this.RequireUppercase && item.All((char c) => !this.IsUpper(c)))
            {
                list.Add(Resources.AspNetIdentity.PasswordRequireUpper);
            }
            if (list.Count == 0)
            {
                return Task.FromResult<IdentityResult>(IdentityResult.Success);
            }
            return Task.FromResult<IdentityResult>(IdentityResult.Failed(new string[]
			{
				string.Join(" ", list)
			}));
        }
        /// <summary>
        ///     Returns true if the character is a digit between '0' and '9'
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public virtual bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
        /// <summary>
        ///     Returns true if the character is between 'a' and 'z'
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public virtual bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }
        /// <summary>
        ///     Returns true if the character is between 'A' and 'Z'
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public virtual bool IsUpper(char c)
        {
            return c >= 'A' && c <= 'Z';
        }
        /// <summary>
        ///     Returns true if the character is upper, lower, or a digit
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public virtual bool IsLetterOrDigit(char c)
        {
            return this.IsUpper(c) || this.IsLower(c) || this.IsDigit(c);
        }
    }
}
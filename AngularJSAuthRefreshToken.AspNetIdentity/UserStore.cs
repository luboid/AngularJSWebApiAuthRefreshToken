using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

using Dapper;
using LL.Repository;
using AngularJSAuthRefreshToken.AspNetIdentity.Properties;
using AngularJSAuthRefreshToken.Data.Browser;

namespace AngularJSAuthRefreshToken.AspNetIdentity
{
    public class UserStore : BaseRepository, 
        IUserLoginStore<IdentityUser, string>,
        IUserClaimStore<IdentityUser, string>,
        IUserRoleStore<IdentityUser, string>,
        IUserPasswordStore<IdentityUser, string>,
        IUserSecurityStampStore<IdentityUser, string>,
        IQueryableUserStore<IdentityUser, string>, 
        IUserEmailStore<IdentityUser, string>,
        IUserPhoneNumberStore<IdentityUser, string>,
        IUserTwoFactorStore<IdentityUser, string>,
        IUserLockoutStore<IdentityUser, string>, 
        IUserStore<IdentityUser, string>,
        IUserStore<IdentityUser>,
        IUserBrowser
    {
        public UserStore(IDbContext dbContext) : base(dbContext) { }

        public IQueryable<IdentityUser> Users
        {
            get
            {
                ThrowIfDisposed();
                return _dbContext.GetUsers();
            }
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<DateTimeOffset>(user.LockoutEndDateUtc.HasValue ?
                new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) : 
                default(DateTimeOffset));
        }

        public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset lockoutEnd)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.LockoutEndDateUtc =
                ((lockoutEnd == DateTimeOffset.MinValue) ? null : new DateTime?(lockoutEnd.UtcDateTime));

            return Task.FromResult<int>(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount++;
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount = 0;
            return Task.FromResult<int>(0);
        }

        public Task<int> GetAccessFailedCountAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.LockoutEnabled = enabled;

            return Task.FromResult<int>(0);
        }

        public virtual Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return _dbContext.GetClaimsAsync(user);
        }

        public virtual Task AddClaimAsync(IdentityUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            return _dbContext.AddClaimAsync(user, claim);
        }

        public virtual Task RemoveClaimAsync(IdentityUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            return _dbContext.RemoveClaimAsync(user, claim);
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.EmailConfirmed = confirmed;

            return Task.FromResult<int>(0);
        }

        public Task SetEmailAsync(IdentityUser user, string email)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.Email = email;
            return Task.FromResult<int>(0);
        }

        public Task<string> GetEmailAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.Email);
        }

        public Task<IdentityUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            return _dbContext.FindByEmailAsync(email);
        }

        public virtual Task<IdentityUser> FindByIdAsync(string userId)
        {
            ThrowIfDisposed();
            return _dbContext.GetUserByIdAsync(userId);
        }

        public virtual Task<IdentityUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            return _dbContext.FindByNameAsync(userName);
        }

        public virtual async Task CreateAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await _dbContext.InsertOrUpdateAsync(user);
        }

        public virtual async Task DeleteAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await _dbContext.DeleteAsync(user);
        }

        public virtual async Task UpdateAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await _dbContext.InsertOrUpdateAsync(user);
        }

        public virtual Task<IdentityUser> FindAsync(UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            return _dbContext.FindAsync(login);
        }

        public virtual Task AddLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            return _dbContext.AddLoginAsync(user, login);
        }

        public virtual Task RemoveLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            return _dbContext.RemoveLoginAsync(user, login);
        }

        public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return _dbContext.GetLoginsAsync(user);
        }

        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.PasswordHash = passwordHash;

            return Task.FromResult<int>(0);
        }

        public Task<string> GetPasswordHashAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(IdentityUser user)
        {
            return Task.FromResult<bool>(user.PasswordHash != null);
        }

        public Task SetPhoneNumberAsync(IdentityUser user, string phoneNumber)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.PhoneNumber = phoneNumber;

            return Task.FromResult<int>(0);
        }

        public Task<string> GetPhoneNumberAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.PhoneNumberConfirmed = confirmed;

            return Task.FromResult<int>(0);
        }

        public virtual async Task AddToRoleAsync(IdentityUser user, string name)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await _dbContext.AddToRoleAsync(user, name);
        }

        public virtual async Task RemoveFromRoleAsync(IdentityUser user, string name)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await _dbContext.RemoveFromRoleAsync(user, name);
        }

        public virtual Task<IList<string>> GetRolesAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return _dbContext.GetRolesAsync(user);
        }

        public virtual Task<bool> IsInRoleAsync(IdentityUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(Resources.ValueCannotBeNullOrEmpty, "roleName");
            }

            return _dbContext.IsInRoleAsync(user, roleName);
        }

        public Task SetSecurityStampAsync(IdentityUser user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.SecurityStamp = stamp;

            return Task.FromResult<int>(0);
        }

        public Task<string> GetSecurityStampAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.SecurityStamp);
        }

        public Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.TwoFactorEnabled = enabled;

            return Task.FromResult<int>(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.TwoFactorEnabled);
        }

        public async Task<PagedResult<DTO.IdentityUser>> Browser(BrowserContext context)
        {
            ThrowIfDisposed();
            return await _dbContext.BrowseUsers(context);
        }
    }
}

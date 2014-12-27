using Microsoft.AspNet.Identity;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Dapper;
using LL.Repository;

namespace AngularJSAuthRefreshToken.Web.AspNetIdentity
{
    public class RoleStore : BaseRepository, 
        IRoleStore<IdentityRole, string>, 
        IQueryableRoleStore<IdentityRole, string>
	{
        public RoleStore(IDbContext dbContext) : 
            base(dbContext) { }

        public IQueryable<IdentityRole> Roles
        {
            get
            {
                ThrowIfDisposed();
                return _dbContext.GetRoles();
            }
        }

        public Task<IdentityRole> FindByIdAsync(string roleId)
        {
            ThrowIfDisposed();
            return _dbContext.FindRoleByIdAsync(roleId);
        }

        public Task<IdentityRole> FindByNameAsync(string roleName)
        {
            ThrowIfDisposed();
            return _dbContext.FindRoleByNameAsync(roleName);
        }

        public virtual async Task CreateAsync(IdentityRole role)
        {
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await _dbContext.InsertOrUpdateAsync(role);
        }

        public virtual async Task DeleteAsync(IdentityRole role)
        {
            ThrowIfDisposed();
            if (role != null)
            {
                await _dbContext.DeleteAsync(role);
            }
        }

        public virtual async Task UpdateAsync(IdentityRole role)
        {
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await _dbContext.InsertOrUpdateAsync(role);
        }
	}
}

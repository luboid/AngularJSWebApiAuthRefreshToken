using LL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.AspNetIdentity
{
    public class RefreshTokenStore : BaseRepository, IRefreshTokenStore
	{
        public RefreshTokenStore(IDbContext dbContext) : 
            base(dbContext) { }

        public async Task<RefreshTokenClientApp> FindClientAppByIdAsync(string id)
        {
            ThrowIfDisposed();
            return await _dbContext.FindAppByIdAsync(id);
        }

        public async Task<bool> ContainsClientAppByIdAsync(string id)
        {
            ThrowIfDisposed();
            return await _dbContext.ContainsClientByIdAsync(id);
        }

        public Task CreateTokenAsync(RefreshToken token)
        {
            ThrowIfDisposed();
            return _dbContext.CreateTokenAsync(token);
        }

        public Task DeleteTokenAsync(string id)
        {
            ThrowIfDisposed();
            return _dbContext.DeleteTokenAsync(id);
        }

        public Task<RefreshToken> FindTokenByIdAsync(string id)
        {
            ThrowIfDisposed();
            return _dbContext.FindTokenByIdAsync(id);
        }
    }
}

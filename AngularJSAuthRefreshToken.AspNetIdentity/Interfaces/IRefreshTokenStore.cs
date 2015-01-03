using System;
using System.Threading.Tasks;
namespace AngularJSAuthRefreshToken.AspNetIdentity
{
    public interface IRefreshTokenStore : IDisposable
    {
        Task<RefreshTokenClientApp> FindClientAppByIdAsync(string id);
        Task<bool> ContainsClientAppByIdAsync(string id);
        Task CreateTokenAsync(RefreshToken token);
        Task DeleteTokenAsync(string id);
        Task<RefreshToken> FindTokenByIdAsync(string id);
    }
}

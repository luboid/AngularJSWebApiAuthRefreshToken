using System;
using System.Threading.Tasks;
namespace AngularJSAuthRefreshToken.Web.AspNetIdentity
{
    public interface IRefreshTokenStore : IDisposable
    {
        Task<RefreshTokenClient> FindClientByIdAsync(string id);
        Task<bool> ContainsClientByIdAsync(string id);
        Task CreateTokenAsync(RefreshToken token);
        Task DeleteTokenAsync(string id);
        Task<RefreshToken> FindTokenByIdAsync(string id);
    }
}

using AngularJSAuthRefreshToken.Data.Browser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.AspNetIdentity
{
    public interface IUserBrowser
    {
        Task<PagedResult<DTO.IdentityUser>> Browser(BrowserContext browserContext);
    }
}

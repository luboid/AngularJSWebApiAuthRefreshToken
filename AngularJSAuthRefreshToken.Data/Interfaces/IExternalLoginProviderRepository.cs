using AngularJSAuthRefreshToken.Data.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Data.Interfaces
{
    public interface IExternalLoginProviderRepository : IBaseRepository
    {
        IList<ExternalLoginProvider> All();
    }
}

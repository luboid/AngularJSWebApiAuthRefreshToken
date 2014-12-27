using AngularJSAuthRefreshToken.Data.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Data.Interfaces
{
    public interface IExternalLoginProviders : IBaseRepository
    {
        IList<ExternalLoginProvider> All();
    }
}

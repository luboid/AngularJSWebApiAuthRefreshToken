using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthRefreshToken.Web
{
    public interface IDependencyResolver
    {
        T GetService<T>(bool throwException = true);
        IEnumerable<T> GetServices<T>();
    }
}
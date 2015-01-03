using AngularJSAuthRefreshToken.Data.Interfaces;
using AngularJSAuthRefreshToken.Data.Poco;
using LL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;

namespace AngularJSAuthRefreshToken.Repository
{
    public class ExternalLoginProviderRepository : LL.Repository.BaseRepository, IExternalLoginProviderRepository
    {
        public ExternalLoginProviderRepository(IDbContext dbContext)
            : base(dbContext)
        { }

        public IList<ExternalLoginProvider> All()
        {
            ThrowIfDisposed();
            using (var context = _dbContext.Open())
                return context.Connection.Query<ExternalLoginProvider>(
                    sql: "SELECT * FROM ExternalLoginProviders",
                    transaction: context.Transaction) as IList<ExternalLoginProvider>;
        }
    }
}

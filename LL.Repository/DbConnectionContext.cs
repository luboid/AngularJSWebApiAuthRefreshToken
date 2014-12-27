using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.Repository
{
    public sealed class DbConnectionContext : IDbConnectionContext
    {
        DbContext dbContext;
        bool transaction;
        bool commit;

        internal DbConnectionContext(DbContext dbContext, bool transaction)
        {
            this.dbContext = dbContext;
            this.transaction = transaction;
        }

        void RaiseObjectDisposedException()
        {
            if (null == dbContext)
                throw new ObjectDisposedException(typeof(DbConnectionContext).Name);
        }

        public System.Data.IDbConnection Connection
        {
            get 
            {
                RaiseObjectDisposedException();
                return dbContext.Connection;
            }
        }

        public System.Data.IDbTransaction Transaction
        {
            get 
            {
                RaiseObjectDisposedException();
                return dbContext.Transaction;
            }
        }

        public void Commit()
        {
            RaiseObjectDisposedException();

            if (!transaction)
                throw new ApplicationException("Context is not bound to transaction.");

            dbContext.Commit();
            commit = true;
        }

        public void Dispose()
        {
            if (null != dbContext)
            {
                if (!commit)
                {
                    if (transaction)
                        dbContext.Rollback();
                    else
                        dbContext.CloseConnection();
                }
                dbContext = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}

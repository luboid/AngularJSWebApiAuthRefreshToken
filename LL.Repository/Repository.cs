using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LL.Repository
{
    public class BaseRepository : IDisposable
    {
        bool _disposed;
        protected IDbContext _dbContext;

        public bool DisposeContext
        {
            get;
            set;
        }

        public BaseRepository(IDbContext dbContext)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException("dbContext");
            }
            _dbContext = dbContext;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (DisposeContext && disposing && _dbContext != null)
            {
                _dbContext.Dispose();
            }
            _disposed = true;
            _dbContext = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

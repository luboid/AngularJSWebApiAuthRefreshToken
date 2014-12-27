using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LL.Repository
{
    public class DbContext : IDbContext
    {
        DbConnection sqlConnection;
        int sqlConnectionCount = 0;

        DbTransaction sqlTransaction;
        int sqlTransactionCount = 0;

        public DbContext()
            : this("DefaultConnection")
        { }

        protected DbProviderFactory DbProviderFactory { get; private set; }
        protected ConnectionStringSettings ConnectionStringSettings { get; private set; }

        public DbContext(string connectionStringName)
        {
            ConnectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (null == ConnectionStringSettings)
                throw new ArgumentException("Invalid connection string name.", "connectionStringName");

            DbProviderFactory = DbProviderFactories.GetFactory(ConnectionStringSettings.ProviderName);
        }

        protected virtual void OpenConnection()
        {
            RaiseObjectDisposedException();

            if (null == sqlConnection)
            {
                sqlConnection = DbProviderFactory.CreateConnection();
                try
                {
                    sqlConnection.ConnectionString = ConnectionStringSettings.ConnectionString;
                    sqlConnection.Open();
                }
                catch
                {
                    sqlConnection.Dispose();
                    sqlConnection = null;
                    throw;
                }
            }
            ++sqlConnectionCount;
        }

        internal virtual void CloseConnection(bool disposing = false)
        {
            RaiseObjectDisposedException();

            if (0 == sqlConnectionCount && !disposing)
                throw new ApplicationException("Context already is closed.");

            if (1 == sqlConnectionCount || disposing)
            {
                if (null != sqlTransaction)
                {
                    sqlTransaction.Rollback();
                    sqlTransaction.Dispose();
                    sqlTransaction = null;
                    sqlTransactionCount = 0;
                }
                if (null != sqlConnection)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                    sqlConnection = null;
                }
                if (disposing)
                {
                    sqlConnectionCount = 0;
                }
            }
            --sqlConnectionCount;
        }

        protected virtual void BeginTransaction()
        {
            OpenConnection();
            if (null == sqlTransaction)
            {
                sqlTransaction = sqlConnection.BeginTransaction();
            }
            else
            {
                sqlTransaction.Savepoit("savePoint_" + sqlTransactionCount);
            }
            ++sqlTransactionCount;
        }

        internal virtual void Commit()
        {
            RaiseNoActiveTransaction();

            --sqlTransactionCount;

            if (0 == sqlTransactionCount)
            {
                sqlTransaction.Commit();
                sqlTransaction.Dispose();
                sqlTransaction = null;
            }

            CloseConnection();
        }

        internal virtual void Rollback()
        {
            RaiseNoActiveTransaction();

            --sqlTransactionCount;

            if (0 == sqlTransactionCount)
            {
                sqlTransaction.Rollback();
                sqlTransaction.Dispose();
                sqlTransaction = null;
            }
            else
            {
                sqlTransaction.Rollback("savePoint_" + sqlTransactionCount);
            }

            CloseConnection();
        }

        IDbConnectionContext IDbContext.Open()
        {
            OpenConnection();
            return new DbConnectionContext(this, false);
        }

        IDbConnectionContext IDbContext.BeginTransaction()
        {
            BeginTransaction();
            return new DbConnectionContext(this, true);
        }

        internal System.Data.IDbConnection Connection
        {
            get
            {
                RaiseNoActiveConnection();
                return sqlConnection;
            }
        }

        internal System.Data.IDbTransaction Transaction
        {
            get
            {
                RaiseNoActiveConnection();
                return sqlTransaction;
            }
        }

        protected void RaiseNoActiveTransaction()
        {
            RaiseObjectDisposedException();
            if (0 == sqlTransactionCount)
                throw new ApplicationException("No active transaction is present.");
        }

        protected void RaiseNoActiveConnection()
        {
            RaiseObjectDisposedException();
            if (null == sqlConnection)
                throw new ApplicationException("No active connection is present.");
        }

        protected void RaiseObjectDisposedException()
        {
            if (sqlConnectionCount < 0)
                throw new ObjectDisposedException(typeof(DbContext).Name);
        }

        public void Dispose()
        {
            CloseConnection(true);
            GC.SuppressFinalize(this);
        }
    }
}
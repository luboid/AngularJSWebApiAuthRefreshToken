using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LL.Repository
{
    static class DbTransactionSavepoint
    {
        static ConcurrentDictionary<Type, Tuple<MethodInfo, MethodInfo>> transactionAdditionlMethods =
            new ConcurrentDictionary<Type, Tuple<MethodInfo, MethodInfo>>();

        public static void Savepoit(this DbTransaction transaction, string savepointName)
        {
            Tuple<MethodInfo, MethodInfo> methods = null; Type t = transaction.GetType();
            if (!transactionAdditionlMethods.TryGetValue(t, out methods))
            {
                lock (transactionAdditionlMethods)
                {
                    MethodInfo rollback = null, save = t.GetMethod("Save", new[] { typeof(string) });
                    if (null != save)
                    {
                        rollback = t.GetMethod("Rollback", new[] { typeof(string) });
                        methods = new Tuple<MethodInfo, MethodInfo>(save, rollback);
                    }
                    transactionAdditionlMethods[t] = methods;
                }
            }

            if (null != methods)
            {
                methods.Item1.Invoke(transaction, new[] { savepointName });
            }
        }

        public static void Rollback(this DbTransaction transaction, string savepointName)
        {
            Tuple<MethodInfo, MethodInfo> methods; Type t = transaction.GetType();
            if (transactionAdditionlMethods.TryGetValue(t, out methods) && methods != null)
            {
                methods.Item2.Invoke(transaction, new[] { savepointName });
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.MSSQL.Extensions
{
    public class BrowserContextException : ApplicationException
    {
        public BrowserContextException(string message, params object[] values)
            : base(string.Format(message, values))
        { }
    }
}

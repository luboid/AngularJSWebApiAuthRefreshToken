using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.MSSQL.Extensions
{
    public static class MSSQL
    {
        public static bool IsKeyVioaltion(this SqlException ex)
        {
            return ex.Number == 2627 || ex.Number == 2601;
        }

        public static bool IsForeingKeyViolation(this SqlException ex)
        {
            return ex.Number == 547;
        }
    }
}

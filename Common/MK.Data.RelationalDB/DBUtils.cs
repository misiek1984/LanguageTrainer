using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

using Microsoft.Practices.EnterpriseLibrary.Data;

namespace MK.Data.RelationalDB
{
    public static class DBUtils
    {
        public static long GetLastIdentity(Database db, DbTransaction transaction)
        {
            DbCommand cmd = db.GetSqlStringCommand("SELECT @@IDENTITY");
            var res = db.ExecuteScalar(cmd, transaction);

            return (long)(decimal)res;
        }
    }
}

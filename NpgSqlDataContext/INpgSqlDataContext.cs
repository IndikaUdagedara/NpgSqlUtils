using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgSqlUtils
{
    public interface INpgSqlDataContext: IDisposable
    {
        DataTable Query(
            string query, 
            IDictionary<string, object> scalarParams,
            IDictionary<string, KeyValuePair<NpgsqlTypes.NpgsqlDbType, object[]>> tableParams);

        /// <summary>
        /// fluent interface
        /// </summary>
        INpgSqlDataContext Map<T>(string name) where T : new();
    }
}

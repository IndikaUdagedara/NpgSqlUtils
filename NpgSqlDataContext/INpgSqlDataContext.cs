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
            IDictionary<string, NpgTableParameter> tableParams);

        int NonQuery(
            string query,
            IDictionary<string, object> scalarParams,
            IDictionary<string, NpgTableParameter> tableParams);

        /// <summary>
        /// fluent interface
        /// </summary>
        INpgSqlDataContext MapComposite<T>(string name) where T : new();
    }
}

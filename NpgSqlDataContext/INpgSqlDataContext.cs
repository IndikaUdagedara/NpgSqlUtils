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
            IEnumerable<INpgSqlParameter> parameters);

        int Execute(
            string query,
            IEnumerable<INpgSqlParameter> parameters);

        /// <summary>
        /// fluent interface
        /// </summary>
        INpgSqlDataContext MapComposite<T>(string name) where T : new();
    }
}

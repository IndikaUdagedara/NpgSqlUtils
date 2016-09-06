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
        DataTable Query(string query, params INpgSqlParameter[] parameters);
        int Execute(string query, params INpgSqlParameter[] parameters);

        /// <summary>
        /// fluent interface
        /// </summary>
        INpgSqlDataContext MapComposite<T>(string name) where T : new();
    }
}

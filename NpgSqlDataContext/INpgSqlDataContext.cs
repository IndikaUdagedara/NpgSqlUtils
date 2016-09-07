using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace NpgSqlUtils
{
    public interface INpgSqlDataContext: IDisposable
    {
        INpgsqlNameTranslator Translator { get; }
        DataTable Query(string query, params NpgsqlParameter[] parameters);
        int Execute(string query, params NpgsqlParameter[] parameters);

        /// <summary>
        /// fluent interface
        /// </summary>
        INpgSqlDataContext MapComposite<T>(string name) where T : new();
    }
}

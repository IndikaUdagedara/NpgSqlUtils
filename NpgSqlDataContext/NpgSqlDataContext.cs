using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgSqlUtils
{
    public class NpgSqlDataContext : INpgSqlDataContext
    {
        private NpgsqlConnection _connection;
        public NpgSqlDataContext(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
        }

        public DataTable Query(
            string query, 
            IDictionary<string, object> scalarParams = null, 
            IDictionary<string, NpgTableParameter> tableParams = null)
        {
            DataTable result;
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = query;
                if (scalarParams != null)
                {
                    foreach (var scalarParam in scalarParams)
                        cmd.Parameters.AddWithValue(scalarParam.Key, scalarParam.Value);
                }

                if (tableParams != null)
                {
                    foreach (var tableParam in tableParams)
                        cmd.Parameters.Add(tableParam.Key,
                            NpgsqlTypes.NpgsqlDbType.Array | tableParam.Value.Type).Value = tableParam.Value.Rows;
                }
                
                using (var reader = cmd.ExecuteReader())
                {
                    result = new DataTable();
                    result.Load(reader);
                }
            }

            return result;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public INpgSqlDataContext MapComposite<T>(string name) where T : new()
        {
            _connection.MapComposite<T>(name);
            return this;
        }
    }
}

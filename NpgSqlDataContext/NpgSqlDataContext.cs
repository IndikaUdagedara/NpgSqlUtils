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
            IDictionary<string, KeyValuePair<NpgsqlTypes.NpgsqlDbType, object[]>> tableParams = null)
        {
            DataTable result;
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = query;
                if (scalarParams != null)
                {
                    foreach (var scalarParam in scalarParams)
                    {
                        cmd.Parameters.AddWithValue(scalarParam.Key, scalarParam.Value);
                    }
                }

                if (tableParams != null)
                {
                    foreach (var tableParam in tableParams)
                    {
                        cmd.Parameters.Add(tableParam.Key, NpgsqlTypes.NpgsqlDbType.Array | tableParam.Value.Key)
                            .Value = tableParam.Value.Value;
                    }
                }
                
                using (var reader = cmd.ExecuteReader())
                {
                    result = new DataTable();
                    for(int i = 0; i < reader.FieldCount; i++)
                    {
                        result.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                    }

                    while (reader.Read())
                    {
                        result.Rows.Add(Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetValue(i)).ToArray());
                    }
                }
            }

            return result;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public INpgSqlDataContext Map<T>(string name) where T : new()
        {
            _connection.MapComposite<T>(name);
            return this;
        }
    }
}

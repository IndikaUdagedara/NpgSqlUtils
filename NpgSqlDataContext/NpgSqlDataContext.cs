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
            IEnumerable<INpgSqlParameter> parameters = null)
        {
            DataTable result;
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = query;
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        cmd.Parameters.Add(p.Name, p.Type).Value = p.Value;
                    }
                }
                
                using (var reader = cmd.ExecuteReader())
                {
                    result = new DataTable();
                    result.Load(reader);
                }
            }

            return result;
        }


        public int Execute(
            string query,
            IEnumerable<INpgSqlParameter> parameters = null)
        {
            int result = -1;
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = query;
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        cmd.Parameters.Add(p.Name, p.Type).Value = p.Value;
                    }
                }

                result = cmd.ExecuteNonQuery();
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

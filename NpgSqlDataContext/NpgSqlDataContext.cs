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

        public INpgsqlNameTranslator Translator { get; }

        public NpgSqlDataContext(string connectionString, INpgsqlNameTranslator translator = null)
        {
            if (translator == null)
                translator = new CaseSensitiveTranslator();

            Translator = translator;

            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
        }

        public DataTable Query(string query, params NpgsqlParameter[] parameters)
        {
            DataTable result;
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = query;
                cmd.Parameters.AddRange(parameters);

                using (var reader = cmd.ExecuteReader())
                {
                    result = new DataTable();
                    result.Load(reader);
                }
            }

            return result;
        }


        public int Execute(string query, params NpgsqlParameter[] parameters)
        {
            int result = -1;
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = query;
                cmd.Parameters.AddRange(parameters);
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
            _connection.MapComposite<T>(name, Translator);
            return this;
        }
    }
}

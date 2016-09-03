using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data;
using NpgSqlUtils;

namespace ConsoleApplication1
{
    class id_age
    {
        public int id { get; set; }
        public int age { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            using (var dc = new NpgSqlDataContext("Host=localhost;Username=postgres;Password=admin;Database=TEST"))
            {
                var r1 = dc.Query(@"select * from customers");
                PrintTable(r1);

                var r2 = dc.Query(@"select * from customers where age=@ageval",
                    new Dictionary<string, object> { { "ageval", 25 } });
                PrintTable(r2);

                // using table valued parameters
                // PG doesn't have tvp - they are mimicked by arrays of regular or Composite types
                var r3 = dc.Query(@"select c.* from customers c inner join unnest(@ageval_tvp) tvp on c.age = tvp",
                    null,
                    new Dictionary<string, KeyValuePair<NpgsqlTypes.NpgsqlDbType, object[]>> {
                        { "ageval_tvp",
                            new KeyValuePair<NpgsqlTypes.NpgsqlDbType, object[]>(
                                NpgsqlTypes.NpgsqlDbType.Integer,
                                new object[] { 25, 31})
                        }});
                PrintTable(r3);

                // table value parameter of composite type
                // -- CREATE TYPE id_age AS (id integer, age integer)
                dc.Map<id_age>("id_age");
                var r4 = dc.Query(@"SELECT c.* FROM customers c inner join UNNEST(@x_id_age) x on c.age = x.age and c.id = x.id",
                    null,
                    new Dictionary<string, KeyValuePair<NpgsqlTypes.NpgsqlDbType, object[]>> {
                        { "x_id_age",
                            new KeyValuePair<NpgsqlTypes.NpgsqlDbType, object[]>(
                                NpgsqlTypes.NpgsqlDbType.Composite,
                                new object[] {
                                    new id_age() { id = 1, age = 21 },
                                    new id_age() { id = 3, age = 31 }
                                })
                        }});
                PrintTable(r4);

            }
            Console.ReadKey();
        }

        static void PrintTable(DataTable t)
        {
            for (var i = 0; i < t.Columns.Count; i++)
                Console.Write("{0,10}", t.Columns[i].ColumnName);
            Console.WriteLine();

            for (var i = 0; i < t.Rows.Count; i++)
            {
                for (var j = 0; j < t.Rows[i].ItemArray.Length; j++)
                {
                    Console.Write("{0,10}", t.Rows[i].ItemArray[j].ToString());
                }
                Console.WriteLine();
            }
            Console.WriteLine("\n");
        }
    }
}

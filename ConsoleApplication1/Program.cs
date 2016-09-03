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
                // Postgres doesn't have tvp - they are mimicked by arrays of regular or Composite types
                var r3 = dc.Query(@"select c.* from customers c inner join unnest(@ageval_tvp) tvp on c.age = tvp",
                    null,
                    new Dictionary<string, NpgTableParameter> {
                        { "ageval_tvp",
                            new NpgTableParameter() {
                                Type = NpgsqlTypes.NpgsqlDbType.Integer,
                                Rows = new object[] { 25, 31 }
                            }
                        }
                    });
                PrintTable(r3);

                // tvp of composite type
                dc.MapComposite<id_age>("id_age");
                var r4 = dc.Query(@"SELECT c.* FROM customers c inner join UNNEST(@x_id_age) x on c.age = x.age and c.id = x.id",
                    null,
                    new Dictionary<string, NpgTableParameter> {
                        { "x_id_age",
                            new NpgTableParameter() {
                                Type = NpgsqlTypes.NpgsqlDbType.Composite,
                                Rows = new object[] {
                                    new id_age() { id = 1, age = 21 },
                                    new id_age() { id = 3, age = 31 }
                                }
                            }
                        }
                    });
                PrintTable(r4);
            }
            Console.ReadKey();
        }

        static void PrintTable(DataTable t)
        {
            foreach (var c in t.Columns.Cast<DataColumn>().Select(r => r.ColumnName))
                Console.Write("{0,10}", c);
            Console.WriteLine();

            foreach (var r in t.Rows.Cast<DataRow>())
            {
                foreach (var c in r.ItemArray)
                    Console.Write("{0,10}", c.ToString());
                Console.WriteLine();
            }
            Console.WriteLine("\n");
        }
    }
}

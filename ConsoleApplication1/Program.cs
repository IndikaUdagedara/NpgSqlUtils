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
    class age_name
    {
        public int age { get; set; }
        public string name { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var dc = new NpgSqlDataContext("Host=localhost;Username=postgres;Password=admin;Database=TEST"))
            {
                var r1 = dc.Query(@"SELECT * FROM customers");
                PrintTable(r1);

                var r2 = dc.Query(@"SELECT * FROM customers WHERE age=@ageval",
                    new Dictionary<string, object> { { "ageval", 25 } });
                PrintTable(r2);

                // using table valued parameters
                // Postgres doesn't have tvp - they are mimicked by arrays of regular or Composite types
                var r3 = dc.Query(@"SELECT c.* 
                                    FROM customers c 
                                    INNER JOIN UNNEST(@ageval_tvp) tvp ON 
                                        c.age = tvp",
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
                dc.MapComposite<age_name>("age_name");
                var r4 = dc.Query(@"SELECT c.* 
                                    FROM customers c 
                                    INNER JOIN UNNEST(@x_age_name) x ON 
                                        c.age = x.age AND 
                                        c.name = x.name",
                    null,
                    new Dictionary<string, NpgTableParameter> {
                        { "x_age_name",
                            new NpgTableParameter() {
                                Type = NpgsqlTypes.NpgsqlDbType.Composite,
                                Rows = new object[] {
                                    new age_name() { name = "Phil", age = 43 },
                                    new age_name() { name = "Barry", age = 39 }
                                }
                            }
                        }
                    });
                PrintTable(r4);

                var r5 = dc.NonQuery(@"INSERT INTO customers (age, name) values (@age, @name)",
                    new Dictionary<string, object> {
                        { "age", 39 },
                        { "name", "Sam" },
                    }, null);
                Console.WriteLine("Inserted {0} rows", r5);

                dc.MapComposite<age_name>("age_name");
                var r6 = dc.NonQuery(@"INSERT INTO customers (age, name) 
                                       SELECT age, name from UNNEST(@x_age_name)",
                    null,
                    new Dictionary<string, NpgTableParameter> {
                        { "x_age_name",
                            new NpgTableParameter() {
                                Type = NpgsqlTypes.NpgsqlDbType.Composite,
                                Rows = new object[] {
                                    new age_name() { name = "Phil", age = 43 },
                                    new age_name() { name = "Barry", age = 39 }
                                }
                            }
                        }
                    });
                Console.WriteLine("Inserted {0} rows", r6);
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

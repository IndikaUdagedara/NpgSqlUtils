using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using NpgSqlUtils;

namespace ConsoleApplication1
{
    class customer
    {
        public long customer_id { get; set; }
        public int age { get; set; }
        public string name { get; set; }
    }

    class order
    {
        public int order_id { get;  set;}
        public int customer_id { get; set; }
        public string item { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var dc = new NpgSqlDataContext("Host=localhost;Username=postgres;Password=admin;Database=TEST"))
            {
                var r0 = dc.Query(@"SELECT * FROM orders");
                PrintTable(r0);

                var r1 = dc.Query(@"SELECT * FROM orders where customer_id=@customer_id", 
                    new NpgScalarParameter("customer_id", NpgsqlDbType.Integer, 23));
                PrintTable(r1);

                // using table valued parameters
                // Postgres doesn't have tvp - they are mimicked by arrays of regular or Composite types
                var r3 = dc.Query(@"
SELECT c.* 
FROM customers c 
INNER JOIN UNNEST(@ageval_tvp) tvp ON 
    c.age = tvp",
                        new NpgTableParameter(
                            "ageval_tvp",
                            NpgsqlDbType.Integer,
                            new object[] { 25, 31, 39 })
                    );
                PrintTable(r3);

                // tvp of composite type
                dc.MapComposite<customer>("customers");
                var r4 = dc.Query(@"
SELECT c.* 
FROM customers c 
INNER JOIN UNNEST(@x_customer) x ON 
    c.age = x.age AND 
    c.name = x.name",
                        new NpgTableParameter(
                            "x_customer",
                            NpgsqlDbType.Composite,
                            new object[] {
                                new customer() { name = "Phil", age = 43 },
                                new customer() { name = "Barry", age = 39 }
                            }
                        )
                    );
                PrintTable(r4);

                dc.MapComposite<order>("orders");
                var r6 = dc.Query(@"
INSERT INTO orders (customer_id, item) 
SELECT customer_id, item from UNNEST(@x_orders) returning order_id",
                        new NpgTableParameter(
                            "x_orders",
                            NpgsqlDbType.Composite,
                            new object[] {
                                new order() { customer_id = 22, item = "cc" },
                                new order() { customer_id = 23, item = "dd" }
                            })
                        );
                PrintTable(r6);

                var r7 = dc.Execute(@"
WITH customer as (insert into customers(name, age) values ('Ghan', 55) returning customer_id)
INSERT INTO orders(customer_id, item)
SELECT c.customer_id, x.item FROM customer c CROSS JOIN UNNEST(@x_orders) x
",
                    new NpgTableParameter(
                        "x_orders",
                        NpgsqlDbType.Composite,
                        new object[] {
                            new order() { item = "gg" },
                            new order() { item = "hh" }
                        }
                    )
                );
                Console.WriteLine("Inserted {0} rows", r7);
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

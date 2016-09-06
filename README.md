# NpgSqlUtils

## Some utils for [NpgSql](http://www.npgsql.org/doc/index.html)

- Simple wrapper to run inline SQL without setting up connections, commands, parameters etc. and use scalar and table-valued parameters (Postgres doesn't have table-valued parameters. They are mimicked with arrays of regular/composite types). 
- `NpgSqlDataContext` is a wrapper for `NpgSqlConnection` and handles connection open and dispose. 
- `NpgSqlDataContext.Query()` wraps `IDbCommand.ExecuteQuery` and does command creation based on params, executes and returns a result table.
- `NpgSqlDataContext.Execute()` is its counterpart for `IDbCommand.ExecuteNonQuery` (for `INSERT, UPDATE, DELETE`).

Basic usage is
```csharp
using (var dc = new NpgSqlDataContext("Host=localhost;Username=postgres;Password=admin;Database=TEST"))
{
	DataTable result = dc.Query(@"SELECT ....");
	int rowsAffected = dc.Execute(@"INSERT/UPDATE/DELETE ....");
}
```




##Examples

Assume we have `customers` and `orders` tables
```sql
CREATE TABLE public.customers
(
  customer_id bigint NOT NULL DEFAULT nextval('customers_id_seq'::regclass),
  name character varying(50),
  age integer,
  CONSTRAINT customers_pkey PRIMARY KEY (id)
)

CREATE TABLE public.orders
(
  order_id integer NOT NULL DEFAULT nextval('orders_order_id_seq'::regclass),
  customer_id integer,
  item character varying,
  CONSTRAINT orders_pkey PRIMARY KEY (order_id),
  CONSTRAINT orders_customer_id_fkey FOREIGN KEY (customer_id)
      REFERENCES public.customers (customer_id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
```

and they have corresponding models in C# code
```csharp
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
```

- Query with scalar parameter
```csharp
var r= dc.Query(@"SELECT * FROM orders where customer_id=@customer_id", 
                    new NpgScalarParameter("customer_id", NpgsqlDbType.Integer, 23));
```


- Query with table parameter (`NpgTableParameter` is a new type introduced here). Parameter is a regular (non-composite) type
```csharp
var r = dc.Query(@"
SELECT c.* 
FROM customers c 
INNER JOIN UNNEST(@ageval_tvp) tvp ON 
    c.age = tvp",
	new NpgTableParameter(
		"ageval_tvp",
		NpgsqlDbType.Integer,
		new object[] { 25, 31, 39 }));
```

- Query with table parameter of composite type (calling `MapComposite()` tells `NpgSql` about the mapping)
```csharp
dc.MapComposite<customer>("customers");
var r = dc.Query(@"
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
	));
```

- Insert with table parameter of composite type (To perform a batch operation)
```csharp
dc.MapComposite<order>("orders");
var r = dc.Query(@"
INSERT INTO orders (customer_id, item) 
SELECT customer_id, item from UNNEST(@x_orders) returning order_id",
	new NpgTableParameter(
		"x_orders",
		NpgsqlDbType.Composite,
		new object[] {
			new order() { customer_id = 22, item = "cc" },
			new order() { customer_id = 23, item = "dd" }
		}));
```

- Insert with CTE and table parameters
```csharp
var r = dc.Execute(@"
WITH customer as (insert into customers(name, age) values ('Kate', 55) returning customer_id)
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
	));
```
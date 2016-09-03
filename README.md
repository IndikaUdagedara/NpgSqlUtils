# NpgSqlUtils

## Some utils for NpgSql

- Simple wrapper to run inline sql without setting up connections, commands, parameters etc. and use scalar and table-valued parameters. 
- Postgres doesn't have tvp - they are mimicked by arrays of regular or Composite types
- See the sample program for usage

**NOT a complete project at all - use with care.**



##Examples

```
CREATE TABLE public.customers
(
  id bigint NOT NULL DEFAULT nextval('customers_id_seq'::regclass),
  name character varying(50),
  age integer,
  CONSTRAINT customers_pkey PRIMARY KEY (id)
);

CREATE TYPE id_age AS (id integer, age integer);
```

This POCO is mapped to a Postgres type
```csharp
class id_age
{
	public int id { get; set; }
	public int age { get; set; }
}
```


`NpgSqlDataContext` handles connection open and dispose. `NpgSqlDataContext.Query()` wraps `IDbCommand.ExecuteQuery` and does command creation based on the params.
```csharp
using (var dc = new NpgSqlDataContext("Host=localhost;Username=postgres;Password=admin;Database=TEST"))
{
	var r1 = dc.Query(@"select * from customers");

	var r2 = dc.Query(@"select * from customers where age=@ageval",
		new Dictionary<string, object> { { "ageval", 25 } });

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
}
```

##TODO
- Implement `IDbCommand.ExecuteNonQuery` counterpart (for `INSERT, UPDATE, DELETE`)
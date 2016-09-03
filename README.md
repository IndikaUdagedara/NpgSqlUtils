# NpgSqlUtils

## Some utils for NpgSql

- Simple wrapper to run inline sql without setting up connections, commands, parameters etc. and use scalar and table-valued parameters. 
- Postgres doesn't have tvp - they are mimicked by arrays of regular or Composite types
- See the sample program for usage

NOT a complete project at all - use with care. You've been warned.



Example:

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

```csharp
public class User
using (var dc = new NpgSqlDataContext("Host=localhost;Username=postgres;Password=admin;Database=TEST"))
{
	var r1 = dc.Query("get_all", @"SELECT * FROM customers");

	var r2 = dc.Query("get_all", @"SELECT * FROM customers WHERE age=@ageval",
		new Dictionary<string, object> { { "ageval", 25 } });

	// using table valued parameters
	// PG doesn't have tvp - they are mimicked by arrays of regular or Composite types
	var r3 = dc.Query("get_all", @"SELECT c.* FROM customers c INNER JOIN UNNEST(@ageval_tvp) tvp ON c.age = tvp",
		null,
		new Dictionary<string, KeyValuePair<NpgsqlTypes.NpgsqlDbType, object[]>> {
			{ "ageval_tvp",
				new KeyValuePair<NpgsqlTypes.NpgsqlDbType, object[]>(
					NpgsqlTypes.NpgsqlDbType.Integer,
					new object[] { 25, 31 })
			}});

	// table value parameter of composite type
	dc.Map<id_age>("id_age");
	var r4 = dc.Query("get_all", @"SELECT c.* FROM customers c INNER JOIN UNNEST(@x_id_age) x ON c.age = x.age AND c.id = x.id",
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
}
```
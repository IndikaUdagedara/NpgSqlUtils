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

**NOT a complete project at all - use with care.**



##Examples

Assume we have a `customers` table and Postgres type called `age_name`
```sql
CREATE TABLE public.customers
(
  id bigint NOT NULL DEFAULT nextval('customers_id_seq'::regclass),
  name character varying(50),
  age integer,
  CONSTRAINT customers_pkey PRIMARY KEY (id)
)

CREATE TYPE age_name AS (age integer, name varchar);
```


- Query with scalar parameter
```csharp
var r = dc.Query(@"SELECT * FROM customers WHERE age=@ageval",
	new List<INpgSqlParameter> { new NpgScalarParameter("ageval", NpgsqlDbType.Integer, 25) });
```


- Query with table parameter (`NpgTableParameter` is a new type introduced here). Parameter is a regular (non-composite) type
```csharp
var r = dc.Query(@"SELECT c.* 
					FROM customers c 
					INNER JOIN UNNEST(@ageval_tvp) tvp ON 
						c.age = tvp",
					new List<INpgSqlParameter> {
						new NpgTableParameter(
							"ageval_tvp", 
							NpgsqlDbType.Integer,
							new object[] { 25, 31, 39 })
					});
```

- Query with table parameter of composite type (Note calling `MapComposite()` tells `NpgSql` about the mapping)
```csharp
class age_name
{
	public int age { get; set; }
	public string name { get; set; }
}
```

```csharp
dc.MapComposite<age_name>("age_name");
var r = dc.Query(@"SELECT c.* 
					FROM customers c 
					INNER JOIN UNNEST(@x_age_name) x ON 
						c.age = x.age AND 
						c.name = x.name",
					new List<INpgSqlParameter> {
						new NpgTableParameter(
							"x_age_name",
							NpgsqlDbType.Composite,
							new object[] {
								new age_name() { name = "Phil", age = 43 },
								new age_name() { name = "Barry", age = 39 }
							}
						)
					});
```

- Insert with table parameter of composite type (To perform a batch operation)
```csharp
dc.MapComposite<age_name>("age_name");
var r = dc.Execute(@"INSERT INTO customers (age, name) 
					   SELECT age, name from UNNEST(@x_age_name)",
						new List<INpgSqlParameter> {
							new NpgTableParameter(
								"x_age_name",
								NpgsqlDbType.Composite,
								new object[] {
										new age_name() { name = "Phil", age = 43 },
										new age_name() { name = "Barry", age = 39 }
								}
							)
						});
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;

namespace NpgSqlUtils
{
    public interface INpgSqlParameter
    {
        string Name { get; }
        NpgsqlDbType Type { get; }
        object Value { get; }
    }

    public class NpgTableParameter : INpgSqlParameter
    {
        public NpgTableParameter(string name, NpgsqlDbType type, object[] value)
        {
            Name = name;
            Type = NpgsqlDbType.Array | type;
            Value = value;
        }

        public string Name { get; }
        public NpgsqlDbType Type { get; }
        public object Value { get; }
    }

    public class NpgScalarParameter : INpgSqlParameter
    {
        public NpgScalarParameter(string name, NpgsqlDbType type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public string Name { get; }
        public NpgsqlDbType Type { get; }
        public object Value { get; }
    }
}

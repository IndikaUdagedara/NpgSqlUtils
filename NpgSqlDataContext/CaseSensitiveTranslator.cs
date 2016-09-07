using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace NpgSqlUtils
{
    public class CaseSensitiveTranslator : INpgsqlNameTranslator
    {
        public string TranslateMemberName(string clrName)
        {
            return clrName;
        }

        public string TranslateTypeName(string clrName)
        {
            return clrName;
        }
    }
}

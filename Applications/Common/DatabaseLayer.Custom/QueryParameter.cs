using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace DatabaseLayer
{
    public class QueryParameter
    {
        public QueryParameter(string name, SqlDbType dbType, object value)
          : this(name, dbType, value, -1)
        {
        }

        public QueryParameter(string name, SqlDbType dbType, object value, int length)
        {
            this.Name = name;
            this.DbType = dbType;
            this.Value = value;
            this.Length = length;
        }

        public string Name { get; private set; }

        public SqlDbType DbType { get; private set; }

        public object Value { get; private set; }

        public int Length { get; private set; }

        internal SqlParameter GetSqlParameter()
        {
            SqlParameter sqlParameter = new SqlParameter(this.Name, this.DbType);
            ((DbParameter)sqlParameter).Value = this.Value ?? (object)DBNull.Value;
            if (this.Length > 0)
                ((DbParameter)sqlParameter).Size = this.Length;
            return sqlParameter;
        }
    }
}

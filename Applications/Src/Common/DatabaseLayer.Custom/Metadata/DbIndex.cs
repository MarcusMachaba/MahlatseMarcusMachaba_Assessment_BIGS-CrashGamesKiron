using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseLayer.Metadata
{
    public class DbIndex
    {
        private DbIndex()
        {
            this.Columns = new List<(string, bool)>();
            this.IncludedColumns = new List<string>();
        }

        public List<(string columnName, bool ascending)> Columns { get; private set; }

        public List<string> IncludedColumns { get; private set; }

        public string Name { get; private set; }

        public string TableName { get; private set; }

        public bool Unique { get; private set; }

        public static DbIndex Create(Type type, string indexName, BaseDataProvider provider) => new DbIndex()
        {
            Name = indexName,
            TableName = provider.GetTableName(type)
        };

        internal static DbIndex Create(string tableName, string indexName) => new DbIndex()
        {
            Name = indexName,
            TableName = tableName
        };

        public DbIndex IsUnique()
        {
            this.Unique = true;
            return this;
        }

        public DbIndex AddColumns(params string[] columns)
        {
            this.Columns.AddRange(((IEnumerable<string>)columns).Select<string, (string, bool)>((Func<string, (string, bool)>)(c => (c, true))));
            return this;
        }

        public DbIndex AddColumn(string column)
        {
            this.Columns.Add((column, true));
            return this;
        }

        public DbIndex AddColumnDesc(string column)
        {
            this.Columns.Add((column, false));
            return this;
        }

        public DbIndex AddIncludedColumns(params string[] includedColumns)
        {
            this.IncludedColumns.AddRange((IEnumerable<string>)includedColumns);
            return this;
        }
    }
}

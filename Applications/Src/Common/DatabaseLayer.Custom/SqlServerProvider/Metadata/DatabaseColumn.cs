using DatabaseLayer.Interfaces;
using System;

namespace DatabaseLayer.SqlServerProvider.Metadata
{
    public class DatabaseColumn : IColumnMetaData
    {
        public string Name { get; set; }

        public Type DataType { get; set; }

        public object DefaultValue { get; set; }

        public int Length { get; set; }

        public int? Precision { get; set; }

        public int? Scale { get; set; }

        public bool Required { get; set; }

        public string ForeignKeyTable { get; internal set; }

        public string ForeignKeyName { get; internal set; }

        public string ForeignKeyColumn { get; internal set; }
    }
}

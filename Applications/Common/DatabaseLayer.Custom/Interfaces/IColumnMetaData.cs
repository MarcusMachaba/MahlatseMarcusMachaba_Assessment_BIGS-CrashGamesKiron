using System;

namespace DatabaseLayer.Interfaces
{
    public interface IColumnMetaData
    {
        Type DataType { get; }

        string ForeignKeyTable { get; }

        string ForeignKeyName { get; }

        string ForeignKeyColumn { get; }

        int Length { get; }

        string Name { get; }

        int? Precision { get; }

        bool Required { get; }

        int? Scale { get; }
    }
}

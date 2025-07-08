using DatabaseLayer.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DatabaseLayer.Metadata
{
    public class TableMetadata
    {
        public TableMetadata(Type type, BaseDataProvider provider)
        {
            this.Type = type;
            this.TableContract = CustomAttributeExtensions.GetCustomAttribute<TableContractAttribute>((MemberInfo)type);
            if (this.TableContract == null)
                throw new ArgumentException("The type provided '" + type.FullName + "' does not have a table contract attribute defined.");
            this.Columns = new List<ColumnMetaData>();
            this.JoinedColumns = new List<ColumnMetaData>();
            this.QueryableColumns = new List<ColumnMetaData>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                ColumnContractAttribute customAttribute = CustomAttributeExtensions.GetCustomAttribute<ColumnContractAttribute>((MemberInfo)property);
                if (customAttribute != null)
                {
                    if (((MemberInfo)property).Name == this.TableContract.PrimaryKey)
                    {
                        this.PrimaryKeyProperty = new ColumnMetaData(this, property, customAttribute, provider);
                    }
                    else
                    {
                        ColumnMetaData columnMetaData = new ColumnMetaData(this, property, customAttribute, provider);
                        if (customAttribute.JoinedFrom.Length == 0)
                            this.Columns.Add(columnMetaData);
                        else
                            this.JoinedColumns.Add(columnMetaData);
                        if (customAttribute.Queryable)
                            this.QueryableColumns.Add(columnMetaData);
                    }
                }
            }
        }

        public List<ColumnMetaData> Columns { get; set; }

        public List<ColumnMetaData> QueryableColumns { get; set; }

        public List<ColumnMetaData> JoinedColumns { get; set; }

        public ColumnMetaData PrimaryKeyProperty { get; private set; }

        public TableContractAttribute TableContract { get; private set; }

        public Type Type { get; private set; }
    }
}

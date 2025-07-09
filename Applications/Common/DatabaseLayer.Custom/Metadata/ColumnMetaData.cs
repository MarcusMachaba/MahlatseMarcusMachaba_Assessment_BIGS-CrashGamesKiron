using DatabaseLayer.Attributes;
using DatabaseLayer.Interfaces;
using DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DatabaseLayer.Metadata
{
    public class ColumnMetaData : IColumnMetaData
    {
        private readonly BaseDataProvider mProvider;
        private readonly ColumnContractAttribute mColumnContract;
        private readonly PropertyInfo mPropertyInfo;

        public ColumnMetaData(
          TableMetadata table,
          PropertyInfo property,
          ColumnContractAttribute columnContract,
          BaseDataProvider provider)
        {
            this.Table = table;
            this.mPropertyInfo = property;
            this.mColumnContract = columnContract;
            this.mProvider = provider;
            if (!(property.PropertyType == typeof(char)) && !(property.PropertyType == typeof(char?)))
                return;
            this.mColumnContract.Length = 1;
        }

        public TableMetadata Table { get; }

        public string Name => ((MemberInfo)this.mPropertyInfo).Name;

        public Type DataType => this.mPropertyInfo.PropertyType;

        public bool IsTimeStamp => this.mColumnContract.IsTimeStamp;

        public int Length => this.mColumnContract.Length;

        public bool Required => this.mColumnContract.Required;

        public int? Precision => new int?(this.mColumnContract.Precision);

        public int? Scale => new int?(this.mColumnContract.Scale);

        public string[] JoinedFrom => this.mColumnContract.JoinedFrom;

        public object LengthString => (object)this.mColumnContract.LengthString;

        public bool HasForeignKey => this.mColumnContract.ForeignKeyType != (Type)null;

        public string ForeignKeyTable => this.HasForeignKey ? this.mProvider.GetTableName(this.mColumnContract.ForeignKeyType) : (string)null;

        public string ForeignKeyName
        {
            get
            {
                if (!this.HasForeignKey)
                    return (string)null;
                return "FK_" + this.mProvider.GetTableName(this.Table.Type) + "_" + this.Name + "_" + this.ForeignKeyTable + "_" + this.ForeignKeyColumn;
            }
        }

        public string ForeignKeyColumn => this.HasForeignKey ? new TableMetadata(this.mColumnContract.ForeignKeyType, this.mProvider).PrimaryKeyProperty.Name : (string)null;

        public object DefaultValue => this.mColumnContract.DefaultValue;

        internal T GetValue<T>(object obj) => (T)this.mPropertyInfo.GetValue(obj);

        internal void SetValue<T>(T obj, object value) => this.mPropertyInfo.SetValue((object)obj, value);
    }
}

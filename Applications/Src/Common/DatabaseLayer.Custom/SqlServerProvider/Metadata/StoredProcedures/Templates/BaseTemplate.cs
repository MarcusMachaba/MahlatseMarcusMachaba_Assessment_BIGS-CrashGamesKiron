using DatabaseLayer.Metadata;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures.Templates
{
    public abstract class BaseTemplate
    {
        public string FileGroup => this.Table.TableContract.FileGroup;

        public ColumnMetaData Primary => this.Table.PrimaryKeyProperty;

        public abstract string Template { get; }

        public abstract string Name { get; }

        public string Title => ((MemberInfo)this.Table.Type).Name;

        public string TableName => this.Provider.GetTableName(this.Table.Type);

        public TableMetadata Table { get; private set; }

        public BaseDataProvider Provider { get; private set; }

        public void Populate(TableMetadata table, BaseDataProvider provider)
        {
            this.Table = table;
            this.Provider = provider;
        }

        public string SqlParameters(bool IncludePrimary = true)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (IncludePrimary)
                stringBuilder.Append("   @" + this.Primary.Name + " " + this.GetSqlDataType(this.Primary));
            foreach (ColumnMetaData column in this.Table.Columns)
            {
                stringBuilder.Append((IncludePrimary ? ", " + Environment.NewLine : "") + "  @" + column.Name + " " + this.GetSqlDataType(column));
                IncludePrimary = true;
            }
            return stringBuilder.ToString();
        }

        public string GetSqlDataType(ColumnMetaData item)
        {
            if (item.JoinedFrom.Length != 0)
                item = this.Provider
                           .GetTableSpec(item.JoinedFrom[item.JoinedFrom.Length - 3])
                           .Columns
                           .First<ColumnMetaData>((Func<ColumnMetaData, bool>)(c => c.Name == item.JoinedFrom[item.JoinedFrom.Length - 1]));

            Type dataType = item.DataType;
            SqlDbType sqlDbType = TypeConverter.ConvertFromObjectType(!dataType.IsEnum ? (!dataType.IsGenericType || !(dataType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? item.DataType : item.DataType.GetGenericArguments()[0]) : Enum.GetUnderlyingType(item.DataType), item.Length, item.IsTimeStamp);

            // special‐case old LOBs
            if (sqlDbType == SqlDbType.Text)
                return string.Format("varchar({0})", item.LengthString);
            if (sqlDbType == SqlDbType.NText)
                return string.Format("nvarchar({0})", item.LengthString);

            // decimal / varbinary
            if (sqlDbType == SqlDbType.Decimal)
                return string.Format("decimal({0},{1})", (object)item.Precision, (object)item.Scale);
            if (sqlDbType == SqlDbType.VarBinary)
                return string.Format("varbinary({0})", item.LengthString);

            //// varchar / nvarchar
            //if (sqlDbType == SqlDbType.VarChar)
            //    return string.Format("varchar({0})", item.LengthString);
            //if (sqlDbType == SqlDbType.NVarChar)
            //    return string.Format("nvarchar({0})", item.LengthString);

            // everything else (int, datetime, etc.)
            return sqlDbType == SqlDbType.VarChar ? string.Format("varchar({0})", item.LengthString) : sqlDbType.ToString();
        }

        public string InsertParameters()
        {
            StringBuilder stringBuilder1 = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            ColumnMetaData columnMetaData = this.Table.Columns.Last<ColumnMetaData>();
            foreach (ColumnMetaData column in this.Table.Columns)
            {
                stringBuilder1.Append("       [" + column.Name + "]");
                stringBuilder2.Append("        @" + column.Name);
                if (column != columnMetaData)
                {
                    stringBuilder1.Append("," + Environment.NewLine);
                    stringBuilder2.Append("," + Environment.NewLine);
                }
            }
            stringBuilder1.Append(Environment.NewLine + "    )" + Environment.NewLine + "    VALUES" + Environment.NewLine + "    (" + Environment.NewLine);
            stringBuilder1.Append(stringBuilder2.ToString());
            return stringBuilder1.ToString();
        }

        public string UpdateParameters()
        {
            StringBuilder stringBuilder = new StringBuilder();
            ColumnMetaData columnMetaData = this.Table.Columns.Last<ColumnMetaData>();
            foreach (ColumnMetaData column in this.Table.Columns)
            {
                stringBuilder.Append("       [dbo].[" + this.TableName + "].[" + column.Name + "] = @" + column.Name);
                if (column != columnMetaData)
                    stringBuilder.Append("," + Environment.NewLine);
            }
            return stringBuilder.ToString();
        }
    }
}

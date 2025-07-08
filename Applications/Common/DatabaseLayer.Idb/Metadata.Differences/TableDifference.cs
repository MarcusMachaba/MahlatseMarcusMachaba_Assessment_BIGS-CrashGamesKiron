using DatabaseLayer.Interfaces;
using DatabaseLayer.SqlServerProvider.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DatabaseLayer.Metadata.Differences
{
    public class TableDifference
    {
        public TableDifference(TableMetadata table)
        {
            this.AdditionalColumns = new List<DatabaseColumn>();
            this.NewColumns = new List<ColumnMetaData>();
            this.ColumnDifferences = new List<ColumnDifference>();
            this.Table = table;
            this.StoredProcedureDifferences = new List<IStoredProcedure>();
        }

        public List<DatabaseColumn> AdditionalColumns { get; set; }

        public List<ColumnMetaData> NewColumns { get; set; }

        public List<ColumnDifference> ColumnDifferences { get; set; }

        public Mismatch PrimaryKey { get; internal set; }

        public TableMetadata Table { get; private set; }

        public bool Match => this.ColumnDifferences.Count == 0 && this.AdditionalColumns.Count == 0 && this.NewColumns.Count == 0 && this.StoredProcedureDifferences.Count == 0 && this.PrimaryKey == null;

        public bool NewTable => this.PrimaryKey != null && this.PrimaryKey.DbValue == null && this.NewColumns.Count > 0 && this.ColumnDifferences.Count == 0 && this.AdditionalColumns.Count == 0;

        public List<IStoredProcedure> StoredProcedureDifferences { get; internal set; }

        internal void DeployTableChanges(
          SqlConnection conn,
          DeploySettings deploySettings,
          BaseDataProvider provider)
        {
            if (this.NewTable)
            {
                this.ExecuteAddTableQuery(conn, provider);
            }
            else
            {
                if (this.PrimaryKey != null)
                    throw new InvalidOperationException("You cannot change a primary key on table '" + provider.GetTableName(this.Table.Type) + "'.");
                if (deploySettings.DropColumnsNotPresentInTableStuctures)
                {
                    foreach (DatabaseColumn additionalColumn in this.AdditionalColumns)
                        this.RemoveAdditionalColumn(additionalColumn, conn, provider);
                }
                this.AddNewColumns(conn, provider);
                foreach (ColumnDifference columnDifference in this.ColumnDifferences)
                    this.AlterColumn(columnDifference, conn, provider);
            }
        }

        internal void DeployStoredProcedureChanges(SqlConnection conn)
        {
            foreach (IStoredProcedure procedureDifference in this.StoredProcedureDifferences)
            {
                this.DropStoredProcedure(conn, procedureDifference.StoredProcedureName);
                this.CreateStoredProcedure(conn, procedureDifference.StoredProcedureCreateText);
            }
        }

        private void CreateStoredProcedure(SqlConnection conn, string createQuery)
        {
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = createQuery;
                ((DbCommand)command).ExecuteNonQuery();
            }
        }

        private void DropStoredProcedure(SqlConnection conn, string storedProcedureName)
        {
            string str = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + storedProcedureName + "]') AND type in (N'P', N'PC'))\r\nDROP PROCEDURE[dbo].[" + storedProcedureName + "]";
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = str;
                ((DbCommand)command).ExecuteNonQuery();
            }
        }

        internal void DeployForeignKeys(SqlConnection conn, BaseDataProvider provider)
        {
            foreach (ColumnMetaData newColumn in this.NewColumns)
                this.AddForeignKey(newColumn, conn, provider);
            foreach (ColumnDifference columnDifference in this.ColumnDifferences)
                this.AlterForeignKey(columnDifference, conn, provider);
        }

        private void RemoveAdditionalColumn(
          DatabaseColumn additionalColumn,
          SqlConnection conn,
          BaseDataProvider provider)
        {
            string str = "ALTER TABLE [dbo].[" + provider.GetTableName(this.Table.Type) + "]" + Environment.NewLine + ("DROP COLUMN [" + additionalColumn.Name + "]");
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = str;
                ((DbCommand)command).ExecuteNonQuery();
            }
        }

        private void AddNewColumns(SqlConnection conn, BaseDataProvider provider)
        {
            if (this.NewColumns.Count == 0)
                return;
            string str1 = "ALTER TABLE [dbo].[" + provider.GetTableName(this.Table.Type) + "] ADD";
            foreach (ColumnMetaData newColumn in this.NewColumns)
                str1 = str1 + Environment.NewLine + (" " + this.GetColumnSpec(newColumn, provider) + ",");
            string str2 = str1.Substring(0, str1.Length - 1);
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = str2;
                ((DbCommand)command).ExecuteNonQuery();
            }
        }

        private void AlterColumn(
          ColumnDifference columnDiff,
          SqlConnection conn,
          BaseDataProvider provider)
        {
            if (!columnDiff.ColumnChanged)
                return;
            if (columnDiff.DatabaseColumn.Name != columnDiff.ModelColumn.Name)
            {
                using (SqlCommand command = conn.CreateCommand())
                {
                    ((DbCommand)command).CommandText = "sp_rename";
                    ((DbCommand)command).CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@objname", (object)("dbo." + provider.GetTableName(this.Table.Type) + "." + columnDiff.DatabaseColumn.Name));
                    command.Parameters.AddWithValue("@newname", (object)columnDiff.ModelColumn.Name);
                    command.Parameters.AddWithValue("@objtype", (object)"COLUMN");
                    ((DbCommand)command).ExecuteNonQuery();
                }
            }
            string str = "ALTER TABLE [dbo].[" + provider.GetTableName(this.Table.Type) + "]" + Environment.NewLine + ("ALTER COLUMN " + this.GetColumnSpec(columnDiff.ModelColumn, provider));
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = str;
                ((DbCommand)command).ExecuteNonQuery();
            }
        }

        public void AlterForeignKey(
          ColumnDifference columnDiff,
          SqlConnection conn,
          BaseDataProvider provider)
        {
            if (!columnDiff.ForeignKeyChanged)
                return;
            if (columnDiff.DatabaseColumn.ForeignKeyName != null)
            {
                string str = "ALTER TABLE [dbo].[" + provider.GetTableName(this.Table.Type) + "]" + Environment.NewLine + ("DROP CONSTRAINT " + columnDiff.DatabaseColumn.ForeignKeyName);
                using (SqlCommand command = conn.CreateCommand())
                {
                    ((DbCommand)command).CommandText = str;
                    ((DbCommand)command).ExecuteNonQuery();
                }
            }
            this.AddForeignKey(columnDiff.ModelColumn, conn, provider);
        }

        private void AddForeignKey(
          ColumnMetaData modelColumn,
          SqlConnection conn,
          BaseDataProvider provider)
        {
            if (!modelColumn.HasForeignKey)
                return;
            string str = "ALTER TABLE [dbo].[" + provider.GetTableName(this.Table.Type) + "]" + Environment.NewLine + ("ADD " + this.GetForeignKey(modelColumn, provider));
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = str;
                ((DbCommand)command).ExecuteNonQuery();
            }
        }

        private string GetForeignKey(ColumnMetaData modelColumn, BaseDataProvider provider)
        {
            if (!modelColumn.HasForeignKey)
                return string.Empty;
            string str = modelColumn.ForeignKeyName;
            if (str.Length > 128)
                str = str.Substring(0, 128);
            return "CONSTRAINT [" + str + "] FOREIGN KEY ([" + modelColumn.Name + "]) REFERENCES [" + modelColumn.ForeignKeyTable + "]([" + modelColumn.ForeignKeyColumn + "])";
        }

        private void ExecuteAddTableQuery(SqlConnection conn, BaseDataProvider provider)
        {
            string str1 = "CREATE TABLE [dbo].[" + provider.GetTableName(this.Table.Type) + "]" + Environment.NewLine + "(" + Environment.NewLine + ("\t" + this.PrimaryKey.ModelValue + " INT NOT NULL IDENTITY(1,1),");
            foreach (ColumnMetaData newColumn in this.NewColumns)
                str1 = str1 + Environment.NewLine + "\t" + this.GetColumnSpec(newColumn, provider) + ",";
            string str2 = str1 + ("CONSTRAINT [PK_" + provider.GetTableName(this.Table.Type) + "_" + this.PrimaryKey.ModelValue + "] PRIMARY KEY CLUSTERED ( [" + this.Table.PrimaryKeyProperty.Name + "] ASC )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [" + this.Table.TableContract.FileGroup + "]") + Environment.NewLine + (") ON [" + this.Table.TableContract.FileGroup + "]");
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = str2;
                ((DbCommand)command).ExecuteNonQuery();
            }
        }

        private string GetColumnSpec(ColumnMetaData column, BaseDataProvider provider)
        {
            bool flag = column.Required;
            Type objectType = column.DataType;
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                flag = false;
                objectType = objectType.GetGenericArguments()[0];
            }
            string columnSpec = "[" + column.Name + "] " + this.GetType(column, objectType) + " " + (flag ? "NOT " : "") + "NULL";
            if (column.DefaultValue != null)
                columnSpec = columnSpec + " CONSTRAINT DF_" + provider.GetTableName(this.Table.Type) + "_" + column.Name + " DEFAULT(" + this.GetDefaultValueString(column) + ")";
            return columnSpec;
        }

        private string GetDefaultValueString(ColumnMetaData column)
        {
            if (column.DataType == typeof(string))
                return string.Format("'{0}'", column.DefaultValue);
            if (!(column.DataType == typeof(bool)) || !(column.DefaultValue is bool))
                return column.DefaultValue.ToString();
            return !(bool)column.DefaultValue ? "0" : "1";
        }

        private string GetType(ColumnMetaData column, Type objectType)
        {
            SqlDbType sqlDbType = TypeConverter.ConvertFromObjectType(objectType, column.Length, column.IsTimeStamp);
            string type = Enum.GetName(typeof(SqlDbType), (object)sqlDbType);
            if (sqlDbType == SqlDbType.VarBinary || sqlDbType == SqlDbType.VarChar)
                type = type + "(" + column.LengthString + ")";
            if (sqlDbType == SqlDbType.Decimal)
                type = type + "(" + (object)column.Precision + ", " + (object)column.Scale + ")";
            return type;
        }
    }
}

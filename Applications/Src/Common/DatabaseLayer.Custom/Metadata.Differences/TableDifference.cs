using DatabaseLayer.Interfaces;
using DatabaseLayer.SqlServerProvider.Metadata;
using DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures.Templates;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;

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
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.CreateProcedureFromText";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProcedureCreateText", createQuery);
                cmd.ExecuteNonQuery();
            }
        }

        private void DropStoredProcedure(SqlConnection conn, string storedProcedureName)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.DropProcedureIfExists";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProcedureName", storedProcedureName);
                cmd.ExecuteNonQuery();
            }
        }

        internal void DeployForeignKeys(SqlConnection conn, BaseDataProvider provider)
        {
            foreach (ColumnMetaData newColumn in this.NewColumns)
                this.AddForeignKey(newColumn, conn, provider);
            foreach (ColumnDifference columnDifference in this.ColumnDifferences)
                this.AlterForeignKey(columnDifference, conn, provider);
        }

        private void RemoveAdditionalColumn(DatabaseColumn additionalColumn, SqlConnection conn, BaseDataProvider provider)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.DropColumnFromTable";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TableName", provider.GetTableName(this.Table.Type));
                cmd.Parameters.AddWithValue("@ColumnName", additionalColumn.Name);
                cmd.ExecuteNonQuery();
            }
        }

        private void AddNewColumns(SqlConnection conn, BaseDataProvider provider)
        {
            if (this.NewColumns.Count == 0)
                return;

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.AddColumnsToTable";
                cmd.CommandType = CommandType.StoredProcedure;
                var defs = string.Join(",\r\n",
                    this.NewColumns.Select(c => this.GetColumnSpec(c, provider))
                );
                cmd.Parameters.AddWithValue("@TableName", provider.GetTableName(this.Table.Type));
                cmd.Parameters.AddWithValue("@ColumnDefinitions", defs);
                cmd.ExecuteNonQuery();
            }
        }

        private void AlterColumn(ColumnDifference columnDiff, SqlConnection conn, BaseDataProvider provider)
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

            var tableName = provider.GetTableName(this.Table.Type);
            var columnDefinition = this.GetColumnSpec(columnDiff.ModelColumn, provider);
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.AlterColumnInTable";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@ColumnDefinition", columnDefinition);
                cmd.ExecuteNonQuery();
            }
        }

        public void AlterForeignKey(ColumnDifference columnDiff, SqlConnection conn, BaseDataProvider provider)
        {
            if (!columnDiff.ForeignKeyChanged)
                return;
            if (columnDiff.DatabaseColumn.ForeignKeyName != null)
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "dbo.DropConstraintFromTable";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TableName", provider.GetTableName(this.Table.Type));
                    cmd.Parameters.AddWithValue("@ConstraintName", columnDiff.DatabaseColumn.ForeignKeyName);
                    cmd.ExecuteNonQuery();
                }
            }
            this.AddForeignKey(columnDiff.ModelColumn, conn, provider);
        }

        private void AddForeignKey(ColumnMetaData modelColumn, SqlConnection conn, BaseDataProvider provider)
        {
            if (!modelColumn.HasForeignKey)
                return;

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.AddForeignKeyToTable";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TableName", provider.GetTableName(this.Table.Type));
                cmd.Parameters.AddWithValue("@ForeignKeyDefinition", this.GetForeignKey(modelColumn, provider));
                cmd.ExecuteNonQuery();
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

        // TODO: Refactor this method to use a more structured approach for SQL type retrieval.
        private string GetColumnSqlType(ColumnMetaData column, BaseDataProvider provider)
        {
            var tmpl = new RetrieveTemplate();
            tmpl.Populate(this.Table, provider);
            return tmpl.GetSqlDataType(column);
        }

        private void ExecuteAddTableQuery(SqlConnection conn, BaseDataProvider provider)
        {
            if (this.PrimaryKey == null)
                throw new InvalidOperationException(
                    $"No primary key defined for table '{provider.GetTableName(this.Table.Type)}'. " +
                    "Please set PrimaryKey = nameof(YourKeyProperty) in your [TableContract].");

            var pkName = this.PrimaryKey.ModelValue;
            var colMeta = this.Table.PrimaryKeyProperty;
            var sqlDataType = this.GetColumnSqlType(colMeta, provider);

            bool isIntegral = sqlDataType.Equals("int", StringComparison.OrdinalIgnoreCase)
                   || sqlDataType.Equals("bigint", StringComparison.OrdinalIgnoreCase)
                   || sqlDataType.Equals("smallint", StringComparison.OrdinalIgnoreCase)
                   || sqlDataType.Equals("tinyint", StringComparison.OrdinalIgnoreCase);

            string pkColDef;
            var includeIdentity = this.Table.TableContract.Identity;
            if (isIntegral && includeIdentity)
            {
                pkColDef = $"[{pkName}] {sqlDataType} NOT NULL IDENTITY(1,1)";
            }
            else if (isIntegral && !includeIdentity)
            {
                pkColDef = $"[{pkName}] {sqlDataType} NOT NULL";
            }
            else if (sqlDataType.Equals("uniqueidentifier", StringComparison.OrdinalIgnoreCase))
            {
                pkColDef = $"[{pkName}] uniqueidentifier NOT NULL DEFAULT NEWSEQUENTIALID()";
            }
            else
            {
                pkColDef = $"[{pkName}] {sqlDataType} NOT NULL";
            }

            var allCols = new List<string> { pkColDef };
            allCols.AddRange(
                this.NewColumns
                    .Select(c => this.GetColumnSpec(c, provider))
            );
            var colDefs = string.Join(",\r\n", allCols);
            var compositDef = $"CONSTRAINT [PK_{provider.GetTableName(this.Table.Type)}] "
                            + $"PRIMARY KEY CLUSTERED ([{colMeta.Name}] ASC, [{colMeta.Table.Columns.FirstOrDefault().ForeignKeyColumn}] ASC) "
                            + this.Table.TableContract.FileGroup switch
                            { var fg => $"WITH(PAD_INDEX=OFF,STATISTICS_NORECOMPUTE=OFF,IGNORE_DUP_KEY=OFF,ALLOW_ROW_LOCKS=ON,ALLOW_PAGE_LOCKS=ON) ON [{fg}]" };
            var pkDef = $"CONSTRAINT [PK_{provider.GetTableName(this.Table.Type)}_{this.PrimaryKey.ModelValue}] "
                       + $"PRIMARY KEY CLUSTERED ([{colMeta.Name}] ASC) "
                       + this.Table.TableContract.FileGroup switch
                       { var fg => $"WITH(PAD_INDEX=OFF,STATISTICS_NORECOMPUTE=OFF,IGNORE_DUP_KEY=OFF,ALLOW_ROW_LOCKS=ON,ALLOW_PAGE_LOCKS=ON) ON [{fg}]" };
            var primaryKeyDefinition = includeIdentity ? pkDef : compositDef;
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.CreateTableWithColumns";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TableName", provider.GetTableName(this.Table.Type));
                cmd.Parameters.AddWithValue("@ColumnDefinitions", colDefs);
                cmd.Parameters.AddWithValue("@PrimaryKeyDefinition", primaryKeyDefinition);
                cmd.Parameters.AddWithValue("@FileGroup", this.Table.TableContract.FileGroup);
                cmd.ExecuteNonQuery();
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

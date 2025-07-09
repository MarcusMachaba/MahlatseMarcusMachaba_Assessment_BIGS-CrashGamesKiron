using DatabaseLayer.Interfaces;
using DatabaseLayer.Metadata;
using DatabaseLayer.Metadata.Differences;
using DatabaseLayer.SqlServerProvider.Metadata;
using DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace DatabaseLayer.SqlServerProvider
{
    public static class CompareToDbObject
    {
        public static TableDifference Compare(
          BaseDataProvider dataProvider,
          TableMetadata tableMetadata,
          string connectionString)
        {
            TableDifference tableDifference = new TableDifference(tableMetadata);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                ((DbConnection)conn).Open();
                DatabaseTable databaseTable = Provider.BuildDbTable(conn, dataProvider.GetTableName(tableMetadata.Type));
                if (tableMetadata.PrimaryKeyProperty.Name.CompareTo(databaseTable.PrimaryKey) != 0)
                    tableDifference.PrimaryKey = new Mismatch(tableMetadata.PrimaryKeyProperty.Name, databaseTable.PrimaryKey);
                foreach (ColumnMetaData column in tableMetadata.Columns)
                {
                    ColumnMetaData col = column;
                    bool columnChanged = false;
                    DatabaseColumn dbCol = databaseTable.ColumnList.FirstOrDefault<DatabaseColumn>((Func<DatabaseColumn, bool>)(dbColumn => col.Name.ToLower().CompareTo(dbColumn.Name.ToLower()) == 0));
                    if (dbCol == null)
                    {
                        tableDifference.NewColumns.Add(col);
                    }
                    else
                    {
                        Type enumType = col.DataType;
                        bool flag = col.Required;
                        if (enumType.IsGenericType && enumType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            flag = false;
                            enumType = enumType.GetGenericArguments()[0];
                        }
                        if (enumType.IsEnum)
                            enumType = Enum.GetUnderlyingType(enumType);
                        if (enumType != dbCol.DataType || flag != dbCol.Required || col.Length != dbCol.Length || col.Name != dbCol.Name)
                            columnChanged = true;
                        if (enumType == typeof(Decimal) && !columnChanged)
                        {
                            int? nullable1 = col.Precision;
                            int? nullable2 = dbCol.Precision;
                            if (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() & nullable1.HasValue == nullable2.HasValue)
                            {
                                nullable2 = col.Scale;
                                nullable1 = dbCol.Scale;
                                if (nullable2.GetValueOrDefault() == nullable1.GetValueOrDefault() & nullable2.HasValue == nullable1.HasValue)
                                    goto label_17;
                            }
                            columnChanged = true;
                        }
                    label_17:
                        bool foreignKeyChanged = CompareToDbObject.CheckForeignKeyChanged(col, dbCol);
                        if (columnChanged | foreignKeyChanged)
                            tableDifference.ColumnDifferences.Add(new ColumnDifference(col, dbCol, columnChanged, foreignKeyChanged));
                    }
                }
                tableDifference.AdditionalColumns = databaseTable.ColumnList.Where<DatabaseColumn>((Func<DatabaseColumn, bool>)(c => !tableMetadata.Columns.Any<ColumnMetaData>((Func<ColumnMetaData, bool>)(col => col.Name.ToLower() == c.Name.ToLower())) && c.Name != tableMetadata.PrimaryKeyProperty.Name)).ToList<DatabaseColumn>();
                foreach (IStoredProcedure storedProcedure in Generator.GetStoredProcedures(tableMetadata, dataProvider))
                {
                    if (!StoredProcedureHelper.CheckStoredProcedure(conn, storedProcedure))
                        tableDifference.StoredProcedureDifferences.Add(storedProcedure);
                }
            }
            return tableDifference;
        }

        private static bool CheckForeignKeyChanged(ColumnMetaData col, DatabaseColumn dbCol)
        {
            bool flag = false;
            if (col.HasForeignKey)
            {
                if (dbCol.ForeignKeyName == null)
                    flag = true;
                else if (col.ForeignKeyTable != dbCol.ForeignKeyTable || col.ForeignKeyColumn != dbCol.ForeignKeyColumn)
                    flag = true;
            }
            else if (dbCol.ForeignKeyName != null)
                flag = true;
            return flag;
        }

        public static IndexDifference Compare(
          BaseDataProvider dataProvider,
          DbIndex index,
          string connectionString)
        {
            IndexDifference indexDifference = new IndexDifference(index);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                ((DbConnection)conn).Open();
                indexDifference.DatabaseIndex = Provider.BuildIndex(conn, index.Name);
            }
            return indexDifference;
        }
    }
}

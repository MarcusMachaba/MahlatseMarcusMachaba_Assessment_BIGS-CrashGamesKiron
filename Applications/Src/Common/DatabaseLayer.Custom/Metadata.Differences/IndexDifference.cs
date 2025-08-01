﻿using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace DatabaseLayer.Metadata.Differences
{
    public class IndexDifference
    {
        public IndexDifference(DbIndex index) => this.ObjectIndex = index;

        public DbIndex DatabaseIndex { get; set; }

        public DbIndex ObjectIndex { get; set; }

        public bool Match => this.DatabaseIndex != null && this.ObjectIndex != null && this.CompareColumns(this.DatabaseIndex.Columns, this.ObjectIndex.Columns) && this.CompareColumns(this.DatabaseIndex.IncludedColumns, this.ObjectIndex.IncludedColumns) && this.DatabaseIndex.Unique == this.ObjectIndex.Unique;

        private bool CompareColumns(List<string> includedColumns1, List<string> includedColumns2) => includedColumns1.Count == includedColumns2.Count && !includedColumns1.Except<string>((IEnumerable<string>)includedColumns2).Any<string>() && !includedColumns2.Except<string>((IEnumerable<string>)includedColumns1).Any<string>();

        private bool CompareColumns(
          List<(string columnName, bool ascending)> columns1,
          List<(string columnName, bool ascending)> columns2)
        {
            if (columns1.Count != columns2.Count)
                return false;
            for (int index = 0; index < columns1.Count; ++index)
            {
                if (columns1[index].ascending != columns2[index].ascending || columns1[index].columnName != columns2[index].columnName)
                    return false;
            }
            return true;
        }

        internal void Deploy(SqlConnection conn, DeploySettings settings, BaseDataProvider baseDataProvider)
        {
            // 1) Drop existing index if it exists
            if (this.DatabaseIndex != null)
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "dbo.DropIndexOnTable";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TableName", this.DatabaseIndex.TableName);
                    cmd.Parameters.AddWithValue("@IndexName", this.DatabaseIndex.Name);
                    // optional if we only ever use dbo:
                    // cmd.Parameters.AddWithValue("@SchemaName", "dbo");

                    ((DbCommand)cmd).ExecuteNonQuery();
                }
            }

            // 2) Build comma-delimited column lists
            var keyCols = string.Join(", ", this.ObjectIndex.Columns.Select(c => $"[{c.columnName}] {(c.ascending ? "ASC" : "DESC")}"));
            var inclCols = this.ObjectIndex.IncludedColumns.Any()
                ? string.Join(", ", this.ObjectIndex.IncludedColumns.Select(c => $"[{c}]"))
                : string.Empty;

            // 3) Call the CREATE-INDEX proc
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.CreateIndexOnTable";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TableName", this.ObjectIndex.TableName);
                cmd.Parameters.AddWithValue("@IndexName", this.ObjectIndex.Name);
                cmd.Parameters.AddWithValue("@IsUnique", this.ObjectIndex.Unique);
                cmd.Parameters.AddWithValue("@KeyColumns", keyCols);
                cmd.Parameters.AddWithValue("@IncludedColumns", inclCols);
                // cmd.Parameters.AddWithValue("@SchemaName",     "dbo");

                cmd.ExecuteNonQuery();
            }
        }
    }
}

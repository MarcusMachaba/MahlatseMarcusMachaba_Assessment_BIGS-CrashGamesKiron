using DatabaseLayer.Metadata;
using System.Data.Common;
using System.Data.SqlClient;

namespace DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures
{
    internal static class DatabaseIndexProvider
    {
        private const string GetIndexTableName = "SELECT t.name AS 'TableName', ind.is_unique AS 'IsUnique' FROM sys.indexes ind INNER JOIN sys.tables t ON ind.object_id = t.object_id WHERE ind.name = @IndexName";
        private const string GetColumns = "\r\nSELECT \r\n    CASE is_descending_key WHEN 1 THEN 'DESC' ELSE 'ASC' END AS 'Sorting',\r\n\tcol.name AS 'Column'\r\nFROM \r\n    sys.indexes ind \r\nLEFT JOIN \r\n    sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id \r\nLEFT JOIN \r\n    sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id \r\nWHERE \r\n    ind.name = @IndexName\r\n    AND is_included_column = 0\r\nORDER BY \r\n    key_ordinal";
        private const string GetIncludedColumns = "\r\nSELECT \r\n    col.name AS 'Column'\r\nFROM \r\n    sys.indexes ind \r\nLEFT JOIN \r\n    sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id \r\nLEFT JOIN \r\n    sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id \r\nWHERE \r\n    ind.name = @IndexName\r\n    AND is_included_column = 1\r\nORDER BY \r\n    key_ordinal";

        internal static DbIndex GetIndex(SqlConnection conn, string name)
        {
            DbIndex index;
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = "SELECT t.name AS 'TableName', ind.is_unique AS 'IsUnique' FROM sys.indexes ind INNER JOIN sys.tables t ON ind.object_id = t.object_id WHERE ind.name = @IndexName";
                command.Parameters.AddWithValue("@IndexName", (object)name);
                using (SqlDataReader sqlDataReader = command.ExecuteReader())
                {
                    if (!((DbDataReader)sqlDataReader).Read())
                        return (DbIndex)null;
                    index = DbIndex.Create(((DbDataReader)sqlDataReader)["TableName"] as string, name);
                    if ((bool)((DbDataReader)sqlDataReader)["IsUnique"])
                        index.IsUnique();
                }
            }
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = "\r\nSELECT \r\n    CASE is_descending_key WHEN 1 THEN 'DESC' ELSE 'ASC' END AS 'Sorting',\r\n\tcol.name AS 'Column'\r\nFROM \r\n    sys.indexes ind \r\nLEFT JOIN \r\n    sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id \r\nLEFT JOIN \r\n    sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id \r\nWHERE \r\n    ind.name = @IndexName\r\n    AND is_included_column = 0\r\nORDER BY \r\n    key_ordinal";
                command.Parameters.AddWithValue("@IndexName", (object)name);
                using (SqlDataReader sqlDataReader = command.ExecuteReader())
                {
                    while (((DbDataReader)sqlDataReader).Read())
                    {
                        if (((DbDataReader)sqlDataReader)["Sorting"] as string == "ASC")
                            index.AddColumn(((DbDataReader)sqlDataReader)["Column"] as string);
                        else
                            index.AddColumnDesc(((DbDataReader)sqlDataReader)["Column"] as string);
                    }
                }
            }
            using (SqlCommand command = conn.CreateCommand())
            {
                ((DbCommand)command).CommandText = "\r\nSELECT \r\n    col.name AS 'Column'\r\nFROM \r\n    sys.indexes ind \r\nLEFT JOIN \r\n    sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id \r\nLEFT JOIN \r\n    sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id \r\nWHERE \r\n    ind.name = @IndexName\r\n    AND is_included_column = 1\r\nORDER BY \r\n    key_ordinal";
                command.Parameters.AddWithValue("@IndexName", (object)name);
                using (SqlDataReader sqlDataReader = command.ExecuteReader())
                {
                    while (((DbDataReader)sqlDataReader).Read())
                        index.AddIncludedColumns(((DbDataReader)sqlDataReader)["Column"] as string);
                }
            }
            return index;
        }
    }
}

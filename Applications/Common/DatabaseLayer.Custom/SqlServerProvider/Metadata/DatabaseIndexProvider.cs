using DatabaseLayer.Metadata;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace DatabaseLayer.SqlServerProvider.Metadata
{
    internal static class DatabaseIndexProvider
    {
        internal static DbIndex GetIndex(SqlConnection conn, string name)
        {
            DbIndex index;

            // 1) Table name + uniqueness
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.GetIndexInfo";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IndexName", name);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    index = DbIndex.Create(reader["TableName"] as string, name);
                    if ((bool)reader["IsUnique"])
                        index.IsUnique();
                }
            }

            // 2) Key columns & sorting (ASC/DESC)
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.GetIndexColumns";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IndexName", name);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Sorting"] as string == "ASC")
                            index.AddColumn(reader["Column"] as string);
                        else
                            index.AddColumnDesc(reader["Column"] as string);
                    }
                }
            }

            // 3) Included columns
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.GetIncludedIndexColumns";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IndexName", name);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        index.AddIncludedColumns(reader["Column"] as string);
                }
            }

            return index;
        }
    }
}

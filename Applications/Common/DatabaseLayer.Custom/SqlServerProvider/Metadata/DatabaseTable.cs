using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseLayer.SqlServerProvider.Metadata
{
    public class DatabaseTable
    {
        public DatabaseTable(SqlConnection conn, string tableName)
        {
            ColumnList = new List<DatabaseColumn>();

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.GetTablePrimaryKeyColumns";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TableName", tableName);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PrimaryKey = reader["COLUMN_NAME"].ToString();
                    }
                }
            }

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.GetTableColumns";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TableName", tableName);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ColumnList.Add(new DatabaseColumn
                        {
                            Name = reader["COLUMN_NAME"].ToString(),
                            Required = (uint)reader["IS_NULLABLE"].ToString().ToUpper().CompareTo("YES") > 0U,
                            DataType = TypeConverter.ConvertFromSqlType(reader["DATA_TYPE"].ToString(), string.IsNullOrEmpty(reader["CHARACTER_MAXIMUM_LENGTH"].ToString()) ? 0 : int.Parse(reader["CHARACTER_MAXIMUM_LENGTH"].ToString())),
                            Length = string.IsNullOrEmpty(reader["CHARACTER_MAXIMUM_LENGTH"].ToString()) || reader["DATA_TYPE"].ToString().ToLower() == "image" || reader["DATA_TYPE"].ToString().ToLower() == "text" ? 0 : int.Parse(reader["CHARACTER_MAXIMUM_LENGTH"].ToString()),
                            DefaultValue = string.IsNullOrEmpty(reader["COLUMN_DEFAULT"].ToString()) ? null : reader["COLUMN_DEFAULT"],       //reader["COLUMN_DEFAULT"] == DBNull.Value ? null : reader["COLUMN_DEFAULT"],
                            Precision = new int?(string.IsNullOrEmpty(reader["NUMERIC_PRECISION"].ToString()) ? 0 : int.Parse(reader["NUMERIC_PRECISION"].ToString())), //reader["NUMERIC_PRECISION"] as int?,
                            Scale = new int?(string.IsNullOrEmpty(reader["NUMERIC_SCALE"].ToString()) ? 0 : int.Parse(reader["NUMERIC_SCALE"].ToString())) //reader["NUMERIC_SCALE"] as int ?
                        });
                    }
                }
            }

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.GetForeignKeyInfo";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TableName", tableName);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string currentKey = reader["CurrentColumn"].ToString();
                        DatabaseColumn databaseColumn = ColumnList.FirstOrDefault(cl => cl.Name == currentKey);
                        databaseColumn.ForeignKeyTable = reader["ForeignTable"].ToString();
                        databaseColumn.ForeignKeyColumn = reader["ForeignColumn"].ToString();
                        databaseColumn.ForeignKeyName = reader["Name"].ToString();
                    }
                }
            }

            TableName = tableName;
        }

        public string TableName { get; set; }

        public string PrimaryKey { get; set; }

        public List<DatabaseColumn> ColumnList { get; set; }
    }
}

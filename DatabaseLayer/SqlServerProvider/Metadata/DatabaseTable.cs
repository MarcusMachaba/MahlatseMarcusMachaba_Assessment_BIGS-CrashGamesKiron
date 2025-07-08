using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace DatabaseLayer.SqlServerProvider.Metadata
{
    public class DatabaseTable
    {
        public DatabaseTable(SqlConnection conn, string tableName)
        {
            ColumnList = new List<DatabaseColumn>();
            using (SqlCommand sqlCommand = new SqlCommand("SELECT \r\n                                ccu.TABLE_NAME,\r\n                                ccu.COLUMN_NAME, \r\n                                ccu.CONSTRAINT_NAME\r\n                            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc\r\n                                INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ccu  ON tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME\r\n                            WHERE \r\n                                tc.TABLE_NAME = @TableName\r\n\t                            AND ccu.CONSTRAINT_NAME LIKE 'PK_%'", conn))
            {
                sqlCommand.Parameters.AddWithValue("@TableName", tableName);
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                while (sqlDataReader.Read())
                    PrimaryKey = ((DbDataReader)sqlDataReader)["COLUMN_NAME"].ToString();
                sqlDataReader.Close();
            }
            SqlCommand sqlCommand1 = new SqlCommand("SELECT \r\n                        COLUMN_NAME,\r\n                        IS_NULLABLE,\r\n                        DATA_TYPE,\r\n                        CHARACTER_MAXIMUM_LENGTH,\r\n                        COLUMN_DEFAULT, \r\n                        NUMERIC_PRECISION,\r\n                        NUMERIC_SCALE\r\n                    FROM INFORMATION_SCHEMA.COLUMNS\r\n                    WHERE TABLE_NAME = @TableName", conn);
            sqlCommand1.Parameters.AddWithValue("@TableName", tableName);
            SqlDataReader sqlDataReader1 = sqlCommand1.ExecuteReader();
            while (sqlDataReader1.Read())
                ColumnList.Add(new DatabaseColumn()
                {
                    Name = ((DbDataReader)sqlDataReader1)["COLUMN_NAME"].ToString(),
                    Required = (uint)((DbDataReader)sqlDataReader1)["IS_NULLABLE"].ToString().ToUpper().CompareTo("YES") > 0U,
                    DataType = TypeConverter.ConvertFromSqlType(((DbDataReader)sqlDataReader1)["DATA_TYPE"].ToString(), string.IsNullOrEmpty(((DbDataReader)sqlDataReader1)["CHARACTER_MAXIMUM_LENGTH"].ToString()) ? 0 : int.Parse(((DbDataReader)sqlDataReader1)["CHARACTER_MAXIMUM_LENGTH"].ToString())),
                    Length = string.IsNullOrEmpty(((DbDataReader)sqlDataReader1)["CHARACTER_MAXIMUM_LENGTH"].ToString()) || ((DbDataReader)sqlDataReader1)["DATA_TYPE"].ToString().ToLower() == "image" || ((DbDataReader)sqlDataReader1)["DATA_TYPE"].ToString().ToLower() == "text" ? 0 : int.Parse(((DbDataReader)sqlDataReader1)["CHARACTER_MAXIMUM_LENGTH"].ToString()),
                    DefaultValue = string.IsNullOrEmpty(((DbDataReader)sqlDataReader1)["COLUMN_DEFAULT"].ToString()) ? null : ((DbDataReader)sqlDataReader1)["COLUMN_DEFAULT"],
                    Precision = new int?(string.IsNullOrEmpty(((DbDataReader)sqlDataReader1)["NUMERIC_PRECISION"].ToString()) ? 0 : int.Parse(((DbDataReader)sqlDataReader1)["NUMERIC_PRECISION"].ToString())),
                    Scale = new int?(string.IsNullOrEmpty(((DbDataReader)sqlDataReader1)["NUMERIC_SCALE"].ToString()) ? 0 : int.Parse(((DbDataReader)sqlDataReader1)["NUMERIC_SCALE"].ToString()))
                });
            sqlDataReader1.Close();
            SqlCommand sqlCommand2 = new SqlCommand("SELECT \r\n                        Name = FK.CONSTRAINT_NAME,\r\n                        CurrentColumn = CU.COLUMN_NAME, \r\n                        ForeignTable = PK.TABLE_NAME, \r\n                        ForeignColumn = PT.COLUMN_NAME \r\n                    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C \r\n                        INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME \r\n                        INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME \r\n                        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME \r\n                        INNER JOIN ( SELECT \r\n                                           i1.TABLE_NAME, \r\n                                           i2.COLUMN_NAME \r\n                                       FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1 \r\n                                           INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME \r\n                                       WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY' \r\n                                   ) PT ON PT.TABLE_NAME = PK.TABLE_NAME \r\n                    WHERE FK.TABLE_NAME = @TableName", conn);
            sqlCommand2.Parameters.AddWithValue("@TableName", tableName);
            SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
            while (sqlDataReader2.Read())
            {
                string CurrentKey = ((DbDataReader)sqlDataReader2)["CurrentColumn"].ToString();
                DatabaseColumn databaseColumn = ColumnList.FirstOrDefault(cl => cl.Name == CurrentKey);
                databaseColumn.ForeignKeyTable = ((DbDataReader)sqlDataReader2)["ForeignTable"].ToString();
                databaseColumn.ForeignKeyColumn = ((DbDataReader)sqlDataReader2)["ForeignColumn"].ToString();
                databaseColumn.ForeignKeyName = ((DbDataReader)sqlDataReader2)["Name"].ToString();
            }
          sqlDataReader2.Close();
        }

        public string TableName { get; set; }

        public string PrimaryKey { get; set; }

        public List<DatabaseColumn> ColumnList { get; set; }
    }
}

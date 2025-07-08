using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

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

        internal void Deploy(
          SqlConnection conn,
          DeploySettings settings,
          BaseDataProvider baseDataProvider)
        {
            if (this.DatabaseIndex != null)
            {
                using (SqlCommand command = conn.CreateCommand())
                {
                    ((DbCommand)command).CommandText = "DROP INDEX [" + this.DatabaseIndex.Name + "] ON [dbo].[" + this.DatabaseIndex.TableName + "]";
                    ((DbCommand)command).ExecuteNonQuery();
                }
            }
            using (SqlCommand command = conn.CreateCommand())
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("CREATE " + (this.ObjectIndex.Unique ? "UNIQUE " : "") + "NONCLUSTERED INDEX[" + this.ObjectIndex.Name + "] ON[dbo].[" + this.ObjectIndex.TableName + "]");
                stringBuilder.AppendLine("(");
                for (int index = 0; index < this.ObjectIndex.Columns.Count; ++index)
                    stringBuilder.AppendLine("   " + this.ObjectIndex.Columns[index].columnName + " " + (this.ObjectIndex.Columns[index].ascending ? "ASC" : "DESC") + (index == this.ObjectIndex.Columns.Count - 1 ? "" : ","));
                stringBuilder.AppendLine(")");
                if (this.ObjectIndex.IncludedColumns.Count > 0)
                {
                    stringBuilder.AppendLine("INCLUDE");
                    stringBuilder.AppendLine("(");
                    for (int index = 0; index < this.ObjectIndex.IncludedColumns.Count; ++index)
                        stringBuilder.AppendLine("   " + this.ObjectIndex.IncludedColumns[index] + " " + (index == this.ObjectIndex.IncludedColumns.Count - 1 ? "" : ","));
                    stringBuilder.AppendLine(")");
                }
                stringBuilder.AppendLine("WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                ((DbCommand)command).CommandText = stringBuilder.ToString();
                ((DbCommand)command).ExecuteNonQuery();
            }
        }
    }
}

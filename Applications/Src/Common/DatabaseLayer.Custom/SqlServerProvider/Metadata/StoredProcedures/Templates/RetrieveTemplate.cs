using DatabaseLayer.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures.Templates
{
    public class RetrieveTemplate : BaseTemplate
    {
        private Dictionary<string, RetrieveTemplate.JoinSpec> _joins;

        public override string Name => "sp" + this.FileGroup + "_" + this.Title + "_Retrieve";

        public override string Template => "CREATE PROCEDURE [dbo].[" + this.Name + "]\r\n(\r\n    @" + this.Primary.Name + " " + this.GetSqlDataType(this.Primary) + " = null" + this.AdditionalParameters() + "\r\n)\r\nAS\r\nBEGIN\r\n\tSET NOCOUNT ON\r\n\r\n" + this.ParameterSniffingDeclarations() + "\r\n\r\n\tSELECT\r\n" + this.RetrieveParameters() + "\r\n    FROM dbo.[" + this.TableName + "] " + this.GetJoinText() + "\r\n\tWHERE\r\n" + this.WhereClause() + "\r\nEND";

        private string ParameterSniffingDeclarations()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(
                "\tDECLARE @in{0} {1} = @{0}",
                this.Primary.Name,
                this.GetSqlDataType(this.Primary)
            );

            foreach (var col in this.Table.QueryableColumns.Except(new[] { this.Primary }))
            {
                sb.AppendLine(",");
                sb.AppendFormat(
                    "\t\t@in{0} {1} = @{0}",
                    col.Name,
                    this.GetSqlDataType(col)
                );
            }

            return sb.ToString();
        }

        private Dictionary<string, RetrieveTemplate.JoinSpec> Joins
        {
            get
            {
                if (this._joins == null)
                {
                    this._joins = new Dictionary<string, RetrieveTemplate.JoinSpec>();
                    this.BuildJoins();
                }
                return this._joins;
            }
        }

        private Dictionary<string, int> Aliases { get; set; }

        private string GetJoinText()
        {
            string joinText = string.Empty;
            foreach (RetrieveTemplate.JoinSpec joinSpec in this.Joins.Values)
                joinText = joinText + Environment.NewLine + "\t\tLEFT JOIN [" + joinSpec.TableName + "] AS " + joinSpec.Alias + " ON " + joinSpec.FromAlias + ".[" + joinSpec.FromColumn + "] = " + joinSpec.Alias + ".[" + joinSpec.ToColumn + "]";
            return joinText;
        }

        private void BuildJoins()
        {
            foreach (ColumnMetaData joinedColumn in this.Table.JoinedColumns)
            {
                string str1 = this.TableName ?? "";
                string key = string.Empty;
                for (int index = 0; index + 1 < joinedColumn.JoinedFrom.Length; index += 2)
                {
                    string toType = joinedColumn.JoinedFrom[index];
                    string str2 = joinedColumn.JoinedFrom[index + 1];
                    key = key + (key.Length > 0 ? "." : "") + str2 + "." + toType;
                    if (this.Joins.ContainsKey(key))
                    {
                        str1 = this.Joins[key].Alias;
                    }
                    else
                    {
                        TableMetadata tableSpec = this.Provider.GetTableSpec(toType);
                        RetrieveTemplate.JoinSpec joinSpec = new RetrieveTemplate.JoinSpec()
                        {
                            TableName = this.Provider.GetTableName(tableSpec.Type),
                            Alias = this.GetNextAlias(toType),
                            FromAlias = str1,
                            FromColumn = str2,
                            ToColumn = tableSpec.PrimaryKeyProperty.Name
                        };
                        str1 = joinSpec.Alias;
                        this.Joins.Add(key, joinSpec);
                    }
                }
            }
        }

        private string GetNextAlias(string toType)
        {
            if (this.Aliases == null)
                this.Aliases = new Dictionary<string, int>();
            if (!this.Aliases.ContainsKey(toType))
                this.Aliases.Add(toType, 0);
            return string.Format("{0}{1}", (object)toType, (object)++this.Aliases[toType]);
        }

        public string WhereClause()
        {
            string str = "\t\t(" + this.GetFullColumnNameNoAlias(this.Primary) + " = @in" + this.Primary.Name + " OR @in" + this.Primary.Name + " IS NULL)";
            foreach (ColumnMetaData queryableColumn in this.Table.QueryableColumns)
                str = str + Environment.NewLine + "\t\tAND (" + this.GetFullColumnNameNoAlias(queryableColumn) + " = @in" + queryableColumn.Name + " OR @in" + queryableColumn.Name + " IS NULL)";
            return str;
        }

        public string AdditionalParameters()
        {
            string str = string.Empty;
            foreach (ColumnMetaData queryableColumn in this.Table.QueryableColumns)
                str = str + "," + Environment.NewLine + "\t@" + queryableColumn.Name + " " + this.GetSqlDataType(queryableColumn) + " = NULL";
            return str;
        }

        public string RetrieveParameters()
        {
            string str = "\t\t" + this.GetFullColumnName(this.Primary);
            foreach (ColumnMetaData columnMetaData in this.Table.Columns.Union<ColumnMetaData>((IEnumerable<ColumnMetaData>)this.Table.JoinedColumns))
                str = str + "," + Environment.NewLine + "\t\t" + this.GetFullColumnName(columnMetaData);
            return str;
        }

        private string GetFullColumnName(ColumnMetaData item) => this.GetFullColumnNameNoAlias(item) + " AS [" + item.Name + "]";

        private string GetFullColumnNameNoAlias(ColumnMetaData item)
        {
            if (item.JoinedFrom.Length == 0)
                return "[dbo].[" + this.TableName + "].[" + item.Name + "]";
            string key = string.Empty;
            for (int index = 0; index + 1 < item.JoinedFrom.Length; index += 2)
            {
                string str1 = item.JoinedFrom[index];
                string str2 = item.JoinedFrom[index + 1];
                key = key + (key.Length > 0 ? "." : "") + str2 + "." + str1;
            }
            return "[" + this.Joins[key].Alias + "].[" + ((IEnumerable<string>)item.JoinedFrom).Last<string>() + "]";
        }

        private class JoinSpec
        {
            public string Alias { get; set; }

            public string FromAlias { get; set; }

            public string FromColumn { get; set; }

            public string TableName { get; set; }

            public string ToColumn { get; set; }
        }
    }
}

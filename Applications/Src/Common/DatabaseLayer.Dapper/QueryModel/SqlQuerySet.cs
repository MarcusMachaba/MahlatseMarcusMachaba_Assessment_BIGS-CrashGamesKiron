using Dapper;

namespace DatabaseLayer.Dapper.QueryModel
{
    internal class SqlQuerySet
    {
        public string Query { get; }
        public DynamicParameters Parameters { get; }
        public SqlQuerySet(string query, DynamicParameters dynamicParameters)
        {
            this.Query = query;
            this.Parameters = dynamicParameters;
        }
    }
}

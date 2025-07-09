using DatabaseLayer.SqlServerProvider.Metadata;
using Microsoft.Data.SqlClient;
using System;

namespace DatabaseLayer.Metadata
{
    public static class Provider
    {
        internal static TableMetadata Build(Type type, BaseDataProvider provider) => new TableMetadata(type, provider);

        public static TableMetadata Build<T>(BaseDataProvider provider) => Provider.Build(typeof(T), provider);

        public static DatabaseTable BuildDbTable(SqlConnection conn, string tableName) => new DatabaseTable(conn, tableName);

        internal static DbIndex BuildIndex(SqlConnection conn, string name) => DatabaseIndexProvider.GetIndex(conn, name);
    }
}

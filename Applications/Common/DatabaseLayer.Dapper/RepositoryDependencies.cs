namespace DatabaseLayer.Dapper
{
    public static class RepositoryDependencies
    {
        internal static string Database { get; private set; }
        public static void SetMongoDatabase(string database)
        {
            Database = database;
        }
        internal static string MongoConnectionString { get; private set; }
        public static void SetMongoConnection(string connection)
        {
            MongoConnectionString = connection;
        }

        public static string SqlConnectionString { get; private set; }
        public static void SetSqlConnection(string connection)
        {
            SqlConnectionString = connection;
        }
    }
}

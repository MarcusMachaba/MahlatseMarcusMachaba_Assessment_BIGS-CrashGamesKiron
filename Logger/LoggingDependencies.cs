namespace Logger
{
    public static class LoggingDependencies
    {
        internal static string SqlConnectionString { get; private set; }

        public static void SetLog4NetDatabaseConnectionString(string connection)
        {
            SqlConnectionString = connection;
        }
    }
}

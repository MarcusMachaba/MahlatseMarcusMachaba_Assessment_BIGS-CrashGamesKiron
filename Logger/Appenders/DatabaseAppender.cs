using log4net.Appender;
using log4net.Core;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Logger.Appenders
{
    public class DatabaseAppender : AppenderSkeleton
    {
        private readonly string _connectionString;

        public DatabaseAppender()
        {
            _connectionString = LoggingDependencies.SqlConnectionString;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand("INSERT INTO Logs (Date, Thread, Level, Logger, Message, Exception, MachineName) VALUES (@Date, @Thread, @Level, @Logger, @Message, @Exception, @MachineName)", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@Date", SqlDbType.DateTime) { Value = loggingEvent.TimeStamp });
                        command.Parameters.Add(new SqlParameter("@Thread", SqlDbType.NVarChar, 255) { Value = loggingEvent.ThreadName });
                        command.Parameters.Add(new SqlParameter("@Level", SqlDbType.NVarChar, 50) { Value = loggingEvent.Level.Name });
                        command.Parameters.Add(new SqlParameter("@Logger", SqlDbType.NVarChar, 255) { Value = loggingEvent.LoggerName });
                        command.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar, -1) { Value = loggingEvent.RenderedMessage });
                        command.Parameters.Add(new SqlParameter("@Exception", SqlDbType.NVarChar, -1) { Value = loggingEvent.GetExceptionString() });
                        command.Parameters.Add(new SqlParameter("@MachineName", SqlDbType.NVarChar, 255) { Value = loggingEvent.LookupProperty("log4net:HostName") as string ?? Environment.MachineName });

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }
    }
}

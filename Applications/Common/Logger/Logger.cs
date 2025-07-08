using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using Logger.Appenders;
using System;
using System.IO;
using System.Reflection;

namespace Logger
{
    public class Logger
    {
        private readonly ILog logger;
        static Logger()
        {
            try
            {
                log4net.Repository.Hierarchy.Hierarchy repository = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository(Assembly.GetExecutingAssembly());
                if (File.Exists("log4net.xml"))
                {
                    XmlConfigurator.ConfigureAndWatch(repository, new FileInfo("log4net.xml"));
                }
                else
                {
                    PatternLayout patternLayout = new PatternLayout();
                    patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message - %property{log4net:HostName}%newline%exception";
                    patternLayout.ActivateOptions();

                    // Configure Rolling File Appender
                    RollingFileAppender rollingFileAppender = new RollingFileAppender();
                    rollingFileAppender.AppendToFile = true; //turn it off when done deving                       // Append to existing logs
                    rollingFileAppender.File = "logs\\Log-file.txt";                                // Log file path
                    rollingFileAppender.Layout = patternLayout;
                    rollingFileAppender.MaxSizeRollBackups = 5;                                     // Keep up to 5 backup files
                    rollingFileAppender.MaximumFileSize = "10MB";                                   // Maximum log file size
                    rollingFileAppender.RollingStyle = (RollingFileAppender.RollingMode)1;
                    rollingFileAppender.StaticLogFileName = true;                                   // Do not include timestamps in filenames
                    rollingFileAppender.ActivateOptions();

                    // Configure Console Appender
                    ConsoleAppender consoleAppender = new ConsoleAppender();
                    consoleAppender.Layout = patternLayout;
                    consoleAppender.ActivateOptions();

                    // Configure AdoNetAppender (Database logging)
                    //TODO dynamic CustomAdoNetAppender activation
                    DatabaseAppender msDatabaseAppender = new DatabaseAppender();
                    //((AppenderSkeleton)msDatabaseAppender).Layout = (ILayout)patternLayout;
                    msDatabaseAppender.ActivateOptions();


                    repository.Root.AddAppender(rollingFileAppender);
                    repository.Root.AddAppender(consoleAppender);
                    repository.Root.AddAppender(msDatabaseAppender);
                    repository.Root.Level = Level.Debug;
                    repository.Configured = true;
                    BasicConfigurator.Configure(repository);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private Logger(Type type) => logger = LogManager.GetLogger(type);

        public static Logger GetLogger(Type type) => new Logger(type);

        public void Debug(string message) => logger.Debug(message);

        public void Info(string message) => logger.Info(message);

        public void Info(Exception exception) => logger.Info(exception);

        public void Info(string message, Exception exception) => logger.Info(string.Format("{0}{1}{2}", message, Environment.NewLine, exception));

        public void Warn(string message) => logger.Warn(message);

        public void Warn(Exception exception) => logger.Warn(exception);

        public void Warn(string message, Exception exception) => logger.Warn(message, exception);

        public void Error(string message) => logger.Error(message);

        public void Error(Exception exception) => logger.Error(exception);

        public void Error(string message, Exception exception) => logger.Error(message, exception);

        public void Fatal(string message) => logger.Fatal(message);

        public void Fatal(Exception exception) => logger.Fatal(exception);

        public void Fatal(string message, Exception exception) => logger.Fatal(message, exception);
    }
}

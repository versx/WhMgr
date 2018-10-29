namespace WhMgr.Diagnostics
{
    using System;
    using System.IO;

    public class EventLogger : IEventLogger
    {
        #region Static Variables

        private static EventLogger _instance;
        private static readonly object _lock = new object();

        #endregion

        #region Properties

        public Action<LogType, string> LogHandler { get; set; }

        public static EventLogger GetLogger(string name = null)
        {
            if (_instance == null)
            {
                CreateLogsDirectory();

                _instance = new EventLogger();
                _instance.Info("Logging started...");
            }

            return _instance;
        }

        #endregion

        #region Constructor(s)

        public EventLogger()
        {
            LogHandler = new Action<LogType, string>(DefaultLogHandler);
            CreateLogsDirectory();
        }

        public EventLogger(Action<LogType, string> logHandler)
        {
            LogHandler = logHandler;
            CreateLogsDirectory();
        }

        #endregion

        #region Public Methods

        public void Trace(string format, params object[] args)
        {
            LogHandler(LogType.Trace, string.Format(format, args));
        }

        public void Debug(string format, params object[] args)
        {
            LogHandler(LogType.Debug, string.Format(format, args));
        }

        public void Info(string format, params object[] args)
        {
            LogHandler(LogType.Info, string.Format(format, args));
        }

        public void Warn(string format, params object[] args)
        {
            LogHandler(LogType.Warning, string.Format(format, args));
        }

        public void Error(string format, params object[] args)
        {
            LogHandler(LogType.Error, string.Format(format, args));
        }

        public void Error(Exception ex)
        {
            LogHandler(LogType.Error, ex.ToString());
        }

        #endregion

        #region Private Methods

        private void DefaultLogHandler(LogType logType, string message)
        {
            var msg = $"{DateTime.Now.ToShortTimeString()} [{logType.ToString().ToUpper()}] {message}";

            switch (logType)
            {
                case LogType.Debug:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogType.Trace:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }
            Console.WriteLine(msg);

            lock (_lock)
            {
                File.AppendAllText(Path.Combine(Strings.LogsFolder, DateTime.Now.ToString("yyyy-MM-dd") + ".log"), msg + Environment.NewLine);
            }
        }

        private static void CreateLogsDirectory()
        {
            if (!Directory.Exists(Strings.LogsFolder))
            {
                Directory.CreateDirectory(Strings.LogsFolder);
            }
        }

        #endregion
    }
}
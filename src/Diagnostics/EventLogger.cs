namespace T.Diagnostics
{
    using System;
    using System.IO;

    public class EventLogger : IEventLogger
    {
        const string LogFile = "logs.log";

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
        }

        public EventLogger(Action<LogType, string> logHandler)
        {
            LogHandler = logHandler;
        }

        #endregion

        #region Public Methods

        public void Trace(string format, params object[] args)
        {
            LogHandler(LogType.Trace, args.Length > 0 ? string.Format(format, args) : format);
        }

        public void Debug(string format, params object[] args)
        {
            LogHandler(LogType.Debug, args.Length > 0 ? string.Format(format, args) : format);
        }

        public void Info(string format, params object[] args)
        {
            LogHandler(LogType.Info, args.Length > 0 ? string.Format(format, args) : format);
        }

        public void Warn(string format, params object[] args)
        {
            LogHandler(LogType.Warning, args.Length > 0 ? string.Format(format, args) : format);
        }

        public void Error(string format, params object[] args)
        {
            LogHandler(LogType.Error, args.Length > 0 ? string.Format(format, args) : format);
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
                File.AppendAllText(LogFile, msg + Environment.NewLine);
            }
        }

        #endregion
    }
}
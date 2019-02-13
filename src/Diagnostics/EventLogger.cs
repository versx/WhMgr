namespace WhMgr.Diagnostics
{
    using System;
    using System.IO;
    using System.Threading;

    public class EventLogger : IEventLogger
    {
        #region Static Variables

        private static EventLogger _instance;
        //private static readonly object _lock = new object();
        private static readonly EventWaitHandle _waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, Strings.BotName + new Random().Next(10000, 90000));

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
            LogHandler(LogType.Error, ex?.ToString());
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

            //lock (_lock)
            //{
            _waitHandle.WaitOne();
            File.AppendAllText(Path.Combine(Strings.LogsFolder, $"{Program.ManagerName}_{DateTime.Now.ToString("yyyy-MM-dd")}.log"), msg + Environment.NewLine);
            _waitHandle.Set();
            //}
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
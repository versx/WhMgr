namespace WhMgr.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Event logger diagnostics class
    /// </summary>
    public class EventLogger : IEventLogger
    {
        private const string DefaultLoggerName = "default";

        #region Static Variables

        private static readonly Dictionary<string, EventLogger> _instances = new Dictionary<string, EventLogger>();
#if Windows
        private static readonly EventWaitHandle _waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, Strings.BotName + new Random().Next(10000, 90000));
#endif

#endregion

        #region Properties

        /// <summary>
        /// Gets or sets the logger instance name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the log handler callback
        /// </summary>
        public Action<LogType, string> LogHandler { get; set; }

        /// <summary>
        /// Gets the event logger class by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EventLogger GetLogger(string name = null)
        {
            var instanceName = (name ?? DefaultLoggerName).ToLower();
            if (_instances.ContainsKey(instanceName))
            {
                return _instances[instanceName];
            }

            _instances.Add(instanceName, new EventLogger(instanceName));
            return _instances[instanceName];
        }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Instantiate a new <see cref="EventLogger"/> class
        /// </summary>
        public EventLogger() 
            : this(DefaultLoggerName)
        {
        }

        /// <summary>
        /// Instantiate a new <see cref="EventLogger"/> class by name
        /// </summary>
        /// <param name="name">Name to set</param>
        public EventLogger(string name)
        {
            Name = name;
            LogHandler = new Action<LogType, string>(DefaultLogHandler);
            CreateLogsDirectory();
        }

        /// <summary>
        /// Instantiate a new <see cref="EventLogger"/> class by name and log handler
        /// </summary>
        /// <param name="name">Name to set</param>
        /// <param name="logHandler">Event logger handler callback</param>
        public EventLogger(string name, Action<LogType, string> logHandler)
        {
            Name = name;
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
            var msg = $"{DateTime.Now.ToShortTimeString()} [{logType.ToString().ToUpper()}] [{Name.ToUpper()}] {message}";

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

#if Windows
            _waitHandle.WaitOne();
#endif
            File.AppendAllText(Path.Combine(Strings.LogsFolder, $"{Program.ManagerName}_{DateTime.Now:yyyy-MM-dd}.log"), msg + Environment.NewLine);
#if Windows
            _waitHandle.Set();
#endif
        }

        private static void CreateLogsDirectory()
        {
            if (Directory.Exists(Strings.LogsFolder))
                return;

            Directory.CreateDirectory(Strings.LogsFolder);
        }

        #endregion
    }
}
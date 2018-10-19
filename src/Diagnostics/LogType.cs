namespace T.Diagnostics
{
    /// <summary>
    /// The log level to use.
    /// </summary>
    public enum LogType : uint
    {
        /// <summary>
        /// Normal log level.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Successful log level.
        /// </summary>
        Success,

        /// <summary>
        /// Warning log level.
        /// </summary>
        Warning,

        /// <summary>
        /// Error log level.
        /// </summary>
        Error,

        /// <summary>
        /// Debug log level.
        /// </summary>
        Debug,

        /// <summary>
        /// Information log level.
        /// </summary>
        Info,

        /// <summary>
        /// Trace log level.
        /// </summary>
        Trace
    }
}
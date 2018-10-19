namespace T.Diagnostics
{
    using System;

    public interface IEventLogger
    {
        void Trace(string format, params object[] args);

        void Debug(string format, params object[] args);

        void Info(string format, params object[] args);

        void Warn(string format, params object[] args);

        void Error(string format, params object[] args);

        void Error(Exception ex);
    }
}
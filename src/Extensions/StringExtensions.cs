namespace WhMgr.Extensions
{
    using System;

    using WhMgr.Diagnostics;

    public static class StringExtensions
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        public static string FormatText(this string text, params object[] args)
        {
            try
            {
                var msg = text;
                for (var i = 0; i < args.Length; i++)
                {
                    if (string.IsNullOrEmpty(msg))
                        continue;

                    if (args == null)
                        continue;

                    if (args[i] == null)
                    {
                        msg = msg.Replace("{" + i + "}", null);
                        continue;
                    }

                    msg = msg.Replace("{" + i + "}", args[i].ToString());
                    msg = msg.Replace("\n", "\n");
                }
                return msg;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return string.Format(text, args);
            }
        }
    }
}
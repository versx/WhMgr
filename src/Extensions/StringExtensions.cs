namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;

    using WhMgr.Diagnostics;

    public static class StringExtensions
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("STRING_EXTENSIONS");

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

        public static IEnumerable<string> SplitInParts(this string s, int partLength)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException(nameof(s));

            if (partLength <= 0)
                throw new ArgumentException("Part length must be a positive number.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
            {
                yield return s.Substring(i, Math.Min(partLength, s.Length));
            }
        }

        public static string ToUppercaseFirstLetter(this string text)
        {
            var firstLetter = text[0].ToString().ToUpper();
            var remainingLetters = text.Substring(1, text.Length - 1);
            return firstLetter + remainingLetters;
        }
    }
}
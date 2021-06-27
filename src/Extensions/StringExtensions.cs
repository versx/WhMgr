namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using WhMgr.Diagnostics;

    public static class StringExtensions
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("STRING_EXTENSIONS", Program.LogLevel);

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
                }
                return msg;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return string.Format(text, args);
            }
        }

        // TODO: Use dynamic object instead of tuple array for ease
        public static string FormatText(this string text, (string, string)[] args)
        {
            /*
            (t.prototype.applyFormatting = function (t, n, e) {
                var r, l;
                for (r in e) (l = new RegExp("%{" + r + "}", "g")), (t = t.replace(l, e[r]));
                return t;
            }),
             */
            foreach (var (key, obj) in args)
            {
                var pattern = "%{" + key + "}";
                text = Regex.Replace(text, pattern, (m) => obj);
            }
            return text;
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

        public static List<string> RemoveSpaces(this string value)
        {
            return value.Replace(", ", ",")
                        .Replace(" ,", ",")
                        .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
        }
    }
}
namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class StringExtensions
    {
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
                Console.WriteLine($"Error: {ex}");
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
    }
}
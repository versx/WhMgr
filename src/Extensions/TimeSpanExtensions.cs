namespace WhMgr.Extensions
{
    using System;

    public static class TimeSpanExtensions
    {
        public static string ToReadableString(this TimeSpan span, bool shortForm = false)
        {
            string formatted = string.Format
            (
                "{0}{1}{2}{3}",
                span.Duration().Days > 0 ? shortForm ? string.Format("{0:0}d, ", span.Days) : string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? "" : "s") : "",
                span.Duration().Hours > 0 ? shortForm ? string.Format("{0:0}h, ", span.Hours) : string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? "" : "s") : "",
                span.Duration().Minutes > 0 ? shortForm ? string.Format("{0:0}m, ", span.Minutes) : string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? "" : "s") : "",
                span.Duration().Seconds > 0 ? shortForm ? string.Format("{0:0}s", span.Seconds) : string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? "" : "s") : ""
            );

            if (formatted.EndsWith(", ", StringComparison.Ordinal))
                formatted = formatted[0..^2];

            if (string.IsNullOrEmpty(formatted))
                formatted = "0 seconds";

            return formatted;
        }

        public static string ToReadableStringNoSeconds(this TimeSpan span)
        {
            string formatted = string.Format
            (
                "{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0}d ", span.Days) : "",
                span.Duration().Hours > 0 ? string.Format("{0:0}h ", span.Hours) : "",
                span.Duration().Minutes > 0 ? string.Format("{0:0}m ", span.Minutes) : "",
                span.Duration().Minutes == 0 ? span.Duration().Seconds > 0 ? string.Format("{0:0}s", span.Seconds) : "" : ""
            );

            if (string.IsNullOrEmpty(formatted))
                formatted = "0s";

            return formatted.TrimEnd(' ');
        }
    }
}
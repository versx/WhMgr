namespace WhMgr.Web.Events
{
    using System;
    using System.Diagnostics.Tracing;

    [EventSource(Name = Strings.BotName + " EventCounter")]
    public sealed class MinimalEventCounterSource : EventSource
    {
        public static readonly MinimalEventCounterSource Logger = new();

        private EventCounter _requestCounter;

        private MinimalEventCounterSource()
        {
            _requestCounter = new EventCounter("request-time", this)
            {
                DisplayName = "Request Processing Time",
                DisplayUnits = "ms"
            };
        }

        public void Request(string url, long elapsedMilliseconds)
        {
            WriteEvent(1, url, elapsedMilliseconds);
            Console.WriteLine($"Request {url} time elapsed: {elapsedMilliseconds} ms");
            _requestCounter?.WriteMetric(elapsedMilliseconds);
        }

        protected override void Dispose(bool disposing)
        {
            _requestCounter?.Dispose();
            _requestCounter = null;

            base.Dispose(disposing);
        }
    }
}
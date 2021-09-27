namespace WhMgr.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;

    public class WebhookQueueItem
    {
        public string Url { get; set; }

        public string Json { get; set; }

        public int RetryAfter { get; set; }
    }

    public static class NetUtils
    {
        private static readonly Queue<WebhookQueueItem> _backlogQueue = new();
        private static readonly System.Timers.Timer _timer = new();

        static NetUtils()
        {
            _timer.Elapsed += (sender, e) => HandleBacklogQueue();
            _timer.Interval = 200;
            _timer.Start();
        }

        /// <summary>
        /// Sends webhook data
        /// </summary>
        /// <param name="webhookUrl"></param>
        /// <param name="json"></param>
        public static void SendWebhook(string webhookUrl, string json)
        {
            using (var wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                //wc.Headers.Add(HttpRequestHeader.Authorization, "Bot base64_auth_token");
                wc.Headers.Add(HttpRequestHeader.UserAgent, Strings.BotName);
                try
                {
                    var resp = wc.UploadString(webhookUrl, json);
                    //Console.WriteLine($"Response: {resp}");
                    Thread.Sleep(200);
                }
                catch (WebException ex)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    switch ((int)(resp?.StatusCode ?? 0))
                    {
                        //https://discordapp.com/developers/docs/topics/rate-limits
                        case 429:
                            HandleRateLimitedRequest(resp, webhookUrl, json);
                            break;
                        case 400:
                            Console.WriteLine($"Failed to send webhook: {ex}");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Get(string url)
        {
            using (var wc = new WebClient())
            {
                wc.Proxy = null;
                try
                {
                    return wc.DownloadString(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to download data from {url}: {ex}");
                }
            }

            return null;
        }

        private static void HandleRateLimitedRequest(HttpWebResponse response, string url, string json)
        {
            if (_backlogQueue.Count > 0)
            {
                Console.WriteLine($"[Webhook] RATE LIMITED: {url} Added to backlog queue, currently {_backlogQueue.Count:N0} items long.");
            }

            var retryAfter = response.Headers["Retry-After"];
            //var limit = resp.Headers["X-RateLimit-Limit"];
            //var remaining = resp.Headers["X-RateLimit-Remaining"];
            //var reset = resp.Headers["X-RateLimit-Reset"];
            if (!int.TryParse(retryAfter, out var retry))
                return;

            //Thread.Sleep(retry);
            //SendWebhook(webhookUrl, json);
            _backlogQueue.Enqueue(new WebhookQueueItem
            {
                Url = url,
                Json = json,
                RetryAfter = retry,
            });
        }

        private static void HandleBacklogQueue()
        {
            if (_backlogQueue.Count == 0)
                return;

            var item = _backlogQueue.Dequeue();
            Thread.Sleep(item.RetryAfter);

            SendWebhook(item.Url, item.Json);
        }
    }
}
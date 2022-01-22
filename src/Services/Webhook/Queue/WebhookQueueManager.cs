namespace WhMgr.Services.Webhook.Queue
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using WhMgr.Extensions;

    // TODO: Convert to HostedService
    public class WebhookQueueManager : IWebhookQueueManager
    {
        private readonly Queue<WebhookQueueItem> _backlogQueue = new();
        private readonly System.Timers.Timer _timer = new();

        // TODO: Singleton instance

        public WebhookQueueManager()
        {
            _timer.Elapsed += (sender, e) => HandleBacklogQueue();
            _timer.Interval = 200;

            Start();
        }

        public void Start()
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }

        public void Stop()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
        }

        /// <summary>
        /// Sends webhook data
        /// </summary>
        /// <param name="webhookUrl"></param>
        /// <param name="json"></param>
        public async Task SendWebhook(string webhookUrl, string json)
        {
            try
            {
                using var client = new HttpClient();
                var requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(webhookUrl),
                    Headers =
                        {
                            { HttpRequestHeader.UserAgent.ToString(), Strings.BotName },
                        },
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
                var response = client.SendAsync(requestMessage).Result;
                /*
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                //data.Headers.Add("UserAgent", Strings.BotName);
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue(Strings.BotName));
                await client.PostAsync(webhookUrl, data);
                */
                await Task.CompletedTask;
            }
            catch (WebException ex)
            {
                var response = (HttpWebResponse)ex.Response;
                switch ((int)(response?.StatusCode ?? 0))
                {
                    //https://discordapp.com/developers/docs/topics/rate-limits
                    case 429:
                        HandleRateLimitedRequest(response, webhookUrl, json);
                        break;
                    case 400:
                        Console.WriteLine($"Failed to send webhook: {webhookUrl}\nJson: {json}\nError: {ex}");
                        break;
                    default:
                        Console.WriteLine($"Failed to send webhook with status: {response?.StatusCode}\nUrl: {webhookUrl}\nError: {ex}");
                        break;
                }
            }
        }

        private void HandleRateLimitedRequest(HttpWebResponse response, string url, string json)
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

            _backlogQueue.Enqueue(new WebhookQueueItem
            {
                Url = url,
                Json = json,
                RetryAfter = retry,
            });
        }

        private void HandleBacklogQueue()
        {
            if (_backlogQueue.Count == 0)
                return;

            var queueChunkSize = 10;
            var items = _backlogQueue.DequeueChunk(queueChunkSize);
            var tasks = items.Select(item => Task.Factory.StartNew(() =>
            {
                if (item.RetryAfter > 0)
                {
                    // Wait rate limit timeout
                    Thread.Sleep(item.RetryAfter);
                }
                return SendWebhook(item.Url, item.Json);
            }));
            Task.WaitAll(tasks.ToArray());
            /*
            if (item.RetryAfter > 0)
            {
                // Wait rate limit timeout
                Thread.Sleep(item.RetryAfter);
            }
            await SendWebhook(item.Url, item.Json);
            */
        }
    }
}
namespace WhMgr.Utilities
{
    using System;
    using System.Net;
    using System.Threading;

    public static class NetUtils
    {
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
                //wc.Headers.Add(HttpRequestHeader.UserAgent, "");
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
                            Console.WriteLine("RATE LIMITED");
                            var retryAfter = resp.Headers["Retry-After"];
                            //var limit = resp.Headers["X-RateLimit-Limit"];
                            //var remaining = resp.Headers["X-RateLimit-Remaining"];
                            //var reset = resp.Headers["X-RateLimit-Reset"];
                            if (!int.TryParse(retryAfter, out var retry))
                                return;

                            Thread.Sleep(retry);
                            SendWebhook(webhookUrl, json);
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
    }
}
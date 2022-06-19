namespace WhMgr.Utilities
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public static class NetUtils
    {
        public static string Get(string url)
        {
            return GetAsync(url).Result;
        }

        /// <summary>
        /// Sends a HTTP GET request to the specified url.
        /// </summary>
        /// <param name="url">Url to send the request to.</param>
        /// <returns>Returns the response string of the HTTP GET request.</returns>
        public static async Task<string> GetAsync(string url)
        {
            try
            {
                using var client = new HttpClient();
                var mime = "application/json";
                client.DefaultRequestHeaders.Add(HttpRequestHeader.Accept.ToString(), mime);
                client.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), mime);
                return await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download data from {url}: {ex}");
            }
            return null;
        }

        public static string Post(string url, string payload)
        {
            return PostAsync(url, payload).Result;
        }

        /// <summary>
        /// Sends a HTTP POST request to the specified url with JSON payload.
        /// </summary>
        /// <param name="url">Url to send the request to.</param>
        /// <param name="payload">JSON payload that will be sent in the request.</param>
        /// <returns>Returns the response string of the HTTP POST request.</returns>
        public static async Task<string> PostAsync(string url, string payload, string userAgent = Strings.BotName)
        {
            try
            {
                using var client = new HttpClient();
                var mime = "application/json";
                var requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Headers =
                    {
                        { HttpRequestHeader.UserAgent.ToString(), userAgent },
                        { HttpRequestHeader.Accept.ToString(), mime },
                        { HttpRequestHeader.ContentType.ToString(), mime },
                    },
                    Content = new StringContent(payload, Encoding.UTF8, mime),
                };
                var response = await client.SendAsync(requestMessage);
                var responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to post data to {url}: {ex}");
            }
            return null;
        }
    }
}
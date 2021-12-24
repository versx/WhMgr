namespace WhMgr.Utilities
{
    using System;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Web;

    using WhMgr.Configuration;
    using WhMgr.Extensions;

    /// <summary>
    /// Url shortener class using yourls.org
    /// </summary>
    public class UrlShortener
    {
        public UrlShortenerConfig Configuration { get; }

        public UrlShortener(UrlShortenerConfig config)
        {
            Configuration = config;
        }

        /// <summary>
        /// Creates a short url from the url provided
        /// </summary>
        /// <param name="baseApiUrl">Yourls.org endpoint base url with signature parameter</param>
        /// <param name="url">URL address to shorten</param>
        /// <param name="action">Action to invoke</param>
        /// <param name="format">Response text format</param>
        /// <returns>Returns the shortened URL address</returns>
        public string Create(string url)
        {
            // Check if service enabled or if base `yourls` url not set, return original url
            if (!Configuration.Enabled || string.IsNullOrEmpty(Configuration.ApiUrl))
                return url;

            try
            {
                var encodedUrl = HttpUtility.UrlEncode(url);
                var sb = new StringBuilder();
                sb.Append(Configuration.ApiUrl);
                sb.Append("&action=");
                sb.Append(Configuration.Action);
                sb.Append("&url=");
                sb.Append(encodedUrl);
                sb.Append("&format=");
                sb.Append(Configuration.Format);
                var apiUrl = sb.ToString();
                var json = NetUtils.Get(apiUrl);
                if (string.IsNullOrEmpty(json))
                    return url;

                var obj = json.FromJson<UrlShortenerResponse>();
                return obj?.ShortUrl;
            }
            catch (Exception)
            {
                return url;
            }
        }
    }

    /*
{
    "url": {
        "keyword":"1",
        "url":"https://www.google.com/maps?q=34.01,-117.01",
        "title":"Google Maps",
        "date":"2019-05-25 04:48:55",
        "ip":"172.89.225.76"
    },
    "status":"success",
    "message":"https://www.google.com/maps?q=34.01,-117.01[...] added to database",
    "title":"Google Maps",
    "shorturl":"https://site.com/u/1",
    "statusCode":200
}
    */
    public class UrlShortenerResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("shorturl")]
        public string ShortUrl { get; set; }

        [JsonPropertyName("url")]
        public UrlShortenerResponseUrl Url { get; set; }
    }

    public class UrlShortenerResponseUrl
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("ip")]
        public string IpAddress { get; set; }
    }
}
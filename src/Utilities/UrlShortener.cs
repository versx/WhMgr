namespace WhMgr.Utilities
{
    using System;
    using System.Text.Json.Serialization;
    using System.Web;

    using WhMgr.Extensions;

    /// <summary>
    /// Url shortener class using yourls.org
    /// </summary>
    public class UrlShortener
    {
        /*
{
    "url": {
    "keyword":"1",
    "url":"https://www.google.com/maps?q=34.1351088673568,-118.051129828759",
    "title":"Google Maps",
    "date":"2019-05-25 04:48:55",
    "ip":"172.89.225.76"
    },
    "status":"success",
    "message":"https://www.google.com/maps?q=34.1351088673568,-118.051[...] added to database",
    "title":"Google Maps",
    "shorturl":"https://site.com/u/1",
    "statusCode":200
}
        */

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("shorturl")]
        public string ShortUrl { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Creates a short url from the url provided
        /// </summary>
        /// <param name="baseApiUrl">Yourls.org endpoint base url with signature parameter</param>
        /// <param name="url">URL address to shorten</param>
        /// <returns>Returns the shortened URL address</returns>
        public static string CreateShortUrl(string baseApiUrl, string url)
        {
            // If base `yourls` url not set, return original url
            if (string.IsNullOrEmpty(baseApiUrl))
                return url;

            try
            {
                var apiUrl = $"{baseApiUrl}&action=shorturl&url={HttpUtility.UrlEncode(url)}&format=json";
                var json = NetUtils.Get(apiUrl);
                if (string.IsNullOrEmpty(json))
                    return url;

                var obj = json.FromJson<UrlShortener>();
                return obj?.ShortUrl;
            }
            catch (Exception)
            {
                return url;
            }
        }
    }
}
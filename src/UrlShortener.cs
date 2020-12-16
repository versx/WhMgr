namespace WhMgr
{
    using System;
    using System.Web;

    using Newtonsoft.Json;

    using WhMgr.Utilities;

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

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("shorturl")]
        public string ShortUrl { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Creates a short url from the url provided
        /// </summary>
        /// <param name="baseApiUrl"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string CreateShortUrl(string baseApiUrl, string url)
        {
            // If base `yourls` url not set, return original url
            if (string.IsNullOrEmpty(baseApiUrl))
                return url;

            try
            {
                var apiUrl = $"{baseApiUrl}&action=shorturl&url={HttpUtility.UrlEncode(url)}&format=json";
                var json = NetUtil.Get(apiUrl);
                if (string.IsNullOrEmpty(json))
                    return url;

                var obj = JsonConvert.DeserializeObject<UrlShortener>(json);
                return obj?.ShortUrl;
            }
            catch (Exception)
            {
                return url;
            }
        }
    }
}
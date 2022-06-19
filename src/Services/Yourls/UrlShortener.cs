namespace WhMgr.Services.Yourls
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Yourls.Models;
    using WhMgr.Utilities;

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
        public async Task<string> CreateAsync(string url)
        {
            // Check if service enabled or if base `yourls` url not set, return original url
            if (!Configuration.Enabled || string.IsNullOrEmpty(Configuration.ApiUrl))
                return url;

            try
            {
                var encodedUrl = HttpUtility.UrlEncode(url);
                var sb = new StringBuilder();
                sb.Append(Configuration.ApiUrl);
                sb.Append("?signature=");
                sb.Append(Configuration.Signature);
                sb.Append("&action=");
                sb.Append(Configuration.Action);
                sb.Append("&url=");
                sb.Append(encodedUrl);
                sb.Append("&format=");
                sb.Append(Configuration.Format);
                var apiUrl = sb.ToString();
                var json = await NetUtils.GetAsync(apiUrl);
                if (string.IsNullOrEmpty(json))
                    return url;

                var obj = json.FromJson<UrlShortenerResponse>();
                return obj?.ShortUrl ?? url;
            }
            catch (Exception)
            {
                return url;
            }
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
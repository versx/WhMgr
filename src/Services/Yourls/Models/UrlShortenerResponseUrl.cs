namespace WhMgr.Services.Yourls.Models
{
    using System;
    using System.Text.Json.Serialization;

    public class UrlShortenerResponseUrl
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("ip")]
        public string IpAddress { get; set; }
    }
}
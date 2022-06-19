namespace WhMgr.Services.Yourls.Models
{
    using System.Text.Json.Serialization;

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
}
namespace WhMgr.Alarms.Alerts
{
    using Newtonsoft.Json;

    public class AlertMessageSettings
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }
}
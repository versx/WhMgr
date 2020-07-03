namespace WhMgr.Data.Models.Discord
{
    using Newtonsoft.Json;

    public class DiscordEmbedImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
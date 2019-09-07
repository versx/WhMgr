namespace WhMgr.Data.Models.Discord
{
    using Newtonsoft.Json;

    public class DiscordEmbedFooter
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
    }
}